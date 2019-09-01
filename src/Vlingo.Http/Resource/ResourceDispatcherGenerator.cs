// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Common.Compiler;

using static Vlingo.Common.Compiler.DynaFile;
using static Vlingo.Common.Compiler.DynaNaming;

namespace Vlingo.Http.Resource
{
    internal class ResourceDispatcherGenerator
    {
        internal sealed class Result
        {
            internal Result(
                string fullyQualifiedClassName,
                string className,
                string source,
                FileInfo sourceFile)
            {
                FullyQualifiedClassName = fullyQualifiedClassName;
                ClassName = className;
                Source = source;
                SourceFile = sourceFile;
            }

            public string FullyQualifiedClassName { get; }
            public string ClassName { get; }
            public string Source { get; }
            public FileInfo SourceFile { get; }
        }
        
        private readonly ILogger _logger;
        private readonly bool _persist;
        private readonly IList<Action> _actions;
        private readonly DirectoryInfo _rootOfGenerated;

        internal DynaType Type { get; }

        public static ResourceDispatcherGenerator ForMain(IList<Action> actions, bool persist/*, ILogger logger*/)
        {
            var classPath = new List<FileInfo>
            {
                new FileInfo(Properties.Instance.GetProperty("resource.dispatcher.generated.classes.main", RootOfMainClasses))
            };
            var type = DynaType.Main;
            var rootOfGenerated = RootOfGeneratedSources(type);

            return new ResourceDispatcherGenerator(actions, classPath, rootOfGenerated, type, persist/*, logger*/);
        }

        public static ResourceDispatcherGenerator ForTest(IList<Action> actions, bool persist/*, ILogger logger*/)
        {
            var classPath = new List<FileInfo>
            {
                new FileInfo(Properties.Instance.GetProperty("resource.dispatcher.generated.classes.test", RootOfTestClasses))
            };
            var type = DynaType.Test;
            var rootOfGenerated = RootOfGeneratedSources(type);

            return new ResourceDispatcherGenerator(actions, classPath, rootOfGenerated, type, persist/*, logger*/);
        }

        public Result GenerateFor(Type handlerProtocol)
        {
            Console.WriteLine("vlingo/http: Generating handler dispatcher for  " + (Type == DynaType.Main ? "main" : "test") + ": " + handlerProtocol.Name);
            try
            {
                var dispatcherClassSource = DispatcherClassSource(handlerProtocol);
                var fullyQualifiedClassName = FullyQualifiedClassNameFor(handlerProtocol, ConfigurationResource<ResourceHandler>.DispatcherSuffix);
                var relativeTargetFile = ToFullPath(fullyQualifiedClassName);
                var sourceFile = _persist ?
                    PersistProxyClassSource(fullyQualifiedClassName, relativeTargetFile, dispatcherClassSource) :
                    new FileInfo(relativeTargetFile);

                return new Result(fullyQualifiedClassName, ClassNameFor(handlerProtocol, ConfigurationResource<ResourceHandler>.DispatcherSuffix), dispatcherClassSource, sourceFile);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Cannot generate dispatcher class for: {handlerProtocol.Name}", ex);
            }
        }

        private static DirectoryInfo RootOfGeneratedSources(DynaType type)
            => type == DynaType.Main ?
                new DirectoryInfo(Properties.Instance.GetProperty("resource.dispatcher.generated.sources.main", GeneratedSources)) :
                new DirectoryInfo(Properties.Instance.GetProperty("resource.dispatcher.generated.sources.test", GeneratedTestSources));

        private ResourceDispatcherGenerator(IList<Action> actions, IList<FileInfo> rootOfClasses, DirectoryInfo rootOfGenerated, DynaType type, bool persist/*, ILogger logger*/)
        {
            _actions = actions;
            _rootOfGenerated = rootOfGenerated;
            Type = type;
            _persist = persist;
            // _logger = logger;
        }

        private string ClassStatement(Type handlerInterface)
            => string.Format("public class {0} : ConfigurationResource<{1}>\n{{",
                ClassNameFor(handlerInterface, ConfigurationResource<ResourceHandler>.DispatcherSuffix),
                GetSimpleTypeName(handlerInterface));

        private string Constructor(Type protocolInterface)
        {
            var builder = new StringBuilder();

            var constructorName = ClassNameFor(protocolInterface, ConfigurationResource<ResourceHandler>.DispatcherSuffix);
            if (protocolInterface.IsGenericType)
            {
                constructorName = constructorName.Substring(0, constructorName.IndexOf('<'));
            }

            var signature = string.Format("  public {0}(string name, Type resourceHandlerClass, int handlerPoolSize, IEnumerable<Vlingo.Http.Resource.Action> actions)" +
                                          " : this(name, resourceHandlerClass, handlerPoolSize, actions)", constructorName);

            builder
                .Append(signature).Append("\n")
                .Append("  {\n")
                .Append("  }");

            return builder.ToString();
        }

        private string ImportStatements()
        {
            var namespaces = new HashSet<string>();
            namespaces.Add("System");
            namespaces.Add("System.Collections.Generic");
            namespaces.Add(typeof(Context).Namespace);
            namespaces.Add(typeof(ResourceHandler).Namespace);
            namespaces.Add(typeof(AtomicBoolean).Namespace); // Vlingo.Common

            return string.Join("\n", namespaces.Select(x => $"using {x};"));
            
        }
        
        private string ActionCase(Action action)
        {
            var builder = new StringBuilder();
    
            builder.Append("      case ").Append(action.Id).Append(": // ").Append(action.Method).Append(" ").Append(action.Uri).Append(" ").Append(action.OriginalTo).Append("\n");
            builder.Append("        consumer = handler => handler.").Append(AsExpression(action.To)).Append(";\n");
            builder.Append("        PooledHandler.HandleFor(context, consumer);\n");
            builder.Append("        break;\n");
    
            return builder.ToString();
        }
        
        private String AsExpression(Action.ToSpec to)
        {
            var builder = new StringBuilder();
    
            builder.Append(to.MethodName).Append("(");
    
            var separator = "";
            int parameterIndex = 0;

            foreach (var parameter in to.Parameters)
            {
                builder.Append(separator).Append("(").Append(parameter.Type).Append(") ").Append("mappedParameters.Mapped[(" + parameterIndex + ")].Value");
                ++parameterIndex;
                separator = ", ";
            }

            builder.Append(")");
    
            return builder.ToString();
        }

        private string MethodDefinition(Type handlerType)
        {
            var builder = new StringBuilder();
    
            builder
                .Append("  public override void DispatchToHandlerWith(Context context, Vlingo.Http.Resource.Action.MappedParameters mappedParameters) {\n")
                .Append("    Action<" + GetSimpleTypeName(handlerType) + "> consumer = null;\n")
                .Append("\n")
                .Append("    try {\n")
                .Append("      switch (mappedParameters.ActionId) {\n");

            foreach (var action in _actions)
            {
                builder.Append(ActionCase(action));
            }

            builder
                .Append("      }\n")
                .Append("    } catch (Exception e) {\n")
                .Append("      throw new IllegalArgumentException(\"Action mismatch: Request: \" + context.request + \"Parameters: \" + mappedParameters);\n")
                .Append("    }\n")
                .Append("  }\n");
    
            return builder.ToString();
        }

        private string NamespaceStatement(Type protocolInterface, bool hasNamespace) => hasNamespace ? $"namespace {protocolInterface.Namespace}\n{{" : string.Empty;

        private FileInfo PersistProxyClassSource(string fullyQualifiedClassName, string relativePathToClass, string proxyClassSource)
        {
            var pathToGeneratedSource = ToNamespacePath(fullyQualifiedClassName);
            var dir = new DirectoryInfo(_rootOfGenerated + pathToGeneratedSource);

            if (!dir.Exists)
            {
                dir.Create();
            }

            var pathToSource = _rootOfGenerated + relativePathToClass + ".cs";

            return PersistDynaClassSource(pathToSource, proxyClassSource);
        }

        private string DispatcherClassSource(Type handlerType)
        {
            var hasNamespace = !string.IsNullOrWhiteSpace(handlerType.Namespace);
            var builder = new StringBuilder();
            builder
                .Append(ImportStatements()).Append("\n")
                .Append(NamespaceStatement(handlerType, hasNamespace)).Append("\n")
                .Append(ClassStatement(handlerType)).Append("\n")
                .Append(Constructor(handlerType)).Append("\n")
                .Append(MethodDefinition(handlerType)).Append("\n")
                .Append("}\n");
            if (hasNamespace)
            {
                builder.Append("}\n");
            }

            return builder.ToString();
        }

        private static readonly IDictionary<Type, string> SimpleTypeNames = new Dictionary<Type, string>
        {
            [typeof(void)] = "void",
            [typeof(object)] = "object",
            [typeof(string)] = "string",
            [typeof(bool)] = "bool",
            [typeof(byte)] = "byte",
            [typeof(sbyte)] = "sbyte",
            [typeof(char)] = "char",
            [typeof(decimal)] = "decimal",
            [typeof(double)] = "double",
            [typeof(float)] = "float",
            [typeof(int)] = "int",
            [typeof(uint)] = "uint",
            [typeof(long)] = "long",
            [typeof(ulong)] = "ulong",
            [typeof(short)] = "short",
            [typeof(ushort)] = "ushort"
        };

        private string GetSimpleTypeName(Type type)
        {
            if(SimpleTypeNames.ContainsKey(type))
            {
                return SimpleTypeNames[type];
            }

            if(Nullable.GetUnderlyingType(type) != null)
            {
                return GetSimpleTypeName(Nullable.GetUnderlyingType(type)) + "?";
            }

            if (type.IsGenericType)
            {
                var typeName = type.FullName ?? type.Name;
                var name = typeName.Substring(0, typeName.IndexOf('`'));
                var genericTypeDeclaration = string.Join(", ", type.GetGenericArguments().Select(GetSimpleTypeName));
                return $"{name}<{genericTypeDeclaration}>";
            }

            return type.FullName ?? type.Name;
        }
    }
}
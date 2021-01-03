// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Vlingo.Actors;
using Vlingo.Common.Compiler;

using static Vlingo.Common.Compiler.DynaFile;

namespace Vlingo.Http.Resource
{
    public abstract class ConfigurationResource : Resource, IConfigurationResource
    {
        public const string DispatcherSuffix = "Dispatcher";

        private static readonly DynaClassLoader ClassLoader = new DynaClassLoader();
        private static readonly DynaCompiler DynaCompiler = new DynaCompiler();

        public Type ResourceHandlerClass { get; }
        public IReadOnlyList<Action> Actions { get; }

        public static IConfigurationResource Defining(
            string resourceName,
            Type resourceHandlerClass,
            int handlerPoolSize,
            IList<Action> actions,
            ILogger logger)
            => NewResourceFor(resourceName, resourceHandlerClass, handlerPoolSize, actions, logger);

        internal static IConfigurationResource NewResourceFor(
            string resourceName,
            Type resourceHandlerType,
            int handlerPoolSize,
            IList<Action> actions,
            ILogger logger)
        {
            AssertSaneActions(actions);

            try
            {
                var fullyQualifiedTypeName = DynaNaming.FullyQualifiedClassNameFor(resourceHandlerType, DispatcherSuffix);
                var lookupTypeName = DynaNaming.FullyQualifiedClassNameFor(resourceHandlerType, DispatcherSuffix, true);

                Type? resourceClass;
                try
                {
                    // this check is done primarily for testing to prevent duplicate class mimeType in class loader
                    resourceClass = Type.GetType(fullyQualifiedTypeName);
                    if (resourceClass == null)
                    {
                        if (TryLoadAlreadyGeneratedAssembly(resourceHandlerType, out var assembly))
                        {
                            resourceClass = assembly?.GetType(fullyQualifiedTypeName, true);
                        }
                        else
                        {
                            resourceClass = Assembly.GetCallingAssembly().GetType(fullyQualifiedTypeName, true);
                        }
                    }
                }
                catch
                {
                    resourceClass = TryGenerateCompile(resourceHandlerType, fullyQualifiedTypeName, lookupTypeName, actions, logger);
                }

                var ctorParams = new object[] { resourceName, resourceHandlerType, handlerPoolSize, actions };
                foreach (var ctor in resourceClass!.GetConstructors())
                {
                    if (ctor.GetParameters().Length == ctorParams.Length)
                    {
                        var resourceDispatcher = (IConfigurationResource)ctor.Invoke(ctorParams);
                        return resourceDispatcher;
                    }
                }
                return (IConfigurationResource)Activator.CreateInstance(resourceClass)!;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Cannot create a resource from resource handler {resourceHandlerType.Name} because: {e.Message}");
            }
        }

        internal static Type? NewResourceHandlerTypeFor(string? resourceHandlerTypeName)
        {
            try
            {
                Type? resourceHandlerClass = TypeLoader.Load(resourceHandlerTypeName);
                if (resourceHandlerClass == null)
                {
                    if (TryLoadAlreadyGeneratedAssembly(resourceHandlerTypeName, out var assembly))
                    {
                        try
                        {
                            resourceHandlerClass = assembly?.GetType(resourceHandlerTypeName!, true);
                        }
                        catch (Exception)
                        {
                            resourceHandlerClass = Assembly.GetCallingAssembly().GetType(resourceHandlerTypeName!, true);
                        }
                    }
                    else
                    {
                        resourceHandlerClass = Assembly.GetCallingAssembly().GetType(resourceHandlerTypeName!, true);
                    }
                }
                ConfirmResourceHandler(resourceHandlerClass);
                return resourceHandlerClass;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"The resource handler class {resourceHandlerTypeName} cannot be loaded because: {e.Message}");
            }
        }

        public override void Log(ILogger logger)
        {
            logger.Info($"Resource: {Name}");

            foreach (var action in Actions)
            {
                logger.Info($"Action: id={action.Id}, method={action.Method}, uri={action.Uri}, to={action.To.Signature}");
            }
        }

        private static void AssertSaneActions(IList<Action> actions)
        {
            var expectedId = 0;
            foreach (var action in actions)
            {
                if (action.Id != expectedId)
                {
                    throw new ArgumentException($"Action id in conflict with expected ordering: expected id: {expectedId} Action is: {action}");
                }
                ++expectedId;
            }
        }

        private static void ConfirmResourceHandler(Type? resourceHandlerClass)
        {
            var superclass = resourceHandlerClass?.BaseType;
            while (superclass != null)
            {
                if (superclass == typeof(ResourceHandler))
                {
                    return;
                }
                superclass = superclass.BaseType;
            }
            throw new ArgumentException($"ConfigurationResource handler class must extends ResourceHandler: {resourceHandlerClass?.Name}");
        }
        
        private static bool TryLoadAlreadyGeneratedAssembly(Type resourceHandlerType, out Assembly? assembly)
        {
            try
            {
                var classPath = new FileInfo(HttpProperties.Instance.GetProperty("resource.dispatcher.generated.classes.main", RootOfMainClasses)!);
                var resourcePath = resourceHandlerType.FullName?.Substring(0, resourceHandlerType.FullName.LastIndexOf('.'))
                    .Replace('.', Path.DirectorySeparatorChar);
                var filePath = Path.Combine(classPath.DirectoryName!, resourcePath!, resourceHandlerType.Name + DispatcherSuffix + ".dll");
                byte[] assemblyBytes = File.ReadAllBytes(filePath);
                assembly = Assembly.Load(assemblyBytes);
                return true;
            }
            catch
            {
                assembly = null;
                return false;
            }
        }
        
        private static bool TryLoadAlreadyGeneratedAssembly(string? resourceHandlerTypeName, out Assembly? assembly)
        {
            try
            {
                var classPath = new FileInfo(HttpProperties.Instance.GetProperty("resource.dispatcher.generated.classes.main", RootOfMainClasses)!);
                var resourcePath = resourceHandlerTypeName?
                    .Substring(0, resourceHandlerTypeName.LastIndexOf('.'))
                    .Replace('.', Path.DirectorySeparatorChar);
                var resourceHandlerName = resourceHandlerTypeName?.Substring(resourceHandlerTypeName.LastIndexOf('.') + 1);
                var filePath = Path.Combine(classPath.DirectoryName!, resourcePath!, resourceHandlerName + DispatcherSuffix + ".dll");
                byte[] assemblyBytes = File.ReadAllBytes(filePath);
                assembly = Assembly.Load(assemblyBytes);
                return true;
            }
            catch
            {
                assembly = null;
                return false;
            }
        }

        private static Type? TryGenerateCompile(
            Type resourceHandlerClass,
            string fullyQualifiedClassName,
            string lookupTypeName,
            IList<Action> actions,
            ILogger logger)
        {
            try
            {
                return TryGenerateCompile(
                    resourceHandlerClass,
                    ResourceDispatcherGenerator.ForMain(actions, true, logger),
                    fullyQualifiedClassName,
                    lookupTypeName);
            }
            catch
            {
                try
                {
                    return TryGenerateCompile(
                        resourceHandlerClass,
                        ResourceDispatcherGenerator.ForTest(actions, true, logger),
                        fullyQualifiedClassName,
                        lookupTypeName);
                }
                catch (Exception etest)
                {
                    throw new ArgumentException($"ConfigurationResource dispatcher for {resourceHandlerClass.Name} not created for main or test because: {etest.Message}", etest);
                }
            }
        }

        private static Type? TryGenerateCompile(
            Type resourceHandlerClass,
            ResourceDispatcherGenerator generator,
            string fullyQualifiedClassName,
            string lookupTypeName)
        {
            try
            {
                var result = generator.GenerateFor(resourceHandlerClass);
                var input = new Input(resourceHandlerClass, fullyQualifiedClassName, lookupTypeName, result.Source, result.SourceFile, ClassLoader, generator.Type, true);
                var resourceDispatcherClass = DynaCompiler.Compile(input);
                return resourceDispatcherClass;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"ConfigurationResource instance with dispatcher for {resourceHandlerClass.Name} not created because: {e.Message}", e);
            }
        }

        public override Action.MatchResults MatchWith(Method? method, Uri? uri)
        {
            foreach (var action in Actions)
            {
                var matchResults = action.MatchWith(method, uri);
                if (matchResults.IsMatched)
                {
                    return matchResults;
                }
            }
            return Action.UnmatchedResults;
        }


        protected ConfigurationResource(string name, Type resourceHandlerClass, int handlerPoolSize, IList<Action> actions)
                    : base(name, handlerPoolSize)
        {
            ResourceHandlerClass = resourceHandlerClass;
            Actions = new ArraySegment<Action>(actions.ToArray());
        }

        public override ResourceHandler ResourceHandlerInstance(Stage stage)
        {
            try
            {
                foreach (var ctor in ResourceHandlerClass.GetConstructors())
                {
                    if (ctor.GetParameters().Length == 1)
                    {
                        return (ResourceHandler)ctor.Invoke(new object[] { stage.World });
                    }
                }
                return (ResourceHandler)Activator.CreateInstance(ResourceHandlerClass)!;
            }
            catch
            {
                throw new ArgumentException($"The instance for resource handler '{ResourceHandlerClass.Name}' cannot be created.");
            }
        }
    }
}
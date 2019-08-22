// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Actors;
using Vlingo.Common.Compiler;

namespace Vlingo.Http.Resource
{
    public abstract class ConfigurationResource : Resource
    {
        private const string DispatcherSuffix = "Dispatcher";

        private static readonly DynaClassLoader ClassLoader = new DynaClassLoader();
        private static readonly DynaCompiler DynaCompiler = new DynaCompiler();

        public Type ResourceHandlerClass { get; }
        internal IReadOnlyList<Action> Actions { get; }

        public static ConfigurationResource Defining(
            string resourceName,
            Type resourceHandlerClass,
            int handlerPoolSize,
            IList<Action> actions)
            => NewResourceFor(resourceName, resourceHandlerClass, handlerPoolSize, actions);

        internal static ConfigurationResource NewResourceFor(
            string resourceName,
            Type resourceHandlerClass,
            int handlerPoolSize,
            IList<Action> actions)
        {
            AssertSaneActions(actions);

            try
            {
                var fullyQualifiedClassName = DynaNaming.FullyQualifiedClassNameFor(resourceHandlerClass, DispatcherSuffix);
                var lookupTypeName = DynaNaming.FullyQualifiedClassNameFor(resourceHandlerClass, DispatcherSuffix, true);

                Type resourceClass = null;
                try
                {
                    // this check is done primarily for testing to prevent duplicate class mimeType in class loader
                    resourceClass = Type.GetType(fullyQualifiedClassName);
                }
                catch
                {
                    resourceClass = TryGenerateCompile(resourceHandlerClass, fullyQualifiedClassName, lookupTypeName, actions);
                }

                var ctorParams = new object[] { resourceName, resourceHandlerClass, handlerPoolSize, actions };
                foreach (var ctor in resourceClass.GetConstructors())
                {
                    if (ctor.GetParameters().Length == ctorParams.Length)
                    {
                        var resourceDispatcher = (ConfigurationResource)ctor.Invoke(ctorParams);
                        return resourceDispatcher;
                    }
                }
                return (ConfigurationResource)Activator.CreateInstance(resourceClass);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Cannot create a resource from resource handler {resourceHandlerClass.Name} because: {e.Message}");
            }
        }

        internal static Type NewResourceHandlerClassFor(string resourceHandlerClassname)
        {
            try
            {
                var resourceHandlerClass = Type.GetType(resourceHandlerClassname);
                ConfirmResourceHandler(resourceHandlerClass);
                return resourceHandlerClass;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"The resource handler class {resourceHandlerClassname} cannot be loaded because: {e.Message}");
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

        private static void ConfirmResourceHandler(Type resourceHandlerClass)
        {
            var superclass = resourceHandlerClass.BaseType;
            while (superclass != null)
            {
                if (superclass == typeof(ResourceHandler))
                {
                    return;
                }
                superclass = superclass.BaseType;
            }
            throw new ArgumentException($"ConfigurationResource handler class must extends ResourceHandler: {resourceHandlerClass.Name}");
        }

        private static Type TryGenerateCompile(
            Type resourceHandlerClass,
            string fullyQualifiedClassName,
            string lookupTypeName,
            IList<Action> actions)
        {
            try
            {
                return TryGenerateCompile(
                    resourceHandlerClass,
                    ResourceDispatcherGenerator.ForMain(actions, true),
                    fullyQualifiedClassName,
                    lookupTypeName);
            }
            catch
            {
                try
                {
                    return TryGenerateCompile(
                        resourceHandlerClass,
                        ResourceDispatcherGenerator.ForTest(actions, true),
                        fullyQualifiedClassName,
                        lookupTypeName);
                }
                catch (Exception etest)
                {
                    throw new ArgumentException($"ConfigurationResource dispatcher for {resourceHandlerClass.Name} not created for main or test because: {etest.Message}", etest);
                }
            }
        }

        private static Type TryGenerateCompile(
            Type resourceHandlerClass,
            ResourceDispatcherGenerator generator,
            string fullyQualifiedClassName,
            string lookupTypeName)
        {
            try
            {
                var result = generator.GenerateFor(resourceHandlerClass.FullName);
                var input = new Input(resourceHandlerClass, fullyQualifiedClassName, lookupTypeName, result.source, result.sourceFile, ClassLoader, generator.Type, true);
                var resourceDispatcherClass = DynaCompiler.Compile(input);
                return resourceDispatcherClass;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"ConfigurationResource instance with dispatcher for {resourceHandlerClass.Name} not created because: {e.Message}", e);
            }
        }

        internal override Action.MatchResults MatchWith(Method method, Uri uri)
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

        protected override ResourceHandler ResourceHandlerInstance(Stage stage)
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
                return (ResourceHandler)Activator.CreateInstance(ResourceHandlerClass);
            }
            catch
            {
                throw new ArgumentException($"The instance for resource handler '{ResourceHandlerClass.Name}' cannot be created.");
            }
        }
    }
}

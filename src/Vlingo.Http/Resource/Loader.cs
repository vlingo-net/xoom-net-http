// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vlingo.Actors;
using Vlingo.Http.Resource.Sse;

namespace Vlingo.Http.Resource
{
    public static class Loader
    {
        private const string resourceNamePrefix = "resource.name.";
        private const string ssePublisherFeedClassnameParameter = "Type feedClass";
        private const string ssePublisherFeedDefaultId = "string feedDefaultId";
        private const string ssePublisherFeedIntervalParameter = "int feedInterval";
        private const string ssePublisherFeedPayloadParameter = "int feedPayload";
        private const string ssePublisherIdPathParameter = "{id}";
        private const string ssePublisherNamePrefix = "sse.stream.name.";
        private const string ssePublisherNamePathParameter = "{streamName}";
        private const string ssePublisherSubscribeTo =
            "SubscribeToStream(string streamName, " +
                    ssePublisherFeedClassnameParameter + ", " +
                    ssePublisherFeedPayloadParameter + ", " +
                    ssePublisherFeedIntervalParameter + ", " +
                    ssePublisherFeedDefaultId + ")";
        private const string ssePublisherUnsubscribeTo = "UnsubscribeFromStream(string streamName, string id)";
        private const string staticFilesResource = "static.files";
        private const string staticFilesResourcePool = "static.files.resource.pool";
        private const string staticFilesResourceRoot = "static.files.resource.root";
        private const string staticFilesResourceSubPaths = "static.files.resource.subpaths";
        private const string staticFilesResourceServeFile = "ServeFile(string contentFile, string root, string validSubPaths)";
        private const string staticFilesResourcePathParameter = "{contentFile}";

        public static Resources LoadResources(HttpProperties properties)
        {
            var namedResources = new Dictionary<string, Resource>();

            foreach (var resource in FindResources(properties, resourceNamePrefix))
            {
                var loaded = LoadResource(properties, resource);

                namedResources[loaded.Name] = loaded;
            }

            foreach (var item in LoadSseResources(properties))
            {
                namedResources[item.Key] = item.Value;
            }

            foreach (var item in LoadStaticFilesResource(properties))
            {
                namedResources[item.Key] = item.Value;
            }

            return new Resources(namedResources);
        }

        private static HashSet<string> FindResources(HttpProperties properties, string namePrefix)
        {
            var resource = new HashSet<string>();

            properties.Keys
                .Where(k => k.StartsWith(namePrefix))
                .ToList()
                .ForEach(k => resource.Add(k));

            return resource;
        }

        private static ConfigurationResource<ResourceHandler> LoadResource(HttpProperties properties, string resourceNameKey)
        {
            var resourceName = resourceNameKey.Substring(resourceNamePrefix.Length);
            var resourceActionNames = ActionNamesFrom(properties.GetProperty(resourceNameKey), resourceNameKey);
            var resourceHandlerKey = $"resource.{resourceName}.handler";
            var resourceHandlerClassname = properties.GetProperty(resourceHandlerKey);
            var handlerPoolKey = $"resource.{resourceName}.pool";
            var maybeHandlerPoolSize = int.Parse(properties.GetProperty(handlerPoolKey, "1"));
            var handlerPoolSize = maybeHandlerPoolSize <= 0 ? 1 : maybeHandlerPoolSize;
            var disallowPathParametersWithSlashKey = $"resource.{resourceName}.disallowPathParametersWithSlash";
            var disallowPathParametersWithSlash = bool.Parse(properties.GetProperty(disallowPathParametersWithSlashKey, "true"));

            try
            {
                var resourceActions = ResourceActionsOf(properties, resourceName, resourceActionNames, disallowPathParametersWithSlash);

                var resourceHandlerClass = ConfigurationResource<ResourceHandler>.NewResourceHandlerTypeFor(resourceHandlerClassname);

                return ResourceFor(resourceName, resourceHandlerClass, handlerPoolSize, resourceActions);
            }
            catch (Exception e)
            {
                Console.WriteLine("vlingo-net/http: Failed to load resource: " + resourceName + " because: " + e.Message);
                throw e;
            }
        }

        private static IDictionary<string, ConfigurationResource<ResourceHandler>> LoadSseResources(HttpProperties properties)
        {
            var sseResourceActions = new Dictionary<string, ConfigurationResource<ResourceHandler>>();

            foreach (var streamResourceName in FindResources(properties, ssePublisherNamePrefix))
            {
                var streamURI = properties.GetProperty(streamResourceName);
                var resourceName = streamResourceName.Substring(ssePublisherNamePrefix.Length);
                var feedClassnameKey = $"sse.stream.{resourceName}.feed.class";
                var feedClassname = properties.GetProperty(feedClassnameKey);
                var feedPayloadKey = "sse.stream." + resourceName + ".feed.payload";
                var maybeFeedPayload = int.Parse(properties.GetProperty(feedPayloadKey, "20"));
                var feedPayload = maybeFeedPayload <= 0 ? 20 : maybeFeedPayload;
                var feedIntervalKey = $"sse.stream.{resourceName}.feed.interval";
                var maybeFeedInterval = int.Parse(properties.GetProperty(feedIntervalKey, "1000"));
                var feedInterval = maybeFeedInterval <= 0 ? 1000 : maybeFeedInterval;
                var feedDefaultIdKey = $"sse.stream.{resourceName}.feed.default.id";
                var feedDefaultId = properties.GetProperty(feedDefaultIdKey, "");
                var poolKey = $"sse.stream.{resourceName}.pool";
                var maybePoolSize = int.Parse(properties.GetProperty(poolKey, "1"));
                var handlerPoolSize = maybePoolSize <= 0 ? 1 : maybePoolSize;
                var subscribeURI = streamURI?.Replace(resourceName, ssePublisherNamePathParameter);
                var unsubscribeURI = subscribeURI + "/" + ssePublisherIdPathParameter;

                try
                {
                    var feedClass = ActorClassWithProtocol(feedClassname, typeof(ISseFeed));
                    var mappedParameterClass = new Action.MappedParameter("Type", feedClass);
                    var mappedParameterPayload = new Action.MappedParameter("int", feedPayload);
                    var mappedParameterInterval = new Action.MappedParameter("int", feedInterval);
                    var mappedParameterDefaultId = new Action.MappedParameter("string", feedDefaultId);

                    var actions = new List<Action>(2);
                    var additionalParameters = new List<Action.MappedParameter> { mappedParameterClass, mappedParameterPayload, mappedParameterInterval, mappedParameterDefaultId };
                    actions.Add(new Action(0, Method.Get.Name, subscribeURI, ssePublisherSubscribeTo, null, true, additionalParameters));
                    actions.Add(new Action(1, Method.Delete.Name, unsubscribeURI, ssePublisherUnsubscribeTo, null, true));
                    var resource = ResourceFor(resourceName, typeof(SseStreamResource), handlerPoolSize, actions);
                    sseResourceActions[resourceName] = resource;
                }
                catch (Exception e)
                {
                    Console.WriteLine("vlingo-net/http: Failed to load SSE resource: " + streamResourceName + " because: " + e.Message);
                    Console.WriteLine(e.StackTrace);
                    throw e;
                }
            }

            return sseResourceActions;
        }


        private static IDictionary<string, ConfigurationResource<ResourceHandler>> LoadStaticFilesResource(HttpProperties properties)
        {
            var staticFilesResourceActions = new Dictionary<string, ConfigurationResource<ResourceHandler>>();

            var root = properties.GetProperty(staticFilesResourceRoot);

            if (root == null)
            {
                return staticFilesResourceActions;
            }

            var poolSize = properties.GetProperty(staticFilesResourcePool, "5");
            var validSubPaths = properties.GetProperty(staticFilesResourceSubPaths);
            var actionSubPaths = ActionNamesFrom(validSubPaths, staticFilesResourceSubPaths).OrderByDescending(x => x.Length);

            try
            {
                int resourceSequence = 0;

                foreach (var actionSubPath in actionSubPaths)
                {
                    var mappedParameterRoot = new Action.MappedParameter("string", root);
                    var mappedParameterValidSubPaths = new Action.MappedParameter("string", validSubPaths);

                    var slash = actionSubPath.EndsWith("/") ? "" : "/";
                    var resourceName = staticFilesResource + resourceSequence++;

                    var actions = new List<Action>(1);
                    var additionalParameters = new List<Action.MappedParameter> { mappedParameterRoot, mappedParameterValidSubPaths };
                    actions.Add(new Action(0, Method.Get.Name, actionSubPath + slash + staticFilesResourcePathParameter, staticFilesResourceServeFile, null, false, additionalParameters));
                    var resource = ResourceFor(resourceName, typeof(StaticFilesResource), int.Parse(poolSize), actions);
                    staticFilesResourceActions[resourceName] = resource;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("vlingo-net/http: Failed to load static files resource: " + staticFilesResource + " because: " + e.Message);
                Console.WriteLine(e.StackTrace);
                throw e;
            }

            return staticFilesResourceActions;
        }

        private static ConfigurationResource<ResourceHandler> ResourceFor(
            string resourceName,
            Type resourceHandlerClass,
            int handlerPoolSize,
            IList<Action> resourceActions)
        {
            try
            {
                var resource = ConfigurationResource<ResourceHandler>.NewResourceFor(resourceName, resourceHandlerClass, handlerPoolSize, resourceActions);
                return resource;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("ConfigurationResource cannot be created for: " + resourceHandlerClass.Name, e);
            }
        }

        private static string[] ActionNamesFrom(string? actionNamesProperty, string key)
        {
            var open = actionNamesProperty?.IndexOf("[");
            var close = actionNamesProperty?.IndexOf("]");

            if (!open.HasValue || !close.HasValue || open == -1 || close == -1)
            {
                throw new IndexOutOfRangeException("Cannot load action names for resource: " + key);
            }

            var actionNames = Regex.Split(actionNamesProperty?.Substring(open.Value + 1, close.Value).Trim(), ",\\s?");

            if (actionNames.Length == 0)
            {
                throw new InvalidOperationException("Cannot load action names for resource: " + key);
            }

            return actionNames;
        }

        private static List<Action> ResourceActionsOf(
            HttpProperties properties,
            string resourceName,
            string[] resourceActionNames,
            bool disallowPathParametersWithSlash)
        {
            var resourceActions = new List<Action>(resourceActionNames.Length);

            foreach (var actionName in resourceActionNames)
            {
                try
                {
                    var keyPrefix = "action." + resourceName + "." + actionName + ".";

                    var actionId = resourceActions.Capacity;
                    var method = properties.GetProperty(keyPrefix + "method", null);
                    var uri = properties.GetProperty(keyPrefix + "uri", null);
                    var to = properties.GetProperty(keyPrefix + "to", null);
                    var mapper = properties.GetProperty(keyPrefix + "mapper", null);

                    resourceActions.Add(new Action(actionId, method, uri, to, mapper, disallowPathParametersWithSlash));
                }
                catch (Exception e)
                {
                    Console.WriteLine("vlingo-net/http: Failed to load resource: " + resourceName + " action:" + actionName + " because: " + e.Message);
                    throw e;
                }
            }

            return resourceActions;
        }

        private static Type ActorClassWithProtocol(string? actorClassname, Type protocolClass)
        {
            try
            {
                var actorClass = Type.GetType(actorClassname);
                AssertActorWithProtocol(actorClass, protocolClass);
                return actorClass;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"The class {actorClassname} cannot be loaded because: {e.Message}", e);
            }
        }

        private static void AssertActorWithProtocol(Type candidateActorClass, Type protocolClass)
        {
            var superclass = candidateActorClass.BaseType;
            while (superclass != null)
            {
                if (superclass == typeof(Actor))
                {
                    break;
                }
                superclass = superclass.BaseType;
            }

            if (superclass == null)
            {
                throw new ArgumentException($"Class must extend Vlingo.Actors.Actor: {candidateActorClass.FullName}");
            }

            foreach (var protocolInterfaceClass in candidateActorClass.GetInterfaces())
            {
                if (protocolClass == protocolInterfaceClass)
                {
                    return;
                }
            }
            throw new ArgumentException($"Actor class {candidateActorClass.FullName} must implement: {protocolClass.FullName}");
        }
    }
}

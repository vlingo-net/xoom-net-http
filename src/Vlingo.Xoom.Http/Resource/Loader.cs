// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Http.Resource.Feed;
using Vlingo.Xoom.Http.Resource.Sse;

namespace Vlingo.Xoom.Http.Resource
{
    public static class Loader
    {
        private const string FeedProducerNamePrefix = "feed.resource.name.";
        private const string FeedNamePathParameter = "{feedName}";
        private const string FeedProductIdPathParameter = "{feedProductId}";
        private const string FeedProducerClassnameParameter = "Type feedProducerClass";
        private const string FeedProducerProductElementsParameter = "int feedProductElements";
        private const string FeedProducerFeed =
            "feed(string feedName, string feedProductId, " + FeedProducerClassnameParameter + ", " + FeedProducerProductElementsParameter + ")";
            
        private const string ResourceNamePrefix = "resource.name.";
        private const string SsePublisherFeedClassnameParameter = "Type feedClass";
        private const string SsePublisherFeedDefaultId = "string feedDefaultId";
        private const string SsePublisherFeedIntervalParameter = "int feedInterval";
        private const string SsePublisherFeedPayloadParameter = "int feedPayload";
        private const string SsePublisherIdPathParameter = "{id}";
        private const string SsePublisherNamePrefix = "sse.stream.name.";
        private const string SsePublisherNamePathParameter = "{streamName}";
        private const string SsePublisherSubscribeTo =
            "SubscribeToStream(string streamName, " +
                    SsePublisherFeedClassnameParameter + ", " +
                    SsePublisherFeedPayloadParameter + ", " +
                    SsePublisherFeedIntervalParameter + ", " +
                    SsePublisherFeedDefaultId + ")";
        private const string SsePublisherUnsubscribeTo = "UnsubscribeFromStream(string streamName, string id)";
        private const string StaticFilesResource = "static.files";
        private const string StaticFilesResourcePool = "static.files.resource.pool";
        private const string StaticFilesResourceRoot = "static.files.resource.root";
        private const string StaticFilesResourceSubPaths = "static.files.resource.subpaths";
        private const string StaticFilesResourceServeFile = "ServeFile(string contentFile, string root, string validSubPaths)";
        private const string StaticFilesResourcePathParameter = "{contentFile}";
        private const string StaticFilesResourceRoot1 = "//";
        private const string StaticFilesResourceRoot2 = "///";

        public static Resources LoadResources(HttpProperties properties, ILogger logger)
        {
            var namedResources = new Dictionary<string, IResource>();

            foreach (var resource in FindResources(properties, ResourceNamePrefix))
            {
                var loaded = LoadResource(properties, resource, logger);

                namedResources[loaded.Name] = loaded;
            }

            foreach (var item in LoadSseResources(properties, logger))
            {
                namedResources[item.Key] = item.Value;
            }
            
            foreach (var item in LoadFeedResources(properties, logger))
            {
                namedResources[item.Key] = item.Value;
            }

            foreach (var item in LoadStaticFilesResource(properties, logger))
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

        private static IConfigurationResource LoadResource(HttpProperties properties, string resourceNameKey, ILogger logger)
        {
            var resourceName = resourceNameKey.Substring(ResourceNamePrefix.Length);
            var resourceActionNames = ActionNamesFrom(properties.GetProperty(resourceNameKey), resourceNameKey);
            var resourceHandlerKey = $"resource.{resourceName}.handler";
            var resourceHandlerClassname = properties.GetProperty(resourceHandlerKey);
            var handlerPoolKey = $"resource.{resourceName}.pool";
            var maybeHandlerPoolSize = int.Parse(properties.GetProperty(handlerPoolKey, "1") ?? "1");
            var handlerPoolSize = maybeHandlerPoolSize <= 0 ? 1 : maybeHandlerPoolSize;

            try
            {
                var resourceActions = ResourceActionsOf(properties, resourceName, resourceActionNames);

                var resourceHandlerClass = ConfigurationResource.NewResourceHandlerTypeFor(resourceHandlerClassname);

                return ResourceFor(resourceName, resourceHandlerClass!, handlerPoolSize, resourceActions, logger);
            }
            catch (Exception e)
            {
                Console.WriteLine("vlingo-net/http: Failed to load resource: " + resourceName + " because: " + e.Message);
                throw;
            }
        }

        private static IDictionary<string, IConfigurationResource> LoadFeedResources(HttpProperties properties, ILogger logger)
        {
            var feedResourceActions = new Dictionary<string, IConfigurationResource>();

            foreach (var feedResourceName in FindResources(properties, FeedProducerNamePrefix))
            {
                var feedUri = properties.GetProperty(feedResourceName);
                var resourceName = feedResourceName.Substring(FeedProducerNamePrefix.Length);
                var feedProducerClassnameKey = $"feed.resource.{resourceName}.producer.class";
                var feedProducerClassname = properties.GetProperty(feedProducerClassnameKey);
                var feedElementsKey = $"feed.resource.{resourceName}.elements";
                var maybeFeedElements = int.Parse(properties.GetProperty(feedElementsKey, "20") ?? "20");
                var feedElements = maybeFeedElements <= 0 ? 20 : maybeFeedElements;
                var poolKey = $"feed.resource.{resourceName}.pool";
                var maybePoolSize = int.Parse(properties.GetProperty(poolKey, "1") ?? "1");
                int handlerPoolSize = maybePoolSize <= 0 ? 1 : maybePoolSize;
                var feedRequestUri =
                    $"{feedUri?.Replace(resourceName, FeedNamePathParameter)}/{FeedProductIdPathParameter}";

                try
                {
                    var feedClass = ActorClassWithProtocol(feedProducerClassname, typeof(IFeedProducer));
                    var mappedParameterProducerClass = new Action.MappedParameter("Type", feedClass);
                    var mappedParameterProductElements = new Action.MappedParameter("int", feedElements);

                    var actions = new List<Action>(1);
                    var additionalParameters = new List<Action.MappedParameter> { mappedParameterProducerClass, mappedParameterProductElements };
                    actions.Add(new Action(0, Method.Get.Name(), feedRequestUri, FeedProducerFeed, null, additionalParameters));
                    var resource = ResourceFor(resourceName, typeof(FeedResource), handlerPoolSize, actions, logger);
                    feedResourceActions.Add(resourceName, resource);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"vlingo/http: Failed to load feed resource: {resourceName} because: {e.Message}");
                    Console.WriteLine(e.StackTrace);
                    throw;
                }   
            }

            return feedResourceActions;
        }

        private static IDictionary<string, IConfigurationResource> LoadSseResources(HttpProperties properties, ILogger logger)
        {
            var sseResourceActions = new Dictionary<string, IConfigurationResource>();

            foreach (var streamResourceName in FindResources(properties, SsePublisherNamePrefix))
            {
                var streamUri = properties.GetProperty(streamResourceName);
                var resourceName = streamResourceName.Substring(SsePublisherNamePrefix.Length);
                var feedClassnameKey = $"sse.stream.{resourceName}.feed.class";
                var feedClassname = properties.GetProperty(feedClassnameKey);
                var feedPayloadKey = "sse.stream." + resourceName + ".feed.payload";
                var maybeFeedPayload = int.Parse(properties.GetProperty(feedPayloadKey, "20") ?? "20");
                var feedPayload = maybeFeedPayload <= 0 ? 20 : maybeFeedPayload;
                var feedIntervalKey = $"sse.stream.{resourceName}.feed.interval";
                var maybeFeedInterval = int.Parse(properties.GetProperty(feedIntervalKey, "1000") ?? "1000");
                var feedInterval = maybeFeedInterval <= 0 ? 1000 : maybeFeedInterval;
                var feedDefaultIdKey = $"sse.stream.{resourceName}.feed.default.id";
                var feedDefaultId = properties.GetProperty(feedDefaultIdKey, "");
                var poolKey = $"sse.stream.{resourceName}.pool";
                var maybePoolSize = int.Parse(properties.GetProperty(poolKey, "1") ?? "1");
                var handlerPoolSize = maybePoolSize <= 0 ? 1 : maybePoolSize;
                var subscribeUri = streamUri?.Replace(resourceName, SsePublisherNamePathParameter);
                var unsubscribeUri = subscribeUri + "/" + SsePublisherIdPathParameter;

                try
                {
                    var feedClass = ActorClassWithProtocol(feedClassname, typeof(ISseFeed));
                    var mappedParameterClass = new Action.MappedParameter("Type", feedClass);
                    var mappedParameterPayload = new Action.MappedParameter("int", feedPayload);
                    var mappedParameterInterval = new Action.MappedParameter("int", feedInterval);
                    var mappedParameterDefaultId = new Action.MappedParameter("string", feedDefaultId);

                    var actions = new List<Action>(2);
                    var additionalParameters = new List<Action.MappedParameter> { mappedParameterClass, mappedParameterPayload, mappedParameterInterval, mappedParameterDefaultId };
                    actions.Add(new Action(0, Method.Get.Name(), subscribeUri, SsePublisherSubscribeTo, null, additionalParameters));
                    actions.Add(new Action(1, Method.Delete.Name(), unsubscribeUri, SsePublisherUnsubscribeTo, null));
                    var resource = ResourceFor(resourceName, typeof(SseStreamResource), handlerPoolSize, actions, logger);
                    sseResourceActions[resourceName] = resource;
                }
                catch (Exception e)
                {
                    Console.WriteLine("vlingo-net/http: Failed to load SSE resource: " + streamResourceName + " because: " + e.Message);
                    Console.WriteLine(e.StackTrace);
                    throw;
                }
            }

            return sseResourceActions;
        }

        private static IDictionary<string, IConfigurationResource> LoadStaticFilesResource(HttpProperties properties, ILogger logger)
        {
            var staticFilesResourceActions = new Dictionary<string, IConfigurationResource>();

            var root = properties.GetProperty(StaticFilesResourceRoot);

            if (root == null)
            {
                return staticFilesResourceActions;
            }

            var poolSize = properties.GetProperty(StaticFilesResourcePool, "5")!;
            var validSubPaths = properties.GetProperty(StaticFilesResourceSubPaths);
            var actionSubPaths = ActionNamesFrom(validSubPaths, StaticFilesResourceSubPaths);

            LoadStaticFileResource(staticFilesResourceActions, root, poolSize, validSubPaths, actionSubPaths, logger);

            return staticFilesResourceActions;
        }

        private static void LoadStaticFileResource(Dictionary<string, IConfigurationResource> staticFilesResources,
            string root,
            string poolSize,
            string? validSubPaths,
            IEnumerable<string> actionSubPaths, ILogger logger)
        {
            try
            {
                var resourceSequence = 0;

                foreach (var actionSubPath in ListOfSorted(actionSubPaths.ToArray()))
                {
                    var mappedParameterRoot = new Action.MappedParameter("string", root);
                    var mappedParameterValidSubPaths = new Action.MappedParameter("string", validSubPaths);

                    var slash = actionSubPath.EndsWith("/") ? "" : "/";
                    var resourceName = StaticFilesResource + resourceSequence++;

                    var actions = new List<Action>(1);
                    var additionalParameters = new List<Action.MappedParameter>
                        {mappedParameterRoot, mappedParameterValidSubPaths};
                    actions.Add(new Action(0, Method.Get.Name(), PatternFrom(actionSubPath, slash), StaticFilesResourceServeFile, null, additionalParameters));
                    var resource = ResourceFor(resourceName, typeof(StaticFilesResource), int.Parse(poolSize), actions, logger);
                    staticFilesResources[resourceName] = resource;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"vlingo-net/http: Failed to load static files resource: {StaticFilesResource} because: {e.Message}");
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }
        
        private static IEnumerable<string> ListOfSorted(string[] actionSubPaths)
        {
            var sortedActionsSubPaths = actionSubPaths.OrderByDescending(x => x.Length);
            var list = new List<string>(2 + actionSubPaths.Length) {StaticFilesResourceRoot1, StaticFilesResourceRoot2};

            list.AddRange(sortedActionsSubPaths);

            return list;
        }
        
        private static string PatternFrom(string path, string slash)
        {

            switch (path)
            {
                case StaticFilesResourceRoot1:
                    return "";
                case StaticFilesResourceRoot2:
                    return "/";
            }

            return path + slash + StaticFilesResourcePathParameter;
        }

        private static IConfigurationResource ResourceFor(
            string resourceName,
            Type resourceHandlerClass,
            int handlerPoolSize,
            IList<Action> resourceActions,
            ILogger logger)
        {
            try
            {
                var resource = ConfigurationResource.NewResourceFor(resourceName, resourceHandlerClass, handlerPoolSize, resourceActions, logger);
                return resource;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"ConfigurationResource cannot be created for: {resourceHandlerClass.Name}", e);
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

            var actionNames = Regex.Split(actionNamesProperty?.Substring(open.Value + 1, close.Value - 1).Trim() ?? string.Empty, ",\\s?");

            if (actionNames.Length == 0)
            {
                throw new InvalidOperationException($"Cannot load action names for resource: {key}");
            }

            return actionNames;
        }

        private static List<Action> ResourceActionsOf(
            HttpProperties properties,
            string resourceName,
            string[] resourceActionNames)
        {
            var resourceActions = new List<Action>(resourceActionNames.Length);

            foreach (var actionName in resourceActionNames)
            {
                try
                {
                    var keyPrefix = $"action.{resourceName}.{char.ToLowerInvariant(actionName[0])}{actionName.Substring(1)}.";

                    var actionId = resourceActions.Count;
                    var method = properties.GetProperty($"{keyPrefix}method", null);
                    var uri = properties.GetProperty($"{keyPrefix}uri", null);
                    var to = properties.GetProperty($"{keyPrefix}to", null);
                    var mapper = properties.GetProperty($"{keyPrefix}mapper", null);

                    resourceActions.Add(new Action(actionId, method, uri, to, mapper));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"vlingo-net/http: Failed to load resource: {resourceName} action:{actionName} because: {e.Message}");
                    throw;
                }
            }

            return resourceActions;
        }

        private static Type ActorClassWithProtocol(string? actorClassname, Type protocolClass)
        {
            try
            {
                var actorClass = TypeLoader.Load(actorClassname);
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
                throw new ArgumentException($"Class must extend Vlingo.Xoom.Actors.Actor: {candidateActorClass.FullName}");
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
// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Common.Pool;
using Vlingo.Wire.Channel;
using Vlingo.Wire.Fdx.Bidirectional;
using Vlingo.Wire.Message;

namespace Vlingo.Http.Resource
{
    public sealed class ServerActor : Actor, IServer, IRequestChannelConsumerProvider, IScheduled<object?>
    {
        private readonly IServerRequestResponseChannel _channel;
        private readonly IDispatcher[] _dispatcherPool;
        private int _dispatcherPoolIndex;
        private readonly Filters _filters;
        private readonly int _maxMessageSize;
        private readonly Dictionary<string, RequestResponseHttpContext> _requestsMissingContent;
        private readonly long _requestMissingContentTimeout;
        private readonly ConsumerByteBufferPool _responseBufferPool;
        private readonly World _world;

        public ServerActor(
            Resources resources,
            Filters filters,
            int port,
            Configuration.SizingConf sizing,
            Configuration.TimingConf timing,
            string channelMailboxTypeName)
        {
            var start = DateExtensions.GetCurrentMillis();
            
            _filters = filters;
            _dispatcherPoolIndex = 0;
            _world = Stage.World;
            _requestsMissingContent = new Dictionary<string, RequestResponseHttpContext>();
            _maxMessageSize = sizing.MaxMessageSize;
            
            try
            {
                _responseBufferPool = new ConsumerByteBufferPool(
                    ElasticResourcePool<IConsumerByteBuffer, string>.Config.Of(sizing.MaxBufferPoolSize),
                    sizing.MaxMessageSize);
            
                _dispatcherPool = new IDispatcher[sizing.DispatcherPoolSize];
            
                for (int idx = 0; idx < sizing.DispatcherPoolSize; ++idx)
                {
                    _dispatcherPool[idx] = Dispatcher.StartWith(Stage, resources);
                }
            
                _channel =
                    ServerRequestResponseChannelFactory.StartNetty(
                        Stage,
                        Stage.World.AddressFactory.WithHighId(ChannelName),
                        channelMailboxTypeName,
                        this,
                        port,
                        ChannelName,
                        sizing.ProcessorPoolSize,
                        sizing.MaxBufferPoolSize,
                        sizing.MaxMessageSize,
                        timing.ProbeInterval,
                        timing.ProbeTimeout);
            
                var end = DateExtensions.GetCurrentMillis();
            
                Logger.Info($"Server {ServerName} is listening on port: {port} started in {end - start} ms");
            
                _requestMissingContentTimeout = timing.RequestMissingContentTimeout;
            
                LogResourceMappings(resources);
            
            }
            catch (Exception e)
            {
                var message = $"Failed to start server because: {e.Message}";
                Logger.Error(message, e);
                throw new InvalidOperationException(message);
            }
        }
        
        //=========================================
        // IServer
        //=========================================
        
        public ICompletes<bool> ShutDown()
        {
            Stop();

            return Completes().With(true);
        }

        public ICompletes<bool> StartUp()
        {
            Stage.Scheduler.Schedule(SelfAs<IScheduled<object?>>(), null, TimeSpan.FromMilliseconds(1000L), TimeSpan.FromMilliseconds(_requestMissingContentTimeout));

            return Completes().With(true);
        }

        public IServer StartWith(Stage stage) => ServerFactory.StartWith(stage);

        public IServer StartWith(Stage stage, HttpProperties properties) => ServerFactory.StartWith(stage, properties);

        public IServer StartWith(Stage stage, Resources resources, int port, Configuration.SizingConf sizing,
            Configuration.TimingConf timing) => ServerFactory.StartWith(stage, resources, port, sizing, timing);

        public IServer StartWith(Stage stage, Resources resources, Filters filters, int port,
            Configuration.SizingConf sizing, Configuration.TimingConf timing) =>
            ServerFactory.StartWith(stage, resources, filters, port, sizing, timing);

        public IServer StartWith(Stage stage, Resources resources, Filters filters, int port,
            Configuration.SizingConf sizing, Configuration.TimingConf timing,
            string severMailboxTypeName, string channelMailboxTypeName) =>
            ServerFactory.StartWith(stage, resources, filters, port, sizing, timing);
        
        //=========================================
        // IRequestChannelConsumerProvider
        //=========================================

        public IRequestChannelConsumer RequestChannelConsumer() => new ServerRequestChannelConsumer(this, PooledDispatcher());

        public void IntervalSignal(IScheduled<object?> scheduled, object? data) => FailTimedOutMissingContentRequests();

        public static string ServerName { get; } = "vlingo-http-server";
        
        public static string ChannelName { get; } = "server-request-response-channel";
        
        //=========================================
        // Stoppable
        //=========================================

        public override void Stop()
        {
            Logger.Info("Server stopping...");

            FailTimedOutMissingContentRequests();

            _channel.Stop();
            _channel.Close();

            foreach (var dispatcher in _dispatcherPool)
            {
                dispatcher.Stop();
            }
            
            _filters.Stop();

            Logger.Info("Server stopped.");

            base.Stop();
        }

        //=========================================
        // internal implementation
        //=========================================
        
        private void FailTimedOutMissingContentRequests()
        {
            if (IsStopped)
            {
                return;
            }

            if (_requestsMissingContent.Count == 0)
            {
                return;
            }

            var toRemove = new List<string>(); // prevent ConcurrentModificationException

            foreach (var id in _requestsMissingContent.Keys)
            {
                var requestResponseHttpContext = _requestsMissingContent[id];

                if (requestResponseHttpContext.RequestResponseContext.HasConsumerData)
                {
                    var parser = requestResponseHttpContext.RequestResponseContext.ConsumerData<RequestParser>();
                    if (parser.HasMissingContentTimeExpired(_requestMissingContentTimeout))
                    {
                        requestResponseHttpContext.RequestResponseContext.ConsumerData<RequestParser>(null!);
                        toRemove.Add(id);
                        requestResponseHttpContext.HttpContext.Completes.With(Response.Of(Response.ResponseStatus.BadRequest, "Missing content."));
                        requestResponseHttpContext.RequestResponseContext.ConsumerData<RequestParser>(null!);
                    }
                }
                else
                {
                    toRemove.Add(id); // already closed?
                }
            }

            foreach (var id in toRemove)
            {
                _requestsMissingContent.Remove(id);
            }
        }
        
        private void LogResourceMappings(Resources resources)
        {
            var logger = Logger;
            foreach (var resourceName in resources.NamedResources.Keys)
            {
                resources.NamedResources[resourceName].Log(logger);
            }
        }
        
        private IDispatcher PooledDispatcher()
        {
            if (_dispatcherPoolIndex >= _dispatcherPool.Length)
            {
                _dispatcherPoolIndex = 0;
            }
            
            return _dispatcherPool[_dispatcherPoolIndex++];
        }
        
        //=========================================
        // RequestChannelConsumer
        //=========================================

        private class ServerRequestChannelConsumer : IRequestChannelConsumer
        {
            private readonly ServerActor _serverActor;
            private readonly IDispatcher _dispatcher;

            internal ServerRequestChannelConsumer(ServerActor serverActor, IDispatcher dispatcher)
            {
                _serverActor = serverActor;
                _dispatcher = dispatcher;
            }
            
            public void CloseWith(RequestResponseContext requestResponseContext, object? data)
            {
                if (data != null)
                {
                    var request = _serverActor._filters.Process((Request) data);
                    var completes = new ResponseCompletes(_serverActor, requestResponseContext, request.Headers.HeaderOf(RequestHeader.XCorrelationID));
                    var context = new Context(requestResponseContext, request, _serverActor._world.CompletesFor(completes));
                    _dispatcher.DispatchFor(context);
                }
            }

            public void Consume(RequestResponseContext requestResponseContext, IConsumerByteBuffer buffer)
            {
                try
                {
                    RequestParser parser;
                    var wasIncompleteContent = false;

                    if (!requestResponseContext.HasConsumerData)
                    {
                        parser = RequestParser.ParserFor(buffer.ToArray());
                        requestResponseContext.ConsumerData(parser);
                    }
                    else
                    {
                        parser = requestResponseContext.ConsumerData<RequestParser>();
                        wasIncompleteContent = parser.IsMissingContent;
                        parser.ParseNext(buffer.ToArray());
                    }

                    Context? context = null;

                    while (parser.HasFullRequest())
                    {
                        var request = _serverActor._filters.Process(parser.FullRequest());
                        var completes = new ResponseCompletes(_serverActor, requestResponseContext, request.Headers.HeaderOf(RequestHeader.XCorrelationID));
                        context = new Context(requestResponseContext, request, _serverActor._world.CompletesFor(completes));
                        _dispatcher.DispatchFor(context);
                        if (wasIncompleteContent)
                        {
                            _serverActor._requestsMissingContent.Remove(requestResponseContext.Id);
                        }
                    }

                    if (parser.IsMissingContent && !_serverActor._requestsMissingContent.ContainsKey(requestResponseContext.Id))
                    {
                        if (context == null)
                        {
                            var completes = new ResponseCompletes(_serverActor, requestResponseContext);
                            context = new Context(_serverActor._world.CompletesFor(completes));
                        }
                        _serverActor._requestsMissingContent.Add(requestResponseContext.Id, new RequestResponseHttpContext(requestResponseContext, context));
                    }

                }
                catch (Exception e)
                {
                    _serverActor.Logger.Error("Request parsing failed.", e);
                    new ResponseCompletes(_serverActor, requestResponseContext, null).With(Response.Of(Response.ResponseStatus.BadRequest, e.Message));
                }
                finally
                {
                    buffer.Release();
                }
            }
        }
        
        //=========================================
        // RequestResponseHttpContext
        //=========================================

        private class RequestResponseHttpContext
        {
            public Context HttpContext { get; }
            public RequestResponseContext RequestResponseContext { get; }

            public RequestResponseHttpContext(RequestResponseContext requestResponseContext, Context httpContext)
            {
                RequestResponseContext = requestResponseContext;
                HttpContext = httpContext;
            }
        }
        
        //=========================================
        // ResponseCompletes
        //=========================================
        
        private class ResponseCompletes : BasicCompletes<object>
        {
            private readonly Header? _correlationId;
            private readonly ServerActor _serverActor;
            private readonly RequestResponseContext _requestResponseContext;

            internal ResponseCompletes(ServerActor serverActor, RequestResponseContext requestResponseContext, Header? correlationId) : base(serverActor.Stage.Scheduler)
            {
                _serverActor = serverActor;
                _requestResponseContext = requestResponseContext;
                _correlationId = correlationId;
            }

            internal ResponseCompletes(ServerActor serverActor, RequestResponseContext requestResponseContext) : this(serverActor, requestResponseContext, null)
            {
            }
            
            public override ICompletes<T> With<T>(T response)
            {
                var filtered = _serverActor._filters.Process((Response)(object) response!);
                var buffer = BufferFor(filtered);
                var completedResponse = filtered.Include(_correlationId!);
                _requestResponseContext.RespondWith(completedResponse.Into(buffer));
                return (ICompletes<T>) this;
            }

            private IConsumerByteBuffer BufferFor(Response response)
            {
                var size = response.Size;
                if (size < _serverActor._maxMessageSize)
                {
                    return _serverActor._responseBufferPool.Acquire("ServerActor.BasicCompletedBasedResponseCompletes.BufferFor");
                }

                return BasicConsumerByteBuffer.Allocate(0, size + 1024);
            }
        }
    }
}
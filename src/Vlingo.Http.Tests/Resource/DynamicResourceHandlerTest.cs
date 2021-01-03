// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Http.Resource;
using Vlingo.Wire.Channel;
using Vlingo.Wire.Fdx.Bidirectional;
using Vlingo.Wire.Message;
using Vlingo.Wire.Node;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Http.Tests.Resource
{
    public class DynamicResourceHandlerTest : IDisposable
    {
        private static readonly AtomicInteger BaseServerPort = new AtomicInteger(19080);
        private readonly MemoryStream _buffer = new MemoryStream(65535);
        private readonly IClientRequestResponseChannel _client;
        private readonly Progress _progress;
        private readonly TestResource _resource;
        private readonly IServer _server;
        private readonly World _world;


        [Fact]
        public void TestThatBaseIsSet()
        {
            Assert.NotNull(_resource);
            Assert.NotNull(_resource.Stage);
            Assert.NotNull(_resource.Logger);
            Assert.NotNull(_resource.Scheduler);
            Assert.Null(_resource.Context);
        }

        [Fact]
        public void TestThatContextIsSet()
        {
            var request = GetTestRequest();
            _client.RequestWith(ToByteBuffer(request));
            var consumeCalls = _progress.ExpectConsumeTimes(1);
            
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }
            
            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryDequeue(out var createdResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(Response.ResponseStatus.Ok, createdResponse.Status);

            Assert.NotNull(_resource.Context);
            Assert.NotNull(_resource.Context.Request);
            Assert.Equal("GET", _resource.Context.Request.Method.Name);
            Assert.Equal("/test", _resource.Context.Request.Uri.AbsolutePath);
        }

        public DynamicResourceHandlerTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.StartWithDefaults("test-dynamic-resource-handler");

            _resource = new TestResource(_world.Stage);
            var serverPort = BaseServerPort.GetAndIncrement();
            _server = ServerFactory.StartWithAgent(_world.Stage, Resources.Are(_resource.Routes), serverPort, 2);

            _progress = new Progress();
            
            var consumer = _world.ActorFor<IResponseChannelConsumer>(
                () => new TestResponseChannelConsumer(_progress));
            _client = new BasicClientRequestResponseChannel(
                Address.From(Host.Of("localhost"), serverPort, AddressType.None), consumer, 100, 10240,
                _world.DefaultLogger);
        }
        
        public void Dispose()
        {
            _buffer?.Dispose();
            _client.Close();
            _server.ShutDown();
            _world.Terminate();
        }

        private string GetTestRequest() => "GET /test" + " HTTP/1.1\nHost: vlingo.io\nConnection: close\n\n";

        private byte[] ToByteBuffer(string requestContent)
        {
            _buffer.Clear();
            _buffer.Write(Converters.TextToBytes(requestContent));
            _buffer.Flip();
            return _buffer.ToArray();
        }

        private class TestResource : DynamicResourceHandler
        {
            public TestResource(Stage stage) : base(stage)
            {
            }
            
            public ICompletes<Response> Test() => Vlingo.Common.Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok));

            public override Http.Resource.Resource Routes => ResourceBuilder.Resource("Hello Resource", this,
                ResourceBuilder.Get("/test").Handle(Test));
        }
    }
}
// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Wire.Message;
using Vlingo.Xoom.Wire.Nodes;
using Xunit;
using Xunit.Abstractions;
using Configuration = Vlingo.Xoom.Http.Resource.Configuration;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class ResourceFailureTest : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private static readonly AtomicInteger NextPort = new AtomicInteger(14000);

        private readonly IConsumerByteBuffer _consumerByteBuffer = BasicConsumerByteBuffer.Allocate(5, 1024);
        private Client _client;
        private int _count;
        private readonly int _port;
        private Response _response;
        private readonly IServer _server;
        private readonly World _world;

        [Fact]
        public void TestBasicFailure()
        {
            var consumer = new TestResponseConsumer(_output);
            var access = consumer.AfterCompleting(1);
            var unknown = new UnknownResponseConsumer(access, _output);

            var config =
            Client.Configuration.DefaultedExceptFor(_world.Stage, Address.From(Host.Of("localhost"), _port, AddressType.None), unknown);

            _client = Client.Using(config, Client.ClientConsumerType.RoundRobin, 1);

            var request = Request.From(ToConsumerByteBuffer("GET /fail HTTP/1.1\nHost: vlingo.io\n\n"));

            _count = 0;

            _client.RequestWith(request).AndThenConsume(response => {
                ++_count;
                _response = response;
            }).Await();

            Assert.Equal(1, _count);

            Assert.NotNull(_response);
        }
    
        public ResourceFailureTest(ITestOutputHelper output)
        {
            _output = output;
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.StartWithDefaults("test-request-failure");

            var resource = new FailResource(output);

            _port = NextPort.IncrementAndGet();

            _server = ServerFactory.StartWith(
                _world.Stage,
                Resources.Are(resource.Routes()),
                Filters.None(),
                _port,
                Configuration.SizingConf.DefineConf(),
                Configuration.TimingConf.DefineConf());
        }

        public void Dispose()
        {
            _client.Close();
            _consumerByteBuffer?.Clear();
            _server?.ShutDown();
            _world?.Terminate();
        }

        private IConsumerByteBuffer ToConsumerByteBuffer(string requestContent)
        {
            _consumerByteBuffer.Clear();
            _consumerByteBuffer.Put(Converters.TextToBytes(requestContent));
            _consumerByteBuffer.Flip();
            return _consumerByteBuffer;
        }
    }
}
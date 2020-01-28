// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
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
using Vlingo.Wire.Message;
using Vlingo.Wire.Node;
using Xunit;
using Xunit.Abstractions;
using Configuration = Vlingo.Http.Resource.Configuration;

namespace Vlingo.Http.Tests.Resource
{
    public class ResourceFailureTest : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private static readonly AtomicInteger NextPort = new AtomicInteger(14000);

        private readonly MemoryStream _buffer = new MemoryStream(1024);
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

            var request = Request.From(ToStream("GET /fail HTTP/1.1\nHost: vlingo.io\n\n").ToArray());

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
            _buffer?.Dispose();
            _server?.ShutDown();
            _world?.Terminate();
        }
        
        private MemoryStream ToStream(string requestContent)
        {
            _buffer.Clear();
            _buffer.Write(Converters.TextToBytes(requestContent));
            _buffer.Flip();
            return _buffer;
        }
    }
}
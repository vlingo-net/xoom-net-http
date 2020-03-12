// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
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

namespace Vlingo.Http.Tests.Resource.Feed
{
    public class FeedResourceTest
    {
        private static string FeedURI = "/feeds/events";

        private static readonly AtomicInteger ServerPort = new AtomicInteger(17000);

        private readonly MemoryStream _buffer = new MemoryStream(65535);
        private readonly IClientRequestResponseChannel _client;
        private readonly Progress _progress;

        [Fact]
        public void TestThatFeedResourceFeeds()
        {
            var request = RequestFor($"{FeedURI}/100");

            _client.RequestWith(ToMemoryStream(request).ToArray());

            var consumeCalls = _progress.ExpectConsumeTimes(1);

            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }
            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryDequeue(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(Response.ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal("events:100:1\n2\n3\n4\n5\n", contentResponse.Entity.Content);
        }
        
        public FeedResourceTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            var world = World.StartWithDefaults("test-stream-userResource");

            var testServerPort = ServerPort.IncrementAndGet();

            var properties = new Dictionary<string, string>();
            properties.Add("server.http.port", testServerPort.ToString());
            properties.Add("server.dispatcher.pool", "10");
            properties.Add("server.buffer.pool.size", "100");
            properties.Add("server.message.buffer.size", "65535");
            properties.Add("server.probe.interval", "2");
            properties.Add("server.probe.timeout", "2");
            properties.Add("server.processor.pool.size", "10");
            properties.Add("server.request.missing.content.timeout", "100");

            properties.Add("feed.resource.name.events", FeedURI);
            properties.Add("feed.resource.events.producer.class", "Vlingo.Http.Tests.Resource.Feed.EventsFeedProducerActor");
            properties.Add("feed.resource.events.elements", "5");
            properties.Add("feed.resource.events.pool", "10");
            
            var httpProperties = HttpProperties.Instance;
            httpProperties.SetCustomProperties(properties);

            var server = ServerFactory.StartWith(world.Stage, httpProperties);
            Assert.True(server.StartUp().Await(TimeSpan.FromMilliseconds(500)));

            _progress = new Progress();
            var consumer = world.ActorFor<IResponseChannelConsumer>(Definition.Has<TestResponseChannelConsumer>(Definition.Parameters(_progress)));
            _client = new BasicClientRequestResponseChannel(Address.From(Host.Of("localhost"), testServerPort, AddressType.None), consumer, 100, 10240, world.DefaultLogger);
        }
        
        private string RequestFor(string filePath) => $"GET {filePath} HTTP/1.1\nHost: vlingo.io\n\n";

        private MemoryStream ToMemoryStream(string requestContent)
        {
            _buffer.Clear();
            _buffer.Write(Converters.TextToBytes(requestContent));
            _buffer.Flip();
            return _buffer;
        }
    }
}
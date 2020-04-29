// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;
using Vlingo.Http.Resource;
using Vlingo.Http.Tests.Sample.User.Model;
using Vlingo.Wire.Channel;
using Vlingo.Wire.Fdx.Bidirectional;
using Vlingo.Wire.Node;
using Xunit;
using Xunit.Abstractions;
using Configuration = Vlingo.Http.Resource.Configuration;

namespace Vlingo.Http.Tests.Resource
{
    public class ServerTest : ResourceTestFixtures
    {
        private readonly ITestOutputHelper _output;
        private static readonly int TotalRequestsResponses = 1_000;

        private static readonly AtomicInteger BaseServerPort = new AtomicInteger(18080);
        private readonly int _serverPort;

        private readonly IResponseChannelConsumer _consumer;
        private IClientRequestResponseChannel _client;
        private readonly Progress _progress;
        private readonly IServer _server;
        
        [Fact]
        public void TestThatServerHandlesThrowables()
        {
            var request = GetExceptionRequest("1");
            _client.RequestWith(ToStream(request).ToArray());

            var consumeCalls = _progress.ExpectConsumeTimes(1);
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }
            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryDequeue(out var createdResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(Response.ResponseStatus.InternalServerError, createdResponse.Status);
        }
        
        [Fact]
        public void TestThatServerDispatchesRequests()
        {
            var request = PostRequestCloseFollowing(UniqueJohnDoe());
            _client.RequestWith(ToStream(request).ToArray());

            var consumeCalls = _progress.ExpectConsumeTimes(1);
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }
            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryDequeue(out var createdResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.NotNull(createdResponse.Headers.HeaderOf(ResponseHeader.Location));

            var getUserMessage = $"GET {createdResponse.HeaderOf(ResponseHeader.Location).Value} HTTP/1.1\nHost: vlingo.io\nConnection: keep-alive\n\n";

            _client.RequestWith(ToStream(getUserMessage).ToArray());

            var moreConsumeCalls = _progress.ExpectConsumeTimes(1);
            while (moreConsumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }
            moreConsumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryDequeue(out var getResponse);

            Assert.Equal(2, _progress.ConsumeCount.Get());
            Assert.Equal(Response.ResponseStatus.Ok, getResponse.Status);
            Assert.NotNull(getResponse.Entity);
            Assert.NotNull(getResponse.Entity.Content);
            Assert.True(getResponse.Entity.HasContent);
        }
        
        [Fact]
        public void TestThatServerDispatchesManyRequests()
        {
            var startTime = DateExtensions.GetCurrentMillis();

            var consumeCalls = _progress.ExpectConsumeTimes(TotalRequestsResponses);
            var totalPairs = TotalRequestsResponses / 2;
            var currentConsumeCount = 0;
            for (var idx = 0; idx < totalPairs; ++idx)
            {
                _client.RequestWith(ToStream(PostRequestCloseFollowing(UniqueJohnDoe())).ToArray());
                _client.RequestWith(ToStream(PostRequestCloseFollowing(UniqueJaneDoe())).ToArray());
                var expected = currentConsumeCount + 2;
                while (consumeCalls.TotalWrites < expected)
                {
                    _client.ProbeChannel();
                }
                currentConsumeCount = expected;
            }

            while (consumeCalls.TotalWrites < TotalRequestsResponses)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _output.WriteLine("TOTAL REQUESTS-RESPONSES: {0} TIME: {1} ms", TotalRequestsResponses, DateExtensions.GetCurrentMillis() - startTime);

            Assert.Equal(TotalRequestsResponses, _progress.ConsumeCount.Get());
            _progress.Responses.TryPeek(out var createdResponse);
            Assert.NotNull(createdResponse.Headers.HeaderOf(ResponseHeader.Location));
        }
        
        [Fact]
        public void TestThatServerRespondsPermanentRedirectWithNoContentLengthHeader()
        {
            var request = PutRequest("u-123", UniqueJohnDoe());
            _client.RequestWith(ToStream(request).ToArray());

            var consumeCalls = _progress.ExpectConsumeTimes(1);
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }
            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryPeek(out var response);

            Assert.NotNull(response);
            Assert.Equal(Response.ResponseStatus.PermanentRedirect, response.Status);
            Assert.Equal(1, _progress.ConsumeCount.Get());
        }

        [Fact]
        public void TestThatServerRespondsOkWithNoContentLengthHeader()
        {
            var request = PutRequest("u-456", UniqueJohnDoe());
            _client.RequestWith(ToStream(request).ToArray());

            var consumeCalls = _progress.ExpectConsumeTimes(1);
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }
            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryPeek(out var response);

            Assert.NotNull(response);
            Assert.Equal(Response.ResponseStatus.Ok, response.Status);
            Assert.Equal(1, _progress.ConsumeCount.Get());
        }
        
        [Fact]
        public void TestThatServerClosesChannelAfterSingleRequest()
        {
            var totalResponses = 0;
            var maxRequests = 10;

            for (var count = 0; count < maxRequests; ++count)
            {
                var consumeCalls = _progress.ExpectConsumeTimes(1);
                if (count % 2 == 0)
                {
                    _client.RequestWith(ToStream(PostRequestCloseFollowing(UniqueJohnDoe())).ToArray());
                }
                else
                {
                    _client.RequestWith(ToStream(PostRequestCloseFollowing(UniqueJaneDoe())).ToArray());
                }
                while (consumeCalls.TotalWrites < 1)
                {
                    _client.ProbeChannel();
                }
                totalResponses += consumeCalls.ReadFrom<int>("completed");

                _client.Close();
                
                _client = new BasicClientRequestResponseChannel(Address.From(Host.Of("localhost"), _serverPort, AddressType.None), _consumer, 100, 10240, World.DefaultLogger);
            }

            Assert.Equal(maxRequests, totalResponses);
        }
        
        public ServerTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
            UserStateFactory.ResetId();

            _serverPort = BaseServerPort.GetAndIncrement();
            _server = ServerFactory.StartWith(World.Stage, Resources, _serverPort, new Configuration.SizingConf(1, 1, 100, 10240), new Configuration.TimingConf(25, 1, 1000));
            Assert.True(_server.StartUp().Await(TimeSpan.FromMilliseconds(500L)));

            _progress = new Progress();

            _consumer = World.ActorFor<IResponseChannelConsumer>(() => new TestResponseChannelConsumer(_progress));

            _client = new BasicClientRequestResponseChannel(Address.From(Host.Of("localhost"), _serverPort, AddressType.None), _consumer, 100, 10240, World.DefaultLogger);
        }

        public override void Dispose()
        {
            _client.Close();
            _server.ShutDown();
            
            base.Dispose();
        }
    }
}
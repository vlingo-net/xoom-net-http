// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Http.Tests.Sample.User.Model;
using Vlingo.Xoom.Wire.Channel;
using Vlingo.Xoom.Wire.Fdx.Bidirectional;
using Vlingo.Xoom.Wire.Nodes;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class ServerCorsTest : ResourceTestFixtures
    {
        private readonly ITestOutputHelper _output;
        private static readonly Random Random = new Random();
        private static readonly AtomicInteger BaseServerPort = new AtomicInteger(10_000 + Random.Next(50_000));

        private static readonly string HeaderAcceptOriginAny = "*";
        private static readonly string ResponseHeaderAcceptAllHeaders = "X-Requested-With, Content-Type, Content-Length";
        private static readonly string ResponseHeaderAcceptMethodsAll = "POST,GET,PUT,PATCH,DELETE";

        protected readonly int ServerPort;

        private readonly IClientRequestResponseChannel _client;
        private readonly Progress _progress;
        private readonly IServer _server;
        
        [Fact]
        public void TestThatServerRespondsWithAccessControlHeaders()
        {
            var consumeCalls = _progress.ExpectConsumeTimes(1);

            var request = GetUsersOriginHeader();
            _client.RequestWith(ToStream(request).ToArray());

            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }
            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryDequeue(out var response);

            Assert.NotNull(response);
            Assert.Equal(ResponseStatus.Ok, response.Status);
            Assert.Equal(1, _progress.ConsumeCount.Get());

            Assert.Equal(HeaderAcceptOriginAny, response.HeaderValueOr(ResponseHeader.AccessControlAllowOrigin, null));
            Assert.Equal(ResponseHeaderAcceptAllHeaders, response.HeaderValueOr(ResponseHeader.AccessControlAllowHeaders, null));
            Assert.Equal(ResponseHeaderAcceptMethodsAll, response.HeaderValueOr(ResponseHeader.AccessControlAllowMethods, null));
        }
        
        public ServerCorsTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
            UserStateFactory.ResetId();

            var filter = new CORSResponseFilter();

            var headers = new List<ResponseHeader>
            {
                ResponseHeader.Of(ResponseHeader.AccessControlAllowOrigin, HeaderAcceptOriginAny),
                ResponseHeader.Of(ResponseHeader.AccessControlAllowHeaders, ResponseHeaderAcceptAllHeaders),
                ResponseHeader.Of(ResponseHeader.AccessControlAllowMethods, ResponseHeaderAcceptMethodsAll)
            };

            filter.OriginHeadersFor(HeaderAcceptOriginAny, headers);

            var filters = Filters.Are(Filters.NoRequestFilters(), new []{filter});

            ServerPort = BaseServerPort.GetAndIncrement();
            _server = ServerFactory.StartWithAgent(World.Stage, Resources, filters, ServerPort, 100);
            Assert.True(_server.StartUp().Await(TimeSpan.FromMilliseconds(500)));

            _progress = new Progress();

            var consumer = World.ActorFor<IResponseChannelConsumer>(Definition.Has<TestResponseChannelConsumer>(Definition.Parameters(_progress)));

            _client = new BasicClientRequestResponseChannel(Address.From(Host.Of("localhost"), ServerPort, AddressType.None), consumer, 100, 10240, World.DefaultLogger);
        }

        public override void Dispose()
        {
            _client.Close();

            _server.ShutDown();
            
            base.Dispose();
        }
    }
}
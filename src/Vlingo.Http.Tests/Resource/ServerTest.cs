// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
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
        private static int TOTAL_REQUESTS_RESPONSES = 1_000;

        private static AtomicInteger baseServerPort = new AtomicInteger(18080);

        private IClientRequestResponseChannel client;
        private IResponseChannelConsumer consumer;
        private int serverPort;
        private Progress progress;
        private IServer server;
        
        [Fact]
        public void TestThatServerHandlesThrowables()
        {
            var request = GetExceptionRequest("1");
            client.RequestWith(ToStream(request).ToArray());

            var consumeCalls = progress.ExpectConsumeTimes(1);
            while (consumeCalls.TotalWrites < 1)
            {
                client.ProbeChannel();
            }
            consumeCalls.ReadFrom<int>("completed");

            progress.Responses.TryDequeue(out var createdResponse);

            Assert.Equal(1, progress.ConsumeCount.Get());
            Assert.Equal(Response.ResponseStatus.InternalServerError, createdResponse.Status);
        }
        
        public ServerTest(ITestOutputHelper output) : base(output)
        {
            UserStateFactory.ResetId();

            serverPort = baseServerPort.GetAndIncrement();
            server = ServerFactory.StartWith(World.Stage, Resources, serverPort, new Configuration.SizingConf(1, 1, 100, 10240), new Configuration.TimingConf(1, 1, 100));
            Assert.True(server.StartUp().Await(TimeSpan.FromMilliseconds(500L)));

            progress = new Progress();

            consumer = World.ActorFor<IResponseChannelConsumer>(Definition.Has<TestResponseChannelConsumer>(Definition.Parameters(progress)));

            client = new BasicClientRequestResponseChannel(Address.From(Host.Of("localhost"), serverPort, AddressType.None), consumer, 100, 10240, World.DefaultLogger);
        }

        public override void Dispose()
        {
            client.Close();
            server.ShutDown();
            
            base.Dispose();
        }
    }
}
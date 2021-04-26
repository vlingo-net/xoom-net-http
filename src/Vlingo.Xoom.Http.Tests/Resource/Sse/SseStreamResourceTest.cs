// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Tests.Sample.User;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests.Resource.Sse
{
    public class SseStreamResourceTest : IDisposable
    {
        private readonly MockSseStreamResource _resource;
        private static readonly AtomicInteger NextStreamNumber = new AtomicInteger(0);
        private readonly World _world;

        [Fact]
        public void TestThatClientSubscribes()
        {
            var streamName = NextStreamName();
            var request =
                Request
                    .WithMethod(Method.Get)
                    .WithUri($"/eventstreams/{streamName}")
                    .And(RequestHeader.WithHost("StreamsRUs.co"))
                    .And(RequestHeader.WithAccept("text/event-stream"));

            var respondWithSafely = _resource.RequestResponseContext.Channel.ExpectRespondWith(10);

            _resource.NextRequest(request);

            _resource.SubscribeToStream(streamName, typeof(AllSseFeedActor), 10, 10, "1");

            Assert.Equal(10, respondWithSafely.ReadFrom<int>("count"));

            Assert.Equal(10, _resource.RequestResponseContext.Channel.RespondWithCount.Get());
        }

        [Fact]
        public void TestThatClientUnsubscribes()
        {
            var streamName = NextStreamName();
            var subscribe =
                Request
                    .WithMethod(Method.Get)
                    .WithUri($"/eventstreams/{streamName}")
                    .And(RequestHeader.WithHost("StreamsRUs.co"))
                    .And(RequestHeader.WithAccept("text/event-stream"));

            var respondWithSafely = _resource.RequestResponseContext.Channel.ExpectRespondWith(10);

            _resource.NextRequest(subscribe);

            _resource.SubscribeToStream(streamName, typeof(AllSseFeedActor), 1, 10, "1");

            Assert.True(1 <= respondWithSafely.ReadFrom<int>("count"));
            Assert.True(1 <= _resource.RequestResponseContext.Channel.RespondWithCount.Get());

            var clientId = _resource.RequestResponseContext.Id;

            var unsubscribe =
                Request
                    .WithMethod(Method.Delete)
                    .WithUri($"/eventstreams/{streamName}/{clientId}")
                    .And(RequestHeader.WithHost("StreamsRUs.co"))
                    .And(RequestHeader.WithAccept("text/event-stream"));

            var abandonSafely = _resource.RequestResponseContext.Channel.ExpectAbandon(1);

            _resource.NextRequest(unsubscribe);

            _resource.UnsubscribeFromStream(streamName, clientId);

            Assert.Equal(1, abandonSafely.ReadFrom<int>("count"));
            Assert.Equal(1, _resource.RequestResponseContext.Channel.AbandonCount.Get());
        }

        public SseStreamResourceTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);

            _world = World.StartWithDefaults("test-stream-userResource");
            Configuration.Define();
            _resource = new MockSseStreamResource(_world);
        }
        
        private string NextStreamName() => $"all-{NextStreamNumber.IncrementAndGet()}";

        public void Dispose()
        {
            _world.Terminate();
        }
    }
}
// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Http.Resource.Sse;
using Xunit;

namespace Vlingo.Xoom.Http.Tests.Resource.Sse
{
    public class SseSubscriberTest
    {
        private readonly SseClient _client;
        private readonly MockRequestResponseContext _context;

        [Fact]
        public void TestSubscriberPropertiesBehavior()
        {
            _context.Channel.ExpectRespondWith(1);

            var subscriber = new SseSubscriber("all", _client, "123ABC", "42");

            Assert.NotNull(subscriber.Client);
            Assert.Equal(_context.Id, subscriber.Id);
            Assert.Equal("all", subscriber.StreamName);
            Assert.Equal("123ABC", subscriber.CorrelationId);
            Assert.Equal("42", subscriber.CurrentEventId);
            subscriber.CurrentEventId = "4242";
            Assert.Equal("4242", subscriber.CurrentEventId);
            Assert.True(subscriber.IsCompatibleWith("all"));
            Assert.False(subscriber.IsCompatibleWith("amm"));
            Assert.Equal(0, _context.Channel.AbandonCount.Get());
            var abandonSafely = _context.Channel.ExpectAbandon(1);
            subscriber.Close();
            Assert.Equal(1, abandonSafely.ReadFrom<int>("count"));
            Assert.Equal(1, _context.Channel.AbandonCount.Get());
        }
        
        public SseSubscriberTest()
        {
            Configuration.Define();
            _context = new MockRequestResponseContext(new MockResponseSenderChannel());
            _client = new SseClient(_context);
        }
    }
}
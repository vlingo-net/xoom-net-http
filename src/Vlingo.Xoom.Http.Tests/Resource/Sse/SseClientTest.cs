// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Http.Resource.Sse;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests.Resource.Sse
{
    public class SseClientTest
    {
        private readonly SseClient _client;
        private readonly MockRequestResponseContext _context;
        
        [Fact]
        public void TestThatClientCloses()
        {
            var abandonSafely = _context.Channel.ExpectAbandon(1);

            _client.Close();

            Assert.Equal(1, abandonSafely.ReadFrom<int>("count"));
            Assert.Equal(1, _context.Channel.AbandonCount.Get());
        }
        
        [Fact]
        public void TestThatClientHasId()
        {
            Assert.Equal(_context.Id, _client.Id);
        }

        [Fact]
        public void TestThatSingleEventSends()
        {
            var respondWithSafely = _context.Channel.ExpectRespondWith(1);

            var @event =
                SseEvent.Builder.Instance
                    .WithComment("I like events.")
                    .WithId(1)
                    .WithEvent("E1")
                    .WithData("value")
                    .WithRetry(2500)
                    .ToEvent();

            _client.Send(@event);

            Assert.Equal(1, respondWithSafely.ReadFrom<int>("count"));
            Assert.Equal(1, _context.Channel.RespondWithCount.Get());

            var response = _context.Channel.Response.Get();
            Assert.NotNull(response);
            Assert.NotNull(response.HeaderOf(ResponseHeader.Connection));
            Assert.NotNull(response.HeaderOf(ResponseHeader.ContentType));
            Assert.NotNull(response.HeaderOf(ResponseHeader.CacheControl));

            var eventsResponse = respondWithSafely.ReadFrom<Response>("eventsResponse");

            var messageEvents = MessageEvent.From(eventsResponse);

            Assert.Single(messageEvents);

            var messageEvent = messageEvents[0];

            Assert.Equal("I like events.", messageEvent.Comment);
            Assert.Equal("1", messageEvent.Id);
            Assert.Equal("E1", messageEvent.Event);
            Assert.Equal("value", messageEvent.Data);
            Assert.Equal(2500, messageEvent.Retry);
        }

        [Fact]
        public void TestThatMultipleEventsSends()
        {
            var respondWithSafely = _context.Channel.ExpectRespondWith(1);

            var event1 =
                SseEvent.Builder.Instance
                    .WithComment("I like events.")
                    .WithId(1)
                    .WithEvent("E1")
                    .WithData("value1")
                    .WithRetry(2500)
                    .ToEvent();

            var event2 =
                SseEvent.Builder.Instance
                    .WithComment("I love events.")
                    .WithId(2)
                    .WithEvent("E2")
                    .WithData("value2")
                    .ToEvent();

            var event3 =
                SseEvent.Builder.Instance
                    .WithComment("I <3 events.")
                    .WithId(3)
                    .WithEvent("E3")
                    .WithData("value3")
                    .ToEvent();

            _client.Send(event1, event2, event3);

            Assert.Equal(1, respondWithSafely.ReadFrom<int>("count"));
            Assert.Equal(1, _context.Channel.RespondWithCount.Get());

            var response = _context.Channel.Response.Get();
            Assert.NotNull(response);
            Assert.NotNull(response.HeaderOf(ResponseHeader.Connection));
            Assert.NotNull(response.HeaderOf(ResponseHeader.ContentType));
            Assert.NotNull(response.HeaderOf(ResponseHeader.CacheControl));

            var eventsResponse = respondWithSafely.ReadFrom<Response>("eventsResponse");

            var messageEvents = MessageEvent.From(eventsResponse);

            Assert.Equal(3, messageEvents.Count);

            var messageEvent1 = messageEvents[0];

            Assert.Equal("I like events.", messageEvent1.Comment);
            Assert.Equal("1", messageEvent1.Id);
            Assert.Equal("E1", messageEvent1.Event);
            Assert.Equal("value1", messageEvent1.Data);
            Assert.Equal(2500, messageEvent1.Retry);

            var messageEvent2 = messageEvents[1];
            Assert.Equal("I love events.", messageEvent2.Comment);
            Assert.Equal("2", messageEvent2.Id);
            Assert.Equal("E2", messageEvent2.Event);
            Assert.Equal("value2", messageEvent2.Data);
            Assert.Equal(MessageEvent.NoRetry, messageEvent2.Retry);

            var messageEvent3 = messageEvents[2];
            Assert.Equal("I <3 events.", messageEvent3.Comment);
            Assert.Equal("3", messageEvent3.Id);
            Assert.Equal("E3", messageEvent3.Event);
            Assert.Equal("value3", messageEvent3.Data);
            Assert.Equal(MessageEvent.NoRetry, messageEvent3.Retry);
        }
        
        [Fact]
        public void TestThatEndOfStreamSends()
        {
            var respondWithSafely = _context.Channel.ExpectRespondWith(1);

            var @event = SseEvent.Builder.Instance.WithEndOfStream().ToEvent();

            _client.Send(@event);

            Assert.Equal(1, respondWithSafely.ReadFrom<int>("count"));
            Assert.Equal(1, _context.Channel.RespondWithCount.Get());

            var response = _context.Channel.Response.Get();
            Assert.NotNull(response);

            var eventsResponse = respondWithSafely.ReadFrom<Response>("eventsResponse");

            var messageEvents = MessageEvent.From(eventsResponse);

            Assert.Single(messageEvents);

            var messageEvent = messageEvents[0];

            Assert.True(messageEvent.EndOfStream);
        }

        public SseClientTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);

            Configuration.Define();
            _context = new MockRequestResponseContext(new MockResponseSenderChannel());
            _client = new SseClient(_context);
        }
    }
}
// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Http.Resource.Sse;
using Xunit;

namespace Vlingo.Http.Tests.Resource.Sse
{
    public class SseEventTest
    {
        [Fact]
        public void TestThatEventBuilds()
        {
            var @event =
            SseEvent.Builder.Instance
                .WithComment("I like events.")
                .WithId(1)
                .WithEvent("E1")
                .WithData("{ \"name\" : \"value\" }")
                .WithRetry(2500)
                .ToEvent();

            Assert.Equal("I like events.",  @event.Comment);
            Assert.Equal("1",  @event.Id);
            Assert.Equal("E1",  @event.Event);
            Assert.Equal("{ \"name\" : \"value\" }",  @event.Data);
            Assert.Equal(2500,  @event.Retry);
        }

        [Fact]
        public void TestDefaults()
        {
            var @event = SseEvent.Builder.Instance.ToEvent();

            Assert.Null(@event.Comment);
            Assert.Null(@event.Id);
            Assert.False(@event.HasId);
            Assert.Null(@event.Event);
            Assert.Null(@event.Data);
            Assert.Equal(SseEvent.NoRetry,  @event.Retry);
        }

        [Fact]
        public void TestThatMarksEndOfStream()
        {
            var @event = SseEvent.Builder.Instance.WithEndOfStream().ToEvent();

            Assert.Equal("",  @event.Id);
            Assert.True(@event.EndOfStream);
            Assert.True(string.IsNullOrEmpty(@event.Id));
        }

        [Fact]
        public void TestThatEventIsSendable()
        {
            var @event =
            SseEvent.Builder.Instance
                .WithComment("I like events.")
                .WithId(1)
                .WithEvent("E1")
                .WithData("value")
                .WithRetry(2500)
                .ToEvent();

            Assert.Equal(": I like events.\nid: 1\nevent: E1\ndata: value\nretry: 2500\n\n",  @event.Sendable());
        }

        [Fact]
        public void TestThatEventTranslates()
        {
            var @event =
            SseEvent.Builder.Instance
                .WithComment("I like events.")
                .WithId(1)
                .WithEvent("E1")
                .WithData("value")
                .WithRetry(2500)
                .ToEvent();

            var response =
                Response.Of(
                    Response.ResponseStatus.Ok,
                    Headers.Of(ResponseHeader.WithContentType("text/@event-stream")), 
                @event.Sendable());

            var messageEvents = MessageEvent.From(response);

            Assert.Single(messageEvents);

            var messageEvent = messageEvents[0];

            Assert.Equal("I like events.", messageEvent.Comment);
            Assert.Equal("1", messageEvent.Id);
            Assert.Equal("E1", messageEvent.Event);
            Assert.Equal("value", messageEvent.Data);
            Assert.Equal(2500, messageEvent.Retry);
        }

        [Fact]
        public void TestThatEventTranslatesEndOfStream()
        {
            var @event = SseEvent.Builder.Instance.WithEndOfStream().ToEvent();

            var response =
                Response.Of(
                    Response.ResponseStatus.Ok,
                    Headers.Of(ResponseHeader.WithContentType("text/@event-stream")), 
                @event.Sendable());

            var messageEvents = MessageEvent.From(response);

            Assert.Single(messageEvents);

            var messageEvent = messageEvents[0];

            Assert.True(messageEvent.EndOfStream);
        }

        [Fact]
        public void TestThatEventTranslatesEndOfStreamWithComment()
        {
            var @event = SseEvent.Builder.Instance.WithComment("EOS").WithEndOfStream().ToEvent();

            var response =
                Response.Of(
                    Response.ResponseStatus.Ok,
                    Headers.Of(ResponseHeader.WithContentType("text/@event-stream")), 
                @event.Sendable());

            var messageEvents = MessageEvent.From(response);

            Assert.Single(messageEvents);

            var messageEvent = messageEvents[0];

            Assert.True(messageEvent.EndOfStream);
            Assert.Equal("EOS", messageEvent.Comment);
        }
    }
}
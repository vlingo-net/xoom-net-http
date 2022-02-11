// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Http.Resource.Sse;
using Xunit;

namespace Vlingo.Xoom.Http.Tests.Resource.Sse;

public class MessageEventTest
{
    [Fact]
    public void TestThatMessageEventParses()
    {
        var @event = ": I like events.\nid: 1\nevent: E1\ndata: value\nretry: 2500\n\n";
        var response =
            Response.Of(
                ResponseStatus.Ok,
                Headers.Of(ResponseHeader.WithContentType("text/event-stream")), @event);

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
    public void TestThatMultipleMessageEventsParses()
    {
        var event1 = ": I like events.\nid: 1\nevent: E1\ndata: value1\nretry: 2500\n\n";
        var event2 = ": I love events.\nid: 2\nevent: E2\ndata: value2\n\n";
        var event3 = ": I <3 events.\nid: 3\nevent: E3\ndata: value3\n\n";
        var event4 = "id\n\n";

        var response =
            Response.Of(
                ResponseStatus.Ok,
                Headers.Of(ResponseHeader.WithContentType("text/event-stream")),
                event1 + event2 + event3 + event4);

        var messageEvents = MessageEvent.From(response);

        Assert.Equal(4, messageEvents.Count);

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

        var messageEvent4 = messageEvents[3];
        Assert.True(messageEvent4.EndOfStream);
    }
}
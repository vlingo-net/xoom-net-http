// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Http.Resource.Sse;
using Vlingo.Xoom.Http.Tests.Sample.User;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests.Resource.Sse;

public class SseFeedTest : IDisposable
{
    private readonly SseClient _client;
    private readonly MockRequestResponseContext _context;
    private ISseFeed _feed;
    private readonly World _world;

    [Fact]
    public void TestThatFeedFeedsOneSubscriber()
    {
        var respondWithSafely = _context.Channel.ExpectRespondWith(1);

        _feed = _world.ActorFor<ISseFeed>(() => new AllSseFeedActor("all", 10, "1"));

        var subscriber = new SseSubscriber("all", _client, "ABC123", "42");

        _feed.To(new List<SseSubscriber> {subscriber});

        Assert.Equal(1, respondWithSafely.ReadFrom<int>("count"));

        Assert.Equal(1, _context.Channel.RespondWithCount.Get());

        var eventsResponse = respondWithSafely.ReadFrom<Response>("eventsResponse");

        var events = MessageEvent.From(eventsResponse);

        Assert.Equal(10, events.Count);
    }

    [Fact]
    public void TestThatFeedFeedsMultipleSubscribers()
    {
        _feed = _world.ActorFor<ISseFeed>(() => new AllSseFeedActor("all", 10, "1"));

        var subscriber1 = new SseSubscriber("all", _client, "ABC123", "41");
        var subscriber2 = new SseSubscriber("all", _client, "ABC456", "42");
        var subscriber3 = new SseSubscriber("all", _client, "ABC789", "43");

        var respondWithSafely = _context.Channel.ExpectRespondWith(3);

        _feed.To(new List<SseSubscriber> {subscriber1, subscriber2, subscriber3});

        Assert.Equal(3, respondWithSafely.ReadFrom<int>("count"));

        Assert.Equal(3, _context.Channel.RespondWithCount.Get());
    }
        
    public SseFeedTest(ITestOutputHelper output)
    {
        var converter = new Converter(output);
        Console.SetOut(converter);
            
        _world = World.StartWithDefaults("test-feed");
        Configuration.Define();
        _context = new MockRequestResponseContext(new MockResponseSenderChannel());
        _client = new SseClient(_context);
    }

    public void Dispose()
    {
        _client.Close();
        _world.Terminate();
    }
}
// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource.Feed;

/// <summary>
/// Standard reusable resource for serving feeds.
/// </summary>
public class FeedResource : ResourceHandler
{
    private readonly World _world;
    private readonly Dictionary<string, IFeedProducer> _producers;

    /// <summary>
    /// Construct my default state.
    /// </summary>
    /// <param name="world">The World</param>
    public FeedResource(World world)
    {
        _world = world;
        _producers = new Dictionary<string, IFeedProducer>(2);
    }
        
    public void Feed(string feedName, string feedProductId, Type feedProducerClass, int feedProductElements)
    {
        var producer = FeedProducer(feedName, feedProducerClass);
        if (producer == null)
        {
            Completes?.With(Response.Of(ResponseStatus.NotFound, $"Feed '{feedName}' does not exist."));
        }
        else
        {
            producer.ProduceFeedFor(new FeedProductRequest(Context, feedName, feedProductId, feedProductElements));
        }
    }

    private IFeedProducer FeedProducer(string feedName, Type feedProducerClass)
    {
        if (!_producers.TryGetValue(feedName, out var producer))
        {
            producer = FeedProducerFactory.Using(_world.Stage, feedProducerClass);
            _producers.Add(feedName, producer);
        }
        return producer;
    }
}
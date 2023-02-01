// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource.Feed;

/// <summary>
/// Produce a feed item for a named feed.
/// </summary>
public interface IFeedProducer
{
    /// <summary>
    /// Produce the feed to fulfill the <see cref="FeedProductRequest"/>
    /// </summary>
    /// <param name="request">The FeedProductRequest holding request information</param>
    void ProduceFeedFor(FeedProductRequest request);
}

public static class FeedProducerFactory
{
    /// <summary>
    /// Answer a new <see cref="IFeedProducer"/>
    /// </summary>
    /// <param name="stage">The Stage in which the FeedProducer is created</param>
    /// <param name="feedProducerClass">feedProducerClass</param>
    /// <returns>FeedProducer</returns>
    public static IFeedProducer Using(Stage stage, Type feedProducerClass)
        => stage.ActorFor<IFeedProducer>(feedProducerClass);
}
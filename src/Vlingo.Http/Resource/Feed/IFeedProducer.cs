// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;

namespace Vlingo.Http.Resource.Feed
{
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
        /// <typeparam name="T">Type of the feed producer actor to create</typeparam>
        /// <returns>FeedProducer</returns>
        public static IFeedProducer Using<T>(Stage stage, T feedProducerClass) where T : Actor
            => stage.ActorFor<IFeedProducer>(typeof(T), feedProducerClass);
    }
}
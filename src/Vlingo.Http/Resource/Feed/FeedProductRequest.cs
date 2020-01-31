// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Http.Resource.Feed
{
    public class FeedProductRequest
    {
        /// <summary>
        /// Constructs my state
        /// </summary>
        /// <param name="context">the Context of the original HTTP request</param>
        /// <param name="feedName">The name of the feed from which the product is made</param>
        /// <param name="feedProductId">The identity of the product to feed</param>
        /// <param name="feedProductElements">The maximum number of elements in the product</param>
        public FeedProductRequest(Context context, string feedName, string feedProductId, int feedProductElements)
        {
            Context = context;
            FeedName = feedName;
            FeedProductId = feedProductId;
            FeedProductElements = feedProductElements;
        }
        
        public Context? Context { get; }
        public string? FeedName { get; }
        public int FeedProductElements { get; }
        public string? FeedProductId { get; }
    }
}
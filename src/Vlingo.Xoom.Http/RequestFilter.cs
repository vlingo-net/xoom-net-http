// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Http.Resource;

namespace Vlingo.Http
{
    /// <summary>
    /// A <see cref="Filter"/> for <see cref="Request"/> handling.
    /// </summary>
    public abstract class RequestFilter : Filter
    {
        /// <summary>
        /// Answer the <see cref="Request"/> to be propagated forward to the next <see cref="RequestFilter"/>
        /// or to the <see cref="ResourceHandler"/>, and a <code>bool</code> indicating whether or not the
        /// chain should continue or be short circuited. If the <code>bool</code> is true, the chain
        /// will continue; if false, it will be short circuited.
        /// </summary>
        /// <param name="request">The <see cref="Request"/> to filter</param>
        /// <returns>A pair of <code>(Request, bool)</code></returns>
        public abstract (Request, bool) Filter(Request request);
    }
}
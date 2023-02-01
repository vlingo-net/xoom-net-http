// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Http;

/// <summary>
/// A <see cref="Filter"/> for <see cref="Response"/> handling.
/// </summary>
public abstract class ResponseFilter : Filter
{
    /// <summary>
    /// Answer the <see cref="Response"/> to be propagated forward to the next <see cref="ResponseFilter"/>
    /// or as the final <see cref="Response"/>, and a <code>bool</code> indicating whether or not the
    /// chain should continue or be short circuited. If the <code>bool</code> is true, the chain
    /// will continue; if false, it will be short circuited.
    /// </summary>
    /// <param name="response">The <see cref="Response"/> to filter</param>
    /// <returns>A pair of <code>(Response, bool)</code></returns>
    public abstract (Response, bool) Filter(Response response);
        
    public abstract (Response, bool) Filter(Request request, Response response);
}
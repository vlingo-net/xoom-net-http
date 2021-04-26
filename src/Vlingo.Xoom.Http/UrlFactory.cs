// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Http
{
    public static class UrlFactory
    {
        public static Uri ToMatchableUri(this string uri)
        {
            var pathAndQuery = uri.Contains("?")
                ? uri.Split('?')
                : new []{ uri };
            return pathAndQuery.Length == 1
                ? new UriBuilder("http", "localhost", 80, pathAndQuery[0]).Uri
                : new UriBuilder("http", "localhost", 80, pathAndQuery[0], $"?{pathAndQuery[1]}").Uri;
        }
    }
}
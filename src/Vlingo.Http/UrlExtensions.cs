// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Http
{
    public static class UrlExtensions
    {
        public static Uri ToMatchableUri(this Uri uri)
        {
            var matchableUri = uri;
            if (uri.IsAbsoluteUri && (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                return matchableUri;
            }
            
            var pathAndQuery = uri.OriginalString.Contains("?")
                ? uri.OriginalString.Split('?')
                : new []{ uri.OriginalString };
            matchableUri = pathAndQuery.Length == 1
                ? new UriBuilder("http", "localhost", 80, pathAndQuery[0]).Uri
                : new UriBuilder("http", "localhost", 80, pathAndQuery[0], $"?{pathAndQuery[1]}").Uri;

            return matchableUri;
        }
    }
}
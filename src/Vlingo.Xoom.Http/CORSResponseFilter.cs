// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Xoom.Http
{
    public class CORSResponseFilter : ResponseFilter
    {
        private readonly Dictionary<string, List<ResponseHeader>> _originHeaders;

        public CORSResponseFilter() => _originHeaders = new Dictionary<string, List<ResponseHeader>>();

        /// <summary>
        /// Register the <paramref name="responseHeaders"/> with the <paramref name="originUri"/> such that
        /// when a <see cref="Request"/> contains a RequestHeader of type <code>Origin</code>, the
        /// <see cref="Response"/> will contain the <paramref name="responseHeaders"/>.
        /// </summary>
        /// <param name="originUri">The string URI of a valid CORS origin</param>
        /// <param name="responseHeaders">The list of <see cref="ResponseFilter"/> to set in the Responses for <code>Origin</code> URI</param>
        public void OriginHeadersFor(string originUri, IEnumerable<ResponseHeader> responseHeaders) => 
            _originHeaders.Add(originUri, responseHeaders.ToList());

        public override (Response, bool) Filter(Response response) => new ValueTuple<Response, bool>(response, true);

        public override (Response, bool) Filter(Request request, Response response)
        {
            var origin = request.HeaderValueOr(RequestHeader.Origin, null!);

            if (origin != null)
            {
                foreach (string uri in _originHeaders.Keys)
                {
                    if (uri.Equals(origin))
                    {
                        response.IncludeAll(_originHeaders[origin]);
                        break;
                    }
                }
            }

            return new ValueTuple<Response, bool>(response, true);
        }

        public override void Stop()
        {
        }
    }
}
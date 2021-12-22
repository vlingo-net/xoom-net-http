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
        private Dictionary<string, List<ResponseHeader>> originHeaders;

        public CORSResponseFilter() => this.originHeaders = new Dictionary<string, List<ResponseHeader>>();

        /// <summary>
        /// Register the <paramref name="responseHeaders"/> with the <paramref name="originURI"/> such that
        /// when a <see cref="Request"/> contains a RequestHeader of type <code>Origin</code>, the
        /// <see cref="Response"/> will contain the <paramref name="responseHeaders"/>.
        /// </summary>
        /// <param name="originURI">The string URI of a valid CORS origin</param>
        /// <param name="responseHeaders">The list of <see cref="ResponseFilter"/> to set in the Responses for <code>Origin</code> URI</param>
        public void OriginHeadersFor(string originURI, IEnumerable<ResponseHeader> responseHeaders) => 
            originHeaders.Add(originURI, responseHeaders.ToList());

        public override (Response, bool) Filter(Response response) => new ValueTuple<Response, bool>(response, true);

        public override (Response, bool) Filter(Request request, Response response)
        {
            var origin = request.HeaderValueOr(RequestHeader.Origin, null!);

            if (origin != null)
            {
                foreach (string uri in originHeaders.Keys)
                {
                    if (uri.Equals(origin))
                    {
                        response.IncludeAll(originHeaders[origin]);
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
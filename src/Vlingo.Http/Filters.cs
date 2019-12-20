// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Http
{
    public class Filters
    {
        private readonly List<RequestFilter> _requestFilters;
        private readonly List<ResponseFilter> _responseFilters;
        private bool _stopped;
        
        /// <summary>
        /// Answer a new <see cref="Filters"/> for <paramref name="requestFilters"/> and <paramref name="responseFilters"/>.
        /// </summary>
        /// <param name="requestFilters">The collection of <see cref="RequestFilter"/></param>
        /// <param name="responseFilters">The collection of <see cref="ResponseFilter"/></param>
        /// <returns><see cref="Filters"/></returns>
        public static Filters Are(IEnumerable<RequestFilter> requestFilters, IEnumerable<ResponseFilter> responseFilters)
            => new Filters(requestFilters, responseFilters);
        
        /// <summary>
        /// Answer an empty <see cref="Filters"/> instance.
        /// </summary>
        /// <returns><see cref="Filters"/></returns>
        public static Filters None() => new Filters(Enumerable.Empty<RequestFilter>(), Enumerable.Empty<ResponseFilter>());
        
        /// <summary>
        /// Answer any empty <code>IEnumerable{RequestFilter}</code>.
        /// </summary>
        /// <returns><code>IEnumerable{RequestFilter}</code></returns>
        public static IEnumerable<RequestFilter> NoRequestFilters() => Enumerable.Empty<RequestFilter>();

        /// <summary>
        /// Answer any empty <code>IEnumerable{ResponseFilter}</code>.
        /// </summary>
        /// <returns><code>IEnumerable{ResponseFilter}</code></returns>
        public static IEnumerable<ResponseFilter> NoResponseFilters() => Enumerable.Empty<ResponseFilter>();

        /// <summary>
        /// Answer the <see cref="Request"/> resulting from any filtering.
        /// </summary>
        /// <param name="request">The <see cref="Request"/> incoming from the client</param>
        /// <returns><see cref="Request"/></returns>
        public Request Process(Request request)
        {
            if (_stopped)
            {
                return request;
            }

            var current = request;

            foreach (var filter in _requestFilters)
            {
                var (filteredRequest, isFiltered) = filter.Filter(current);
                
                if (!isFiltered)
                {
                    return filteredRequest;
                }

                current = filteredRequest;
            }
            
            return current;
        }

        /// <summary>
        /// Answer the <see cref="Response"/> resulting from any filtering.
        /// </summary>
        /// <param name="response">The <see cref="Response"/> outgoing from a <see cref="ResponseHeader"/></param>
        /// <returns><see cref="Response"/></returns>
        public Response Process(Response response)
        {
            if (_stopped)
            {
                return response;
            }

            var current = response;

            foreach (var filter in _responseFilters)
            {
                var (filteredResponse, isFiltered) = filter.Filter(current);
                
                if (!isFiltered)
                {
                    return filteredResponse;
                }

                current = filteredResponse;
            }
            
            return current;
        }
        
        /// <summary>
        /// Stop all filters.
        /// </summary>
        public void Stop()
        {
            if (_stopped)
            {
                return;
            }

            _stopped = true;

            foreach (var filter in _requestFilters)
            {
                filter.Stop();
            }

            foreach (var filter in _responseFilters)
            {
                filter.Stop();
            }
        }

        /// <summary>
        /// Constructs my state.
        /// </summary>
        /// <param name="requestFilters">The <code>IEnumerable{RequestFilter}</code> of request filters</param>
        /// <param name="responseFilters">The <code>IEnumerable{ResponseFilter}</code> of response filters</param>
        private Filters(IEnumerable<RequestFilter> requestFilters, IEnumerable<ResponseFilter> responseFilters)
        {
            _requestFilters = requestFilters.ToList();
            _responseFilters = responseFilters.ToList();
            _stopped = false;
        }
    }
}
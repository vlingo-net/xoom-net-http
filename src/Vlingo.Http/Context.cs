// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Wire.Channel;

namespace Vlingo.Http
{
    public class Context
    {
        private readonly RequestResponseContext<object> _requestResponseContext;
        private readonly Request _request;
        private readonly ICompletesEventually _completes;

        public Context(RequestResponseContext<object> requestResponseContext, Request request, ICompletesEventually completes)
        {
            _requestResponseContext = requestResponseContext;
            _request = request;
            _completes = completes;
        }

        public Context(Request request, ICompletesEventually completes)
            : this(null, request, completes)
        {
        }

        public Context(ICompletesEventually completes)
            : this(null, completes)
        {
        }

        public RequestResponseContext<object> ClientContext => _requestResponseContext;

        public bool HasClientContext => _requestResponseContext != null;

        public Request Request => _request;

        public bool HasRequest => _request != null;
    }
}

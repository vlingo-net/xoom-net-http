// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Linq;

namespace Vlingo.Http.Resource
{
    public static class ResourceBuilder
    {
        public static Resource<ResourceHandler> Resource(string name, params RequestHandler[] requestHandlers)
            => Resource(name, 10, requestHandlers);

        public static Resource<ResourceHandler> Resource(string name, int handlerPoolSize, params RequestHandler[] requestHandlers)
            => new DynamicResource(name, handlerPoolSize, requestHandlers.ToList());

        public static RequestHandler0 Get(string uri)
            => new RequestHandler0(Method.GET, uri);

        public static RequestHandler0 Post(string uri)
            => new RequestHandler0(Method.POST, uri);

        public static RequestHandler0 Put(string uri)
            => new RequestHandler0(Method.PUT, uri);

        public static RequestHandler0 Delete(string uri)
            => new RequestHandler0(Method.DELETE, uri);

        public static RequestHandler0 Patch(string uri)
            => new RequestHandler0(Method.PATCH, uri);

        public static RequestHandler0 Head(string uri)
            => new RequestHandler0(Method.HEAD, uri);

        public static RequestHandler0 Options(string uri)
            => new RequestHandler0(Method.OPTIONS, uri);

        public static RequestHandler0 Trace(string uri)
            => new RequestHandler0(Method.TRACE, uri);

        public static RequestHandler0 Connect(string uri)
            => new RequestHandler0(Method.CONNECT, uri);
    }
}

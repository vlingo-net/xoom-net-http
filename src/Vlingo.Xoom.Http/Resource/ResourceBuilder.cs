// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Linq;

namespace Vlingo.Xoom.Http.Resource;

public static class ResourceBuilder
{
    public static Resource Resource(string name, params RequestHandler[] requestHandlers)
        => Resource(name, null, 10, requestHandlers);
        
    public static Resource Resource(string name, int handlerPoolSize, params RequestHandler[] requestHandlers)
        => Resource(name, null, 10, requestHandlers);
        
    public static Resource Resource(string name, DynamicResourceHandler? dynamicResourceHandler, params RequestHandler[] requestHandlers)
        => new DynamicResource(name, dynamicResourceHandler, 10, requestHandlers.ToList());

    public static Resource Resource(string name, DynamicResourceHandler? dynamicResourceHandler, int handlerPoolSize, params RequestHandler[] requestHandlers)
        => new DynamicResource(name, dynamicResourceHandler, handlerPoolSize, requestHandlers.ToList());

    public static RequestHandler0 Get(string uri)
        => new RequestHandler0(Method.Get, uri);

    public static RequestHandler0 Post(string uri)
        => new RequestHandler0(Method.Post, uri);

    public static RequestHandler0 Put(string uri)
        => new RequestHandler0(Method.Put, uri);

    public static RequestHandler0 Delete(string uri)
        => new RequestHandler0(Method.Delete, uri);

    public static RequestHandler0 Patch(string uri)
        => new RequestHandler0(Method.Patch, uri);

    public static RequestHandler0 Head(string uri)
        => new RequestHandler0(Method.Head, uri);

    public static RequestHandler0 Options(string uri)
        => new RequestHandler0(Method.Options, uri);

    public static RequestHandler0 Trace(string uri)
        => new RequestHandler0(Method.Trace, uri);

    public static RequestHandler0 Connect(string uri)
        => new RequestHandler0(Method.Connect, uri);
}
// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http.Resource;

public class RequestHandler2<T, TR> : RequestHandler
{
    internal ParameterResolver<T> ResolverParam1 { get; }
    internal ParameterResolver<TR> ResolverParam2 { get; }
    private ParamExecutor2? _executor;

    public delegate ICompletes<Response> Handler2(T param1, TR param2);
    public delegate ICompletes<IObjectResponse> ObjectHandler2(T param1, TR param2);
    internal delegate ICompletes<Response> ParamExecutor2(
        Request request,
        T param1,
        TR param2,
        MediaTypeMapper mediaTypeMapper,
        IErrorHandler errorHandler,
        ILogger logger);

    internal RequestHandler2(
        Method method,
        string path,
        ParameterResolver<T> resolverParam1,
        ParameterResolver<TR> resolverParam2,
        IErrorHandler errorHandler,
        MediaTypeMapper mediaTypeMapper)
        : base(method, path, new List<IParameterResolver> { resolverParam1, resolverParam2 }, errorHandler, mediaTypeMapper)
    {
        ResolverParam1 = resolverParam1;
        ResolverParam2 = resolverParam2;
    }

    internal ICompletes<Response> Execute(Request request, T param1, TR param2, ILogger logger)
    {
        Func<ICompletes<Response>> exec = ()
            => _executor?.Invoke(request, param1, param2, MediaTypeMapper, ErrorHandler, logger)!;

        return RunParamExecutor(_executor, () => RequestExecutor.ExecuteRequest(exec, ErrorHandler, logger));
    }

    public RequestHandler2<T, TR> Handle(Handler2 handler)
    {
        _executor = (request, param1, param2, mediaTypeMapper1, errorHandler1, logger1)
            => RequestExecutor.ExecuteRequest(() => handler.Invoke(param1, param2), errorHandler1, logger1);
        return this;
    }

    public RequestHandler2<T, TR> Handle(ObjectHandler2 handler)
    {
        _executor = (request, param1, param2, mediaTypeMapper1, errorHandler1, logger)
            => RequestObjectExecutor.ExecuteRequest(
                request,
                mediaTypeMapper1,
                () => handler.Invoke(param1, param2),
                errorHandler1,
                logger);
        return this;
    }

    public RequestHandler2<T, TR> OnError(IErrorHandler errorHandler)
    {
        ErrorHandler = errorHandler;
        return this;
    }

    internal override ICompletes<Response> Execute(
        Request request,
        Action.MappedParameters mappedParameters,
        ILogger logger)
    {
        var param1 = ResolverParam1.Apply(request, mappedParameters);
        var param2 = ResolverParam2.Apply(request, mappedParameters);
        return Execute(request, param1, param2, logger);
    }

    public RequestHandler3<T, TR, TU> Param<TU>()
        => new RequestHandler3<T, TR, TU>(Method, Path, ResolverParam1, ResolverParam2, ParameterResolver.Path<TU>(2), ErrorHandler, MediaTypeMapper);

    public RequestHandler3<T, TR, TU> Body<TU>()
        => new RequestHandler3<T, TR, TU>(Method, Path, ResolverParam1, ResolverParam2, ParameterResolver.Body<TU>(MediaTypeMapper), ErrorHandler, MediaTypeMapper);

    [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<U>(Type, MediaTypeMapper) or Body<U>(Type).")]
    public RequestHandler3<T, TR, TU> Body<TU>(Type mapperClass)
        => Body<TU>(MapperFrom(mapperClass));

    [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<U>(Type, MediaTypeMapper) or Body<U>(Type).")]
    public RequestHandler3<T, TR, TU> Body<TU>(IMapper mapper)
        => new RequestHandler3<T, TR, TU>(
            Method, 
            Path, 
            ResolverParam1, 
            ResolverParam2,
            ParameterResolver.Body<TU>(mapper),
            ErrorHandler,
            MediaTypeMapper);

    public RequestHandler3<T, TR, TU> Body<TU>(MediaTypeMapper mediaTypeMapper)
    {
        MediaTypeMapper = mediaTypeMapper;
        return new RequestHandler3<T,TR,TU>(
            Method, 
            Path, 
            ResolverParam1, 
            ResolverParam2,
            ParameterResolver.Body<TU>(mediaTypeMapper), 
            ErrorHandler, 
            MediaTypeMapper);
    }

    public RequestHandler3<T, TR, string> Query(string name)
        => Query<string>(name, typeof(string));

    public RequestHandler3<T, TR, TU> Query<TU>(string name, Type queryClass)
        => new RequestHandler3<T,TR,TU>(Method, Path, ResolverParam1, ResolverParam2, ParameterResolver.Query<TU>(name), ErrorHandler, MediaTypeMapper);

    public RequestHandler3<T, TR, Header> Header(string name)
        => new RequestHandler3<T, TR, Header>(Method, Path, ResolverParam1, ResolverParam2, ParameterResolver.Header(name), ErrorHandler, MediaTypeMapper);
}
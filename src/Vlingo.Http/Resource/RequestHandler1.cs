// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Common;

namespace Vlingo.Http.Resource
{
    public class RequestHandler1<T> : RequestHandler
    {
        internal ParameterResolver<T> Resolver { get; }
        private ParamExecutor1 _executor;

        public delegate ICompletes<Response> Handler1(T param1);
        public delegate ICompletes<IObjectResponse> ObjectHandler1(T param1);
        internal delegate ICompletes<Response> ParamExecutor1(
            Request request,
            T param1,
            MediaTypeMapper mediaTypeMapper,
            IErrorHandler errorHandler,
            ILogger logger);

        internal RequestHandler1(
            Method method,
            string path,
            ParameterResolver<T> resolver,
            IErrorHandler errorHandler,
            MediaTypeMapper mediaTypeMapper)
            : base(method, path, new List<IParameterResolver> { resolver }, errorHandler, mediaTypeMapper)
        {
            Resolver = resolver;
        }

        public RequestHandler1<T> Handle(Handler1 handler)
        {
            _executor = (request, param1, mediaTypeMapper1, errorHandler1, logger1)
                => RequestExecutor.ExecuteRequest(() => handler.Invoke(param1), errorHandler1, logger1);
            return this;
        }

        public RequestHandler1<T> Handle(ObjectHandler1 handler)
        {
            _executor = (request, param1, mediaTypeMapper1, errorHandler1, logger)
                => RequestObjectExecutor.ExecuteRequest(request, mediaTypeMapper1, () => handler.Invoke(param1), errorHandler1, logger);
            return this;
        }

        public RequestHandler1<T> OnError(IErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler;
            return this;
        }

        internal ICompletes<Response> Execute(Request request, T param1, ILogger logger)
        {
            Func<ICompletes<Response>> exec = ()
                => _executor.Invoke(request, param1, MediaTypeMapper, ErrorHandler, logger);

            return RunParamExecutor(_executor, () => RequestExecutor.ExecuteRequest(exec, ErrorHandler, logger));
        }

        internal override ICompletes<Response> Execute(Request request, Action.MappedParameters mappedParameters, ILogger logger)
            => Execute(request, Resolver.Apply(request, mappedParameters), logger);

        public RequestHandler2<T, R> Param<R>(Type paramClass)
            => new RequestHandler2<T, R>(Method, Path, Resolver, ParameterResolver.Path<R>(1, paramClass), ErrorHandler, MediaTypeMapper);

        public RequestHandler2<T, R> Body<R>(Type bodyClass)
            => new RequestHandler2<T, R>(
                Method,
                Path,
                Resolver,
                ParameterResolver.Body<R>(bodyClass, MediaTypeMapper),
                ErrorHandler,
                MediaTypeMapper);

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<R>(Type, MediaTypeMapper) or Body<R>(Type).")]
        public RequestHandler2<T, R> Body<R>(Type bodyClass, Type mapperClass)
            => Body<R>(bodyClass, MapperFrom(mapperClass));

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<R>(Type, MediaTypeMapper) or Body<R>(Type).")]
        public RequestHandler2<T, R> Body<R>(Type bodyClass, IMapper mapper)
            => new RequestHandler2<T, R>(
                Method,
                Path,
                Resolver,
                ParameterResolver.Body<R>(bodyClass, mapper),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler2<T, R> Body<R>(Type bodyClass, MediaTypeMapper mediaTypeMapper)
        {
            MediaTypeMapper = mediaTypeMapper;
            return new RequestHandler2<T, R>(
                Method,
                Path,
                Resolver,
                ParameterResolver.Body<R>(bodyClass, mediaTypeMapper),
                ErrorHandler,
                mediaTypeMapper);
        }

        public RequestHandler2<T, string> Query(string name)
            => Query<string>(name, typeof(string));

        public RequestHandler2<T, R> Query<R>(string name, Type queryClass)
            => Query(name, queryClass, default(R)!);

        public RequestHandler2<T, R> Query<R>(string name, Type queryClass, R defaultValue)
            => new RequestHandler2<T, R>(
                Method,
                Path,
                Resolver,
                ParameterResolver.Query(name, queryClass, defaultValue),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler2<T, Header> Header(string name)
            => new RequestHandler2<T, Header>(Method, Path, Resolver, ParameterResolver.Header(name), ErrorHandler, MediaTypeMapper);
    }
}

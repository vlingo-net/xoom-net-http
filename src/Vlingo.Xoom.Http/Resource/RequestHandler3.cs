// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Http.Resource
{
    public class RequestHandler3<T, R, U> : RequestHandler
    {
        internal ParameterResolver<T> ResolverParam1 { get; }
        internal ParameterResolver<R> ResolverParam2 { get; }
        internal ParameterResolver<U> ResolverParam3 { get; }
        private ParamExecutor3? _executor;

        public delegate ICompletes<Response> Handler3(T param1, R param2, U param3);
        public delegate ICompletes<IObjectResponse> ObjectHandler3(T param1, R param2, U param3);
        internal delegate ICompletes<Response> ParamExecutor3(
            Request request,
            T param1,
            R param2,
            U param3,
            MediaTypeMapper mediaTypeMapper,
            IErrorHandler errorHandler,
            ILogger logger);

        internal RequestHandler3(
            Method method,
            string path,
            ParameterResolver<T> resolverParam1,
            ParameterResolver<R> resolverParam2,
            ParameterResolver<U> resolverParam3,
            IErrorHandler errorHandler,
            MediaTypeMapper mediaTypeMapper)
            : base(method, path, new List<IParameterResolver> { resolverParam1, resolverParam2, resolverParam3 }, errorHandler, mediaTypeMapper)
        {
            ResolverParam1 = resolverParam1;
            ResolverParam2 = resolverParam2;
            ResolverParam3 = resolverParam3;
        }

        internal ICompletes<Response> Execute(Request request, T param1, R param2, U param3, ILogger logger)
        {
            Func<ICompletes<Response>> exec = ()
                => _executor?.Invoke(request, param1, param2, param3, MediaTypeMapper, ErrorHandler, logger)!;

            return RunParamExecutor(_executor, () => RequestExecutor.ExecuteRequest(exec, ErrorHandler, logger));
        }

        public RequestHandler3<T, R, U>? Handle(Handler3 handler)
        {
            _executor = (request, param1, param2, param3, mediaTypeMapper1, errorHandler1, logger1)
                => RequestExecutor.ExecuteRequest(() => handler.Invoke(param1, param2, param3), errorHandler1, logger1);
            return this;
        }

        public RequestHandler3<T, R, U> Handle(ObjectHandler3 handler)
        {
            _executor = (request, param1, param2, param3, mediaTypeMapper1, errorHandler1, logger)
                => RequestObjectExecutor.ExecuteRequest(
                    request,
                    mediaTypeMapper1,
                    () => handler.Invoke(param1, param2, param3),
                    errorHandler1,
                    logger);
            return this;
        }

        public RequestHandler3<T, R, U> OnError(IErrorHandler errorHandler)
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
            var param3 = ResolverParam3.Apply(request, mappedParameters);
            return Execute(request, param1, param2, param3, logger);
        }

        public RequestHandler4<T, R, U, I> Param<I>()
            => new RequestHandler4<T, R, U, I>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Path<I>(3),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler4<T, R, U, I> Body<I>()
            => new RequestHandler4<T, R, U, I>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Body<I>(MediaTypeMapper),
                ErrorHandler,
                MediaTypeMapper);

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<I>(Type, MediaTypeMapper) or Body<I>(Type).")]
        public RequestHandler4<T, R, U, I> Body<I>(Type mapperClass)
            => Body<I>(MapperFrom(mapperClass));

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<I>(Type, MediaTypeMapper) or Body<I>(Type).")]
        public RequestHandler4<T, R, U, I> Body<I>(IMapper mapper)
            => new RequestHandler4<T, R, U, I>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Body<I>(mapper),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler4<T, R, U, I> Body<I>(MediaTypeMapper mediaTypeMapper)
        {
            MediaTypeMapper = mediaTypeMapper;
            return new RequestHandler4<T, R, U, I>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Body<I>(mediaTypeMapper),
                ErrorHandler,
                mediaTypeMapper);
        }

        public RequestHandler4<T, R, U, string> Query(string name)
            => Query<string>(name, typeof(string));

        public RequestHandler4<T, R, U, I> Query<I>(string name, Type queryClass)
            => new RequestHandler4<T, R, U, I>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Query<I>(name),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler4<T, R, U, Header> Header(string name)
            => new RequestHandler4<T, R, U, Header>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Header(name),
                ErrorHandler,
                MediaTypeMapper);

    }
}

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
    public class RequestHandler4<T, R, U, I> : RequestHandler
    {
        internal ParameterResolver<T> ResolverParam1 { get; }
        internal ParameterResolver<R> ResolverParam2 { get; }
        internal ParameterResolver<U> ResolverParam3 { get; }
        internal ParameterResolver<I> ResolverParam4 { get; }
        private ParamExecutor4 _executor;

        public delegate ICompletes<Response> Handler4(T param1, R param2, U param3, I param4);
        public delegate ICompletes<IObjectResponse> ObjectHandler4(T param1, R param2, U param3, I param4);
        internal delegate ICompletes<Response> ParamExecutor4(
            Request request,
            T param1,
            R param2,
            U param3,
            I param4,
            MediaTypeMapper mediaTypeMapper,
            IErrorHandler errorHandler,
            ILogger logger);

        internal RequestHandler4(
            Method method,
            string path,
            ParameterResolver<T> resolverParam1,
            ParameterResolver<R> resolverParam2,
            ParameterResolver<U> resolverParam3,
            ParameterResolver<I> resolverParam4,
            IErrorHandler errorHandler,
            MediaTypeMapper mediaTypeMapper)
            : base(method, path, new List<IParameterResolver> { resolverParam1, resolverParam2, resolverParam3, resolverParam4 }, errorHandler, mediaTypeMapper)
        {
            ResolverParam1 = resolverParam1;
            ResolverParam2 = resolverParam2;
            ResolverParam3 = resolverParam3;
            ResolverParam4 = resolverParam4;
        }

        internal ICompletes<Response> Execute(Request request, T param1, R param2, U param3, I param4, ILogger logger)
        {
            Func<ICompletes<Response>> exec = ()
                => _executor.Invoke(request, param1, param2, param3, param4, MediaTypeMapper, ErrorHandler, logger);

            return RunParamExecutor(_executor, () => RequestExecutor.ExecuteRequest(exec, ErrorHandler, logger));
        }

        public RequestHandler4<T, R, U, I> Handle(Handler4 handler)
        {
            _executor = (request, param1, param2, param3, param4, mediaTypeMapper1, errorHandler1, logger1)
                => RequestExecutor.ExecuteRequest(() => handler.Invoke(param1, param2, param3, param4), errorHandler1, logger1);
            return this;
        }

        public RequestHandler4<T, R, U, I> Handle(ObjectHandler4 handler)
        {
            _executor = (request, param1, param2, param3, param4, mediaTypeMapper1, errorHandler1, logger)
                => RequestObjectExecutor.ExecuteRequest(
                    request,
                    mediaTypeMapper1,
                    () => handler.Invoke(param1, param2, param3, param4),
                    errorHandler1,
                    logger);
            return this;
        }

        public RequestHandler4<T, R, U, I> OnError(IErrorHandler errorHandler)
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
            var param4 = ResolverParam4.Apply(request, mappedParameters);
            return Execute(request, param1, param2, param3, param4, logger);
        }

        public RequestHandler5<T, R, U, I, J> Param<J>(Type paramClass)
            => new RequestHandler5<T, R, U, I, J>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Path<J>(4, paramClass),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler5<T, R, U, I, J> Body<J>(Type bodyClass)
            => new RequestHandler5<T, R, U, I, J>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Body<J>(bodyClass, MediaTypeMapper),
                ErrorHandler,
                MediaTypeMapper);

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<J>(Type, MediaTypeMapper) or Body<J>(Type).")]
        public RequestHandler5<T, R, U, I, J> Body<J>(Type bodyClass, Type mapperClass)
            => Body<J>(bodyClass, MapperFrom(mapperClass));

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<J>(Type, MediaTypeMapper) or Body<J>(Type).")]
        public RequestHandler5<T, R, U, I, J> Body<J>(Type bodyClass, IMapper mapper)
            => new RequestHandler5<T, R, U, I, J>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Body<J>(bodyClass, mapper),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler5<T, R, U, I, J> Body<J>(Type bodyClass, MediaTypeMapper mediaTypeMapper)
            => new RequestHandler5<T, R, U, I, J>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Body<J>(bodyClass, mediaTypeMapper),
                ErrorHandler,
                mediaTypeMapper);

        public RequestHandler5<T, R, U, I, string> Query(string name)
            => Query<string>(name, typeof(string));

        public RequestHandler5<T, R, U, I, J> Query<J>(string name, Type queryClass)
            => new RequestHandler5<T, R, U, I, J>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Query<J>(name, queryClass),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler5<T, R, U, I, Header> Header(string name)
            => new RequestHandler5<T, R, U, I Header>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Header(name),
                ErrorHandler,
                MediaTypeMapper);
    }
}

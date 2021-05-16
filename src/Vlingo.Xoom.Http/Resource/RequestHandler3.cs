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

namespace Vlingo.Xoom.Http.Resource
{
    public class RequestHandler3<T, TR, TU> : RequestHandler
    {
        internal ParameterResolver<T> ResolverParam1 { get; }
        internal ParameterResolver<TR> ResolverParam2 { get; }
        internal ParameterResolver<TU> ResolverParam3 { get; }
        private ParamExecutor3? _executor;

        public delegate ICompletes<Response> Handler3(T param1, TR param2, TU param3);
        public delegate ICompletes<IObjectResponse> ObjectHandler3(T param1, TR param2, TU param3);
        internal delegate ICompletes<Response> ParamExecutor3(
            Request request,
            T param1,
            TR param2,
            TU param3,
            MediaTypeMapper mediaTypeMapper,
            IErrorHandler errorHandler,
            ILogger logger);

        internal RequestHandler3(
            Method method,
            string path,
            ParameterResolver<T> resolverParam1,
            ParameterResolver<TR> resolverParam2,
            ParameterResolver<TU> resolverParam3,
            IErrorHandler errorHandler,
            MediaTypeMapper mediaTypeMapper)
            : base(method, path, new List<IParameterResolver> { resolverParam1, resolverParam2, resolverParam3 }, errorHandler, mediaTypeMapper)
        {
            ResolverParam1 = resolverParam1;
            ResolverParam2 = resolverParam2;
            ResolverParam3 = resolverParam3;
        }

        internal ICompletes<Response> Execute(Request request, T param1, TR param2, TU param3, ILogger logger)
        {
            Func<ICompletes<Response>> exec = ()
                => _executor?.Invoke(request, param1, param2, param3, MediaTypeMapper, ErrorHandler, logger)!;

            return RunParamExecutor(_executor, () => RequestExecutor.ExecuteRequest(exec, ErrorHandler, logger));
        }

        public RequestHandler3<T, TR, TU> Handle(Handler3 handler)
        {
            _executor = (request, param1, param2, param3, mediaTypeMapper1, errorHandler1, logger1)
                => RequestExecutor.ExecuteRequest(() => handler.Invoke(param1, param2, param3), errorHandler1, logger1);
            return this;
        }

        public RequestHandler3<T, TR, TU> Handle(ObjectHandler3 handler)
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

        public RequestHandler3<T, TR, TU> OnError(IErrorHandler errorHandler)
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

        public RequestHandler4<T, TR, TU, TI> Param<TI>()
            => new RequestHandler4<T, TR, TU, TI>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Path<TI>(3),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler4<T, TR, TU, TI> Body<TI>()
            => new RequestHandler4<T, TR, TU, TI>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Body<TI>(MediaTypeMapper),
                ErrorHandler,
                MediaTypeMapper);

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<I>(Type, MediaTypeMapper) or Body<I>(Type).")]
        public RequestHandler4<T, TR, TU, TI> Body<TI>(Type mapperClass)
            => Body<TI>(MapperFrom(mapperClass));

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<I>(Type, MediaTypeMapper) or Body<I>(Type).")]
        public RequestHandler4<T, TR, TU, TI> Body<TI>(IMapper mapper)
            => new RequestHandler4<T, TR, TU, TI>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Body<TI>(mapper),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler4<T, TR, TU, TI> Body<TI>(MediaTypeMapper mediaTypeMapper)
        {
            MediaTypeMapper = mediaTypeMapper;
            return new RequestHandler4<T, TR, TU, TI>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Body<TI>(mediaTypeMapper),
                ErrorHandler,
                mediaTypeMapper);
        }

        public RequestHandler4<T, TR, TU, string> Query(string name)
            => Query<string>(name, typeof(string));

        public RequestHandler4<T, TR, TU, TI> Query<TI>(string name, Type queryClass)
            => new RequestHandler4<T, TR, TU, TI>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ParameterResolver.Query<TI>(name),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler4<T, TR, TU, Header> Header(string name)
            => new RequestHandler4<T, TR, TU, Header>(
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

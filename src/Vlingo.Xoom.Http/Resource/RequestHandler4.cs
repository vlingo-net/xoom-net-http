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

namespace Vlingo.Xoom.Http.Resource
{
    public class RequestHandler4<T, TR, TU, TI> : RequestHandler
    {
        internal ParameterResolver<T> ResolverParam1 { get; }
        internal ParameterResolver<TR> ResolverParam2 { get; }
        internal ParameterResolver<TU> ResolverParam3 { get; }
        internal ParameterResolver<TI> ResolverParam4 { get; }
        private ParamExecutor4? _executor;

        public delegate ICompletes<Response> Handler4(T param1, TR param2, TU param3, TI param4);
        public delegate ICompletes<IObjectResponse> ObjectHandler4(T param1, TR param2, TU param3, TI param4);
        internal delegate ICompletes<Response> ParamExecutor4(
            Request request,
            T param1,
            TR param2,
            TU param3,
            TI param4,
            MediaTypeMapper mediaTypeMapper,
            IErrorHandler errorHandler,
            ILogger logger);

        internal RequestHandler4(
            Method method,
            string path,
            ParameterResolver<T> resolverParam1,
            ParameterResolver<TR> resolverParam2,
            ParameterResolver<TU> resolverParam3,
            ParameterResolver<TI> resolverParam4,
            IErrorHandler errorHandler,
            MediaTypeMapper mediaTypeMapper)
            : base(method, path, new List<IParameterResolver> { resolverParam1, resolverParam2, resolverParam3, resolverParam4 }, errorHandler, mediaTypeMapper)
        {
            ResolverParam1 = resolverParam1;
            ResolverParam2 = resolverParam2;
            ResolverParam3 = resolverParam3;
            ResolverParam4 = resolverParam4;
        }

        internal ICompletes<Response> Execute(Request request, T param1, TR param2, TU param3, TI param4, ILogger logger)
        {
            Func<ICompletes<Response>> exec = ()
                => _executor?.Invoke(request, param1, param2, param3, param4, MediaTypeMapper, ErrorHandler, logger)!;

            return RunParamExecutor(_executor, () => RequestExecutor.ExecuteRequest(exec, ErrorHandler, logger));
        }

        public RequestHandler4<T, TR, TU, TI> Handle(Handler4 handler)
        {
            _executor = (request, param1, param2, param3, param4, mediaTypeMapper1, errorHandler1, logger1)
                => RequestExecutor.ExecuteRequest(() => handler.Invoke(param1, param2, param3, param4), errorHandler1, logger1);
            return this;
        }

        public RequestHandler4<T, TR, TU, TI> Handle(ObjectHandler4 handler)
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

        public RequestHandler4<T, TR, TU, TI> OnError(IErrorHandler errorHandler)
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

        public RequestHandler5<T, TR, TU, TI, TJ> Param<TJ>()
            => new RequestHandler5<T, TR, TU, TI, TJ>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Path<TJ>(4),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler5<T, TR, TU, TI, TJ> Body<TJ>()
            => new RequestHandler5<T, TR, TU, TI, TJ>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Body<TJ>(MediaTypeMapper),
                ErrorHandler,
                MediaTypeMapper);

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<J>(Type, MediaTypeMapper) or Body<J>(Type).")]
        public RequestHandler5<T, TR, TU, TI, TJ> Body<TJ>(Type mapperClass)
            => Body<TJ>(MapperFrom(mapperClass));

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<J>(Type, MediaTypeMapper) or Body<J>(Type).")]
        public RequestHandler5<T, TR, TU, TI, TJ> Body<TJ>(IMapper mapper)
            => new RequestHandler5<T, TR, TU, TI, TJ>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Body<TJ>(mapper),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler5<T, TR, TU, TI, TJ> Body<TJ>(MediaTypeMapper mediaTypeMapper)
            => new RequestHandler5<T, TR, TU, TI, TJ>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Body<TJ>(mediaTypeMapper),
                ErrorHandler,
                mediaTypeMapper);

        public RequestHandler5<T, TR, TU, TI, string> Query(string name)
            => Query<string>(name, typeof(string));

        public RequestHandler5<T, TR, TU, TI, TJ> Query<TJ>(string name, Type queryClass)
            => new RequestHandler5<T, TR, TU, TI, TJ>(
                Method,
                Path,
                ResolverParam1,
                ResolverParam2,
                ResolverParam3,
                ResolverParam4,
                ParameterResolver.Query<TJ>(name),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler5<T, TR, TU, TI, Header> Header(string name)
            => new RequestHandler5<T, TR, TU, TI, Header>(
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

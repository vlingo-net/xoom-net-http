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
    public class RequestHandler6<T, TR, TU, TI, TJ, TK> : RequestHandler
    {
        internal ParameterResolver<T> ResolverParam1 { get; }
        internal ParameterResolver<TR> ResolverParam2 { get; }
        internal ParameterResolver<TU> ResolverParam3 { get; }
        internal ParameterResolver<TI> ResolverParam4 { get; }
        internal ParameterResolver<TJ> ResolverParam5 { get; }
        internal ParameterResolver<TK> ResolverParam6 { get; }
        
        private ParamExecutor6? _executor;

        public delegate ICompletes<Response> Handler6(T param1, TR param2, TU param3, TI param4, TJ param5, TK param6);
        public delegate ICompletes<IObjectResponse> ObjectHandler6(T param1, TR param2, TU param3, TI param4, TJ param5, TK param6);
        internal delegate ICompletes<Response> ParamExecutor6(
            Request request,
            T param1,
            TR param2,
            TU param3,
            TI param4,
            TJ param5,
            TK param6,
            MediaTypeMapper mediaTypeMapper,
            IErrorHandler errorHandler,
            ILogger logger);

        internal RequestHandler6(
            Method method,
            string path,
            ParameterResolver<T> resolverParam1,
            ParameterResolver<TR> resolverParam2,
            ParameterResolver<TU> resolverParam3,
            ParameterResolver<TI> resolverParam4,
            ParameterResolver<TJ> resolverParam5,
            ParameterResolver<TK> resolverParam6,
            IErrorHandler errorHandler,
            MediaTypeMapper mediaTypeMapper)
            : base(method, path, new List<IParameterResolver> { resolverParam1, resolverParam2, resolverParam3, resolverParam4, resolverParam5, resolverParam6 }, errorHandler, mediaTypeMapper)
        {
            ResolverParam1 = resolverParam1;
            ResolverParam2 = resolverParam2;
            ResolverParam3 = resolverParam3;
            ResolverParam4 = resolverParam4;
            ResolverParam5 = resolverParam5;
            ResolverParam6 = resolverParam6;
        }

        internal ICompletes<Response> Execute(Request request, T param1, TR param2, TU param3, TI param4, TJ param5, TK param6, ILogger logger)
        {
            Func<ICompletes<Response>> exec = ()
                => _executor?.Invoke(request, param1, param2, param3, param4, param5, param6, MediaTypeMapper, ErrorHandler, logger)!;

            return RunParamExecutor(_executor, () => RequestExecutor.ExecuteRequest(exec, ErrorHandler, logger));
        }

        public RequestHandler6<T, TR, TU, TI, TJ, TK> Handle(Handler6 handler)
        {
            _executor = (request, param1, param2, param3, param4, param5, param6, mediaTypeMapper1, errorHandler1, logger1)
                => RequestExecutor.ExecuteRequest(() => handler.Invoke(param1, param2, param3, param4, param5, param6), errorHandler1, logger1);
            return this;
        }

        public RequestHandler6<T, TR, TU, TI, TJ, TK> Handle(ObjectHandler6 handler)
        {
            _executor = (request, param1, param2, param3, param4, param5, param6, mediaTypeMapper1, errorHandler1, logger)
                => RequestObjectExecutor.ExecuteRequest(
                    request,
                    mediaTypeMapper1,
                    () => handler.Invoke(param1, param2, param3, param4, param5, param6),
                    errorHandler1,
                    logger);
            return this;
        }

        public RequestHandler6<T, TR, TU, TI, TJ, TK> OnError(IErrorHandler errorHandler)
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
            var param5 = ResolverParam5.Apply(request, mappedParameters);
            var param6 = ResolverParam6.Apply(request, mappedParameters);
            return Execute(request, param1, param2, param3, param4, param5, param6, logger);
        }
        
        public RequestHandler7<T, TR, TU, TI, TJ, TK, TL> Param<TL>() =>
            new RequestHandler7<T, TR, TU, TI, TJ, TK, TL>(Method, Path, ResolverParam1, ResolverParam2, ResolverParam3, ResolverParam4, ResolverParam5, ResolverParam6,
                ParameterResolver.Path<TL>(6),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler7<T, TR, TU, TI, TJ, TK, TL> Body<TL>() =>
            new RequestHandler7<T, TR, TU, TI, TJ, TK, TL>(Method, Path, ResolverParam1, ResolverParam2, ResolverParam3, ResolverParam4, ResolverParam5, ResolverParam6,
                ParameterResolver.Body<TL>(MediaTypeMapper),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler7<T, TR, TU, TI, TJ, TK, string> Query(string name) => Query<string>(name);

        public RequestHandler7<T, TR, TU, TI, TJ, TK, TL> Query<TL>(string name) =>
            new RequestHandler7<T, TR, TU, TI, TJ, TK, TL>(Method, Path, ResolverParam1, ResolverParam2, ResolverParam3, ResolverParam4, ResolverParam5, ResolverParam6,
                ParameterResolver.Query<TL>(name),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler7<T, TR, TU, TI, TJ, TK, Header> Header(string name) =>
            new RequestHandler7<T, TR, TU, TI, TJ, TK, Header>(Method, Path, ResolverParam1, ResolverParam2, ResolverParam3, ResolverParam4, ResolverParam5, ResolverParam6,
                ParameterResolver.Header(name),
                ErrorHandler,
                MediaTypeMapper);
    }
}

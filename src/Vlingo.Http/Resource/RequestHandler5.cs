// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
    public class RequestHandler5<T, R, U, I, J> : RequestHandler
    {
        internal ParameterResolver<T> ResolverParam1 { get; }
        internal ParameterResolver<R> ResolverParam2 { get; }
        internal ParameterResolver<U> ResolverParam3 { get; }
        internal ParameterResolver<I> ResolverParam4 { get; }
        internal ParameterResolver<J> ResolverParam5 { get; }
        private ParamExecutor5? _executor;

        public delegate ICompletes<Response> Handler5(T param1, R param2, U param3, I param4, J param5);
        public delegate ICompletes<IObjectResponse> ObjectHandler5(T param1, R param2, U param3, I param4, J param5);
        internal delegate ICompletes<Response> ParamExecutor5(
            Request request,
            T param1,
            R param2,
            U param3,
            I param4,
            J param5,
            MediaTypeMapper mediaTypeMapper,
            IErrorHandler errorHandler,
            ILogger logger);

        internal RequestHandler5(
            Method method,
            string path,
            ParameterResolver<T> resolverParam1,
            ParameterResolver<R> resolverParam2,
            ParameterResolver<U> resolverParam3,
            ParameterResolver<I> resolverParam4,
            ParameterResolver<J> resolverParam5,
            IErrorHandler errorHandler,
            MediaTypeMapper mediaTypeMapper)
            : base(method, path, new List<IParameterResolver> { resolverParam1, resolverParam2, resolverParam3, resolverParam4, resolverParam5 }, errorHandler, mediaTypeMapper)
        {
            ResolverParam1 = resolverParam1;
            ResolverParam2 = resolverParam2;
            ResolverParam3 = resolverParam3;
            ResolverParam4 = resolverParam4;
            ResolverParam5 = resolverParam5;
        }

        internal ICompletes<Response> Execute(Request request, T param1, R param2, U param3, I param4, J param5, ILogger logger)
        {
            Func<ICompletes<Response>> exec = ()
                => _executor?.Invoke(request, param1, param2, param3, param4, param5, MediaTypeMapper, ErrorHandler, logger)!;

            return RunParamExecutor(_executor, () => RequestExecutor.ExecuteRequest(exec, ErrorHandler, logger));
        }

        public RequestHandler5<T, R, U, I, J>? Handle(Handler5 handler)
        {
            _executor = (request, param1, param2, param3, param4, param5, mediaTypeMapper1, errorHandler1, logger1)
                => RequestExecutor.ExecuteRequest(() => handler.Invoke(param1, param2, param3, param4, param5), errorHandler1, logger1);
            return this;
        }

        public RequestHandler5<T, R, U, I, J> Handle(ObjectHandler5 handler)
        {
            _executor = (request, param1, param2, param3, param4, param5, mediaTypeMapper1, errorHandler1, logger)
                => RequestObjectExecutor.ExecuteRequest(
                    request,
                    mediaTypeMapper1,
                    () => handler.Invoke(param1, param2, param3, param4, param5),
                    errorHandler1,
                    logger);
            return this;
        }

        public RequestHandler5<T, R, U, I, J> OnError(IErrorHandler errorHandler)
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
            return Execute(request, param1, param2, param3, param4, param5, logger);
        }
    }
}

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
    public class RequestHandler2<T, R> : RequestHandler
    {
        internal ParameterResolver<T> ResolverParam1 { get; }
        internal ParameterResolver<R> ResolverParam2 { get; }
        private ParamExecutor2? _executor;

        public delegate ICompletes<Response> Handler2(T param1, R param2);
        public delegate ICompletes<IObjectResponse> ObjectHandler2(T param1, R param2);
        internal delegate ICompletes<Response> ParamExecutor2(
            Request request,
            T param1,
            R param2,
            MediaTypeMapper mediaTypeMapper,
            IErrorHandler errorHandler,
            ILogger logger);

        internal RequestHandler2(
            Method method,
            string path,
            ParameterResolver<T> resolverParam1,
            ParameterResolver<R> resolverParam2,
            IErrorHandler errorHandler,
            MediaTypeMapper mediaTypeMapper)
            : base(method, path, new List<IParameterResolver> { resolverParam1, resolverParam2 }, errorHandler, mediaTypeMapper)
        {
            ResolverParam1 = resolverParam1;
            ResolverParam2 = resolverParam2;
        }

        internal ICompletes<Response> Execute(Request request, T param1, R param2, ILogger logger)
        {
            Func<ICompletes<Response>> exec = ()
                => _executor?.Invoke(request, param1, param2, MediaTypeMapper, ErrorHandler, logger)!;

            return RunParamExecutor(_executor, () => RequestExecutor.ExecuteRequest(exec, ErrorHandler, logger));
        }

        public RequestHandler2<T, R> Handle(Handler2 handler)
        {
            _executor = (request, param1, param2, mediaTypeMapper1, errorHandler1, logger1)
                => RequestExecutor.ExecuteRequest(() => handler.Invoke(param1, param2), errorHandler1, logger1);
            return this;
        }

        public RequestHandler2<T, R> Handle(ObjectHandler2 handler)
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

        public RequestHandler2<T, R> OnError(IErrorHandler errorHandler)
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

        public RequestHandler3<T, R, U> Param<U>(Type paramClass)
            => new RequestHandler3<T, R, U>(Method, Path, ResolverParam1, ResolverParam2, ParameterResolver.Path<U>(2, paramClass), ErrorHandler, MediaTypeMapper);

        public RequestHandler3<T, R, U> Body<U>(Type bodyClass)
            => new RequestHandler3<T, R, U>(Method, Path, ResolverParam1, ResolverParam2, ParameterResolver.Body<U>(bodyClass, MediaTypeMapper), ErrorHandler, MediaTypeMapper);

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<U>(Type, MediaTypeMapper) or Body<U>(Type).")]
        public RequestHandler3<T, R, U> Body<U>(Type bodyClass, Type mapperClass)
            => Body<U>(bodyClass, MapperFrom(mapperClass));

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<U>(Type, MediaTypeMapper) or Body<U>(Type).")]
        public RequestHandler3<T, R, U> Body<U>(Type bodyClass, IMapper mapper)
            => new RequestHandler3<T, R, U>(
                Method, 
                Path, 
                ResolverParam1, 
                ResolverParam2,
                ParameterResolver.Body<U>(bodyClass, mapper),
                ErrorHandler,
                MediaTypeMapper);

        public RequestHandler3<T, R, U> Body<U>(Type bodyClass, MediaTypeMapper mediaTypeMapper)
        {
            MediaTypeMapper = mediaTypeMapper;
            return new RequestHandler3<T,R,U>(
                Method, 
                Path, 
                ResolverParam1, 
                ResolverParam2,
                ParameterResolver.Body<U>(bodyClass, mediaTypeMapper), 
                ErrorHandler, 
                MediaTypeMapper);
        }

        public RequestHandler3<T, R, string> Query(string name)
            => Query<string>(name, typeof(string));

        public RequestHandler3<T, R, U> Query<U>(string name, Type queryClass)
            => new RequestHandler3<T,R,U>(Method, Path, ResolverParam1, ResolverParam2, ParameterResolver.Query<U>(name, queryClass), ErrorHandler, MediaTypeMapper);

        public RequestHandler3<T, R, Header> Header(string name)
            => new RequestHandler3<T, R, Header>(Method, Path, ResolverParam1, ResolverParam2, ParameterResolver.Header(name), ErrorHandler, MediaTypeMapper);


        /**
         * ===========================
         * Not necessary yet
         * ====================
         * 
         * 
         static class RequestExecutor2<T, R> extends RequestExecutor implements ParamExecutor2<T,R> {
    private final Handler2<T,R> handler;

    private RequestExecutor2(Handler2<T,R> handler) { this.handler = handler; }

    @Override
    public Completes<Response> execute(final Request request,
                                       final T param1,
                                       final R param2,
                                       final MediaTypeMapper mediaTypeMapper,
                                       final ErrorHandler errorHandler,
                                       final Logger logger) {
      return executeRequest(() -> handler.execute(param1, param2), errorHandler, logger);
    }

    static <T,R> RequestExecutor2<T,R> from(final Handler2<T,R> handler) {
      return new RequestExecutor2<>(handler);}
  }

  static class RequestObjectExecutor2<T,R> extends RequestObjectExecutor implements ParamExecutor2<T,R> {
    private final ObjectHandler2<T,R> handler;
    private RequestObjectExecutor2(ObjectHandler2<T,R> handler) { this.handler = handler;}

    @Override
    public Completes<Response> execute(final Request request,
                                       final T param1,
                                       final R param2,
                                       final MediaTypeMapper mediaTypeMapper,
                                       final ErrorHandler errorHandler,
                                       final Logger logger) {
      return executeRequest(request,
                            mediaTypeMapper,
                            () -> handler.execute(param1, param2),
                            errorHandler,
                            logger);
    }

    static <T,R> RequestObjectExecutor2<T,R> from(final ObjectHandler2<T,R> handler) {
      return new RequestObjectExecutor2<>(handler);}
  }
         **/
    }
}

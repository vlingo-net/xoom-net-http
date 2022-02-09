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
    public class RequestHandler0 : RequestHandler
    {
        private ParamExecutor0? _executor;

        public delegate ICompletes<Response> Handler0();
        
        public delegate ICompletes<IObjectResponse> ObjectHandler0();
        
        internal delegate ICompletes<Response> ParamExecutor0(
            Request request,
            MediaTypeMapper mediaTypeMapper,
            IErrorHandler errorHandler,
            ILogger logger);

        internal RequestHandler0(Method method, string path)
            : base(method, path, new List<IParameterResolver>(0))
        {
        }

        public RequestHandler0 Handle(Handler0 handler)
        {
            _executor = (request, mediaTypeMapper1, errorHandler1, logger)
                => RequestExecutor.ExecuteRequest(() => handler.Invoke(), errorHandler1, logger);
            return this;
        }

        public RequestHandler0 Handle(ObjectHandler0 handler)
        {
            _executor = (request, mediaTypeMapper1, errorHandler1, logger)
                => RequestObjectExecutor.ExecuteRequest(request, mediaTypeMapper1, () => handler.Invoke(), errorHandler1, logger);
            return this;
        }

        public RequestHandler0 OnError(IErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler;
            return this;
        }

        public RequestHandler0 Mapper(MediaTypeMapper mediaTypeMapper)
        {
            MediaTypeMapper = mediaTypeMapper;
            return this;
        }

        internal ICompletes<Response> Execute(Request request, ILogger logger)
        {
            Func<ICompletes<Response>> exec = ()
                => _executor?.Invoke(request, MediaTypeMapper, ErrorHandler, logger)!;

            return RunParamExecutor(_executor, () => RequestExecutor.ExecuteRequest(exec, ErrorHandler, logger));
        }

        internal override ICompletes<Response> Execute(Request request, Action.MappedParameters mappedParameters, ILogger logger)
            => Execute(request, logger);


        public RequestHandler1<T> Param<T>()
            => new RequestHandler1<T>(Method, Path, ParameterResolver.Path<T>(0), ErrorHandler, MediaTypeMapper);

        public RequestHandler1<T> Body<T>()
            => new RequestHandler1<T>(Method, Path, ParameterResolver.Body<T>(MediaTypeMapper), ErrorHandler, MediaTypeMapper);

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<T>(Type, MediaTypeMapper) or Body<T>(Type).")]
        public RequestHandler1<T> Body<T>(Type mapperClass)
            => Body<T>(MapperFrom(mapperClass));

        [Obsolete("Deprecated in favor of using the ContentMediaType method, which handles media types appropriately. Use Body<T>(Type, MediaTypeMapper) or Body<T>(Type).")]
        public RequestHandler1<T> Body<T>(IMapper mapper)
            => new RequestHandler1<T>(Method, Path, ParameterResolver.Body<T>(mapper), ErrorHandler, MediaTypeMapper);

        public RequestHandler1<T> Body<T>(MediaTypeMapper mediaTypeMapper)
        {
            MediaTypeMapper = mediaTypeMapper;
            return new RequestHandler1<T>(Method, Path, ParameterResolver.Body<T>(mediaTypeMapper), ErrorHandler, mediaTypeMapper);
        }

        public RequestHandler1<string> Query(string name)
            => Query<string>(name);

        public RequestHandler1<T> Query<T>(string name)
            => Query(name, default(T)!);

        public RequestHandler1<T> Query<T>(string name, T defaultValue)
            => new RequestHandler1<T>(Method, Path, ParameterResolver.Query(name, defaultValue), ErrorHandler, MediaTypeMapper);

        public RequestHandler1<Header> Header(string name)
            => new RequestHandler1<Header>(Method, Path, ParameterResolver.Header(name), ErrorHandler, MediaTypeMapper);
    }
}

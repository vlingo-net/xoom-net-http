// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Http.Media;
using Vlingo.Http.Resource;
using Vlingo.Http.Resource.Serialization;
using Vlingo.Http.Tests.Sample.User;
using Vlingo.Http.Tests.Sample.User.Model;
using Xunit;
using Xunit.Abstractions;
using Action = Vlingo.Http.Resource.Action;

namespace Vlingo.Http.Tests.Resource
{
    public class RequestHandlerTest : RequestHandlerTestBase
    {
        [Fact]
        public void ExecutionErrorUsesErrorHandlerWhenExceptionThrown()
        {
            var testStatus = Response.ResponseStatus.BadRequest;

            var handler = new RequestHandlerFake(
                Method.Get, 
                "/hello",
                new List<IParameterResolver>(),
                () => throw new Exception("Handler failed"));

            var customHandler = new ErrorHandlerImpl(exception => {
                Assert.True(exception != null);
                Assert.IsAssignableFrom<Exception>(exception);
                return Response.Of(testStatus);
            });
            

            var response = handler.Execute(Request.WithMethod(Method.Get), customHandler, Logger).Await();
            AssertResponsesAreEquals(Response.Of(testStatus), response);
        }
        
        [Fact]
        public void ExecutionErrorObjectUsesErrorHandlerWhenExceptionThrown()
        {
            var testStatus = Response.ResponseStatus.BadRequest;

            var validHandler = new ErrorHandlerImpl(exception => {
                Assert.True(exception != null);
                Assert.IsAssignableFrom<Exception>(exception);
                return Response.Of(testStatus);
            });
            
            var handler = new RequestObjectHandlerFake(
                Method.Get, 
                "/hello",
                validHandler,
                () => throw new Exception("Handler failed"));


            var response = handler.Execute(Request.WithMethod(Method.Get), null, Logger).Await();
            AssertResponsesAreEquals(Response.Of(testStatus), response);
        }
        
        [Fact]
        public void InternalErrorReturnedWhenErrorHandlerThrowsException()
        {
            var testStatus = Response.ResponseStatus.InternalServerError;

            var handler = new RequestHandlerFake(
                Method.Get, 
                "/hello",
                new List<IParameterResolver>(),
                () => throw new Exception("Handler failed"));

            var badHandler = new ErrorHandlerImpl(exception => throw new InvalidOperationException());

            var response = handler.Execute(Request.WithMethod(Method.Get), badHandler, Logger).Await();
            AssertResponsesAreEquals(Response.Of(testStatus), response);
        }
        
        [Fact]
        public void InternalErrorReturnedWhenNoErrorHandlerDefined()
        {
            var testStatus = Response.ResponseStatus.InternalServerError;

            var handler = new RequestHandlerFake(
                Method.Get, 
                "/hello",
                new List<IParameterResolver>(),
                () => throw new Exception("Handler failed"));

            var response = handler.Execute(Request.WithMethod(Method.Get), null, Logger).Await();
            AssertResponsesAreEquals(Response.Of(testStatus), response);
        }
        
        [Fact]
        public void MappingNotAvailableReturnsMediaTypeNotFoundResponse()
        {
            var testStatus = Response.ResponseStatus.UnsupportedMediaType;

            var handler = new RequestHandlerFake(
                Method.Get, 
                "/hello",
                new List<IParameterResolver>(),
                () => throw new MediaTypeNotSupportedException("foo/bar"));

            var response = handler.Execute(Request.WithMethod(Method.Get), null, Logger).Await();
            AssertResponsesAreEquals(Response.Of(testStatus), response);
        }
        
        [Fact]
        public void ObjectResponseMappedToContentType()
        {
            var name = new Name("first", "last");
            
            var handler = new RequestObjectHandlerFake(
                Method.Get, 
                "/hello",
                () => Completes.WithSuccess(ObjectResponse<Name>.Of(Response.ResponseStatus.Ok, name)));


            var response = handler.Execute(Request.WithMethod(Method.Get), null, Logger).Await();
            var nameAsJson = JsonSerialization.Serialized(name);
            AssertResponsesAreEquals(
                Response.Of(Response.ResponseStatus.Ok,
                    ResponseHeader.WithHeaders(ResponseHeader.ContentType, ContentMediaType.Json.ToString()),
                    nameAsJson),
                response);
        }
        
        [Fact]
        public void GenerateActionSignatureWhenNoPathIsSpecifiedIsEmptyString()
        {
            var handler = new RequestHandlerFake(
                Method.Get,
                "/hello",
                new List<IParameterResolver> { ParameterResolver.Body<NameData>()});

            Assert.Equal("", handler.ActionSignature);
        }

        [Fact]
        public void GenerateActionSignatureWithOnePathParameterReturnsSignatureWithOneParam()
        {
            var handler = new RequestHandlerFake(
                Method.Get,
                "/user/{userId}",
                new List<IParameterResolver> { ParameterResolver.Path<string>(0)} );

            Assert.Equal("String userId", handler.ActionSignature);
        }

        [Fact]
        public void GenerateActionWithTwoPathParameters()
        {
            var handler = new RequestHandlerFake(
                Method.Get,
                "/user/{userId}/comment/{commentId}",
                new List<IParameterResolver> { ParameterResolver.Path<string>(0), ParameterResolver.Path<int>(0)} );

            Assert.Equal("String userId, Int32 commentId", handler.ActionSignature);
        }

        [Fact]
        public void GenerateActionWithOnePathParameterAndBodyJustReturnPathParameterSignature()
        {
            var handler = new RequestHandlerFake(
                Method.Get,
                "/user/{userId}",
                new List<IParameterResolver> { ParameterResolver.Path<string>(0), ParameterResolver.Body<NameData>()} );

            Assert.Equal("String userId", handler.ActionSignature);
        }

        [Fact]
        public void UnsortedPathParametersThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                return new RequestHandlerFake(
                    Method.Get,
                    "/user/{userId}",
                    new List<IParameterResolver>
                        {ParameterResolver.Body<NameData>(), ParameterResolver.Path<string>(0)});
            });
        }
        
        public RequestHandlerTest(ITestOutputHelper output) : base(output)
        {
        }
    }

    internal class RequestObjectHandlerFake : RequestHandler
    {
        private readonly RequestHandler0.ParamExecutor0 _executor;
        private readonly IErrorHandler _errorHandler;

        internal RequestObjectHandlerFake(Method method, string path, RequestHandler0.ObjectHandler0 handler) : base (method, path, new List<IParameterResolver>())
        {
            _executor = (request, mediaTypeMapper1, errorHandler1, logger1) =>
                RequestObjectExecutor.ExecuteRequest(request, mediaTypeMapper1, handler.Invoke, _errorHandler, logger1);
        }
        
        internal RequestObjectHandlerFake(Method method, string path, IErrorHandler errorHandler, RequestHandler0.ObjectHandler0 handler) : base(method, path, new List<IParameterResolver>())
        {
            _executor = (request, mediaTypeMapper1, errorHandler1, logger1) =>
                RequestObjectExecutor.ExecuteRequest(request, mediaTypeMapper1, handler.Invoke, errorHandler1, logger1);
            _errorHandler = errorHandler;
        }

        internal override ICompletes<Response> Execute(Request request, Action.MappedParameters mappedParameters, ILogger logger)
            => _executor.Invoke(request, DefaultMediaTypeMapper.Instance, _errorHandler, logger);
    }
    
    internal class RequestHandlerFake : RequestHandler
    {
        private readonly RequestHandler0.ParamExecutor0 _executor;
        
        internal RequestHandlerFake(Method method, string path, List<IParameterResolver> parameterResolvers) : base(method, path, parameterResolvers)
        {
            _executor = (request, mediaTypeMapper1, errorHandler1, logger1) =>
                RequestExecutor.ExecuteRequest(() => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok)), errorHandler1, logger1);
        }

        internal RequestHandlerFake(Method method, string path, List<IParameterResolver> parameterResolvers, RequestHandler0.Handler0 handler) : base (method, path, parameterResolvers)
        {
            _executor = (request, mediaTypeMapper1, errorHandler1, logger1) =>
                RequestExecutor.ExecuteRequest(handler.Invoke, errorHandler1, logger1);
        }

        internal override ICompletes<Response> Execute(Request request, Action.MappedParameters mappedParameters, ILogger logger)
        {
            throw new System.NotImplementedException();
        }
        
        internal ICompletes<Response> Execute(Request request, IErrorHandler errorHandler, ILogger logger) => 
            _executor.Invoke(request, null, errorHandler, logger);
    }
}
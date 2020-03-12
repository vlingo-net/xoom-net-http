// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Common;
using Vlingo.Common.Serialization;
using Vlingo.Http.Resource;
using Xunit;
using Xunit.Abstractions;
using Action = Vlingo.Http.Resource.Action;

namespace Vlingo.Http.Tests.Resource
{
    public class RequestHandler5Test : RequestHandlerTestBase
    {
        [Fact]
        public void HandlerWithOneParam()
        {
            var handler = CreateRequestHandler(
                Method.Get,
                "/posts/{postId}/comment/{commentId}/user/{userId}",
                ParameterResolver.Path<string>(0),
                ParameterResolver.Path<string>(1),
                ParameterResolver.Path<string>(2),
            ParameterResolver.Query("page", 10),
                ParameterResolver.Query("pageSize", 10))
                .Handle((postId, commentId, userId, page, pageSize) =>
                    Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(
                        $"{postId} {commentId}"))));

            var response = handler
                .Execute(Request.WithMethod(Method.Get), "my-post", "my-comment", "admin", 10, 10, Logger).Outcome;

            Assert.NotNull(handler);
            Assert.Equal(Method.Get, handler.Method);
            Assert.Equal("/posts/{postId}/comment/{commentId}/user/{userId}", handler.Path);
            Assert.Equal(typeof(string), handler.ResolverParam1.ParamClass);
            Assert.Equal(typeof(string), handler.ResolverParam2.ParamClass);
            Assert.Equal(typeof(string), handler.ResolverParam3.ParamClass);
            AssertResponsesAreEquals(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized("my-post my-comment")), response);
        }

        [Fact]
        public void ThrowExceptionWhenNoHandlerIsDefined()
        {
            var handler = CreateRequestHandler(
                Method.Get,
                "/posts/{postId}/comment/{commentId}/user/{userId}",
                ParameterResolver.Path<string>(0),
                ParameterResolver.Path<string>(1),
                ParameterResolver.Path<string>(2),
                ParameterResolver.Query("page", 10),
                ParameterResolver.Query("pageSize", 10));

            var exception = Assert.Throws<HandlerMissingException>(() => handler.Execute(Request.WithMethod(Method.Get), "my-post",
                "my-comment", "admin", 10, 10, Logger));
            Assert.Equal("No handler defined for GET /posts/{postId}/comment/{commentId}/user/{userId}", exception.Message);
        }

        [Fact]
        public void ActionSignature()
        {
            var handler = CreateRequestHandler(
                Method.Get,
                "/posts/{postId}/comment/{commentId}/user/{userId}",
                ParameterResolver.Path<string>(0),
                ParameterResolver.Path<string>(1),
                ParameterResolver.Path<string>(2),
                ParameterResolver.Query("page", 10),
                ParameterResolver.Query("pageSize", 10));

            Assert.Equal("String postId, String commentId, String userId", handler.ActionSignature);
        }

        [Fact]
        public void ExecuteWithRequestAndMappedParameters()
        {
            var request = Request.Has(Method.Get)
                .And("/posts/my-post/comments/my-comment/users/my-user".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter> {
                    new Action.MappedParameter("string", "my-post"),
                    new Action.MappedParameter("string", "my-comment"),
                    new Action.MappedParameter("string", "my-user"),
                });
            var handler = CreateRequestHandler(
                    Method.Get,
                    "/posts/{postId}/comment/{commentId}/user/{userId}",
                    ParameterResolver.Path<string>(0),
                    ParameterResolver.Path<string>(1),
                    ParameterResolver.Path<string>(2),
                    ParameterResolver.Query("page", 10),
                    ParameterResolver.Query("pageSize", 10))
                .Handle((postId, commentId, userId, page, pageSize)
                    => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(
                        $"{postId} {commentId} {userId}"))));

            var response = handler.Execute(request, mappedParameters, Logger).Outcome;

            AssertResponsesAreEquals(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized("my-post my-comment my-user")), response);
        }

        public RequestHandler5Test(ITestOutputHelper output) : base(output)
        {
        }

        private RequestHandler5<T, R, U, I, J> CreateRequestHandler<T, R, U, I, J>(Method method,
            string path,
            ParameterResolver<T> parameterResolver1,
            ParameterResolver<R> parameterResolver2,
            ParameterResolver<U> parameterResolver3,
            ParameterResolver<I> parameterResolver4,
            ParameterResolver<J> parameterResolver5)
        {
            return new RequestHandler5<T, R, U, I, J>(
                method,
                path,
                parameterResolver1,
                parameterResolver2,
                parameterResolver3,
                parameterResolver4,
                parameterResolver5,
                ErrorHandler.HandleAllWith(Response.ResponseStatus.InternalServerError),
                DefaultMediaTypeMapper.Instance);
        }
    }
}
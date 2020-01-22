// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using Vlingo.Common;
using Vlingo.Http.Resource;
using Vlingo.Http.Resource.Serialization;
using Vlingo.Http.Tests.Sample.User;
using Xunit;
using Xunit.Abstractions;
using Action = Vlingo.Http.Resource.Action;

namespace Vlingo.Http.Tests.Resource
{
    public class RequestHandler4Test : RequestHandlerTestBase
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
                    ParameterResolver.Query("page", 10))
                .Handle((postId, commentId, userId, page) =>
                    Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(
                        $"{postId} {commentId}"))));

            var response = handler
                .Execute(Request.WithMethod(Method.Get), "my-post", "my-comment", "admin", 10, Logger).Outcome;

            Assert.NotNull(handler);
            Assert.Equal(Method.Get, handler.Method);
            Assert.Equal("/posts/{postId}/comment/{commentId}/user/{userId}", handler.Path);
            Assert.Equal(typeof(string), handler.ResolverParam1.ParamClass);
            Assert.Equal(typeof(string), handler.ResolverParam2.ParamClass);
            AssertResponsesAreEquals(
                Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized("my-post my-comment")), response);
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
                ParameterResolver.Query("page", 10));

            var exception = Assert.Throws<HandlerMissingException>(() => handler.Execute(Request.WithMethod(Method.Get),
                "my-post",
                "my-comment", "admin", 10, Logger));
            Assert.Equal("No handler defined for GET /posts/{postId}/comment/{commentId}/user/{userId}",
                exception.Message);
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
                ParameterResolver.Query("page", 10));

            Assert.Equal("String postId, String commentId, String userId", handler.ActionSignature);
        }

        [Fact]
        public void ExecuteWithRequestAndMappedParameters()
        {
            var request = Request.Has(Method.Get)
                .And("/posts/my-post/comments/my-comment/users/my-user".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>
                {
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
                    ParameterResolver.Query("page", 10))
                .Handle((postId, commentId, userId, page)
                    => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(
                        $"{postId} {commentId} {userId}"))));

            var response = handler.Execute(request, mappedParameters, Logger).Outcome;

            AssertResponsesAreEquals(
                Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized("my-post my-comment my-user")),
                response);
        }

        [Fact]
        public void AddingHandlerParam()
        {
            var request = Request.Has(Method.Get)
                .And("/posts/my-post/comment/my-comment/votes/10/user/admin".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>
                {
                    new Action.MappedParameter("String", "my-post"),
                    new Action.MappedParameter("String", "my-comment"),
                    new Action.MappedParameter("String", 10),
                    new Action.MappedParameter("String", "admin"),
                    new Action.MappedParameter("String", "justanother")   
                });

            var
            handler = CreateRequestHandler(
                    Method.Get,
                    "/posts/{postId}/comment/{commentId}/votes/{votesNumber}/user/{userId}/{another}",
                    ParameterResolver.Path<string>(0),
                    ParameterResolver.Path<string>(1),
                    ParameterResolver.Path<int>(2),
                    ParameterResolver.Path<string>(3)
                )
                .Param<string>();

            AssertResolvesAreEquals(ParameterResolver.Path<string>(4), handler.ResolverParam5);
            Assert.Equal("justanother", handler.ResolverParam5.Apply(request, mappedParameters));
        }

        [Fact]
        public void AddingHandlerBody()
        {
            var request = Request.Has(Method.Post)
                .And("/posts/my-post/comment/my-comment".ToMatchableUri())
                .And(Body.From("{\"given\":\"John\",\"family\":\"Doe\"}"))
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Post, "ignored", new List<Action.MappedParameter>
                {
                    new Action.MappedParameter("String", "my-post"),
                    new Action.MappedParameter("String", "my-comment")   
                });

            var handler = CreateRequestHandler(
                    Method.Post,
                    "/posts/{postId}/comment/{commentId}/votes/{votesNumber}",
                    ParameterResolver.Path<string>(0),
                    ParameterResolver.Path<string>(1),
                    ParameterResolver.Path<int>(2),
            ParameterResolver.Query("page", 10)
                )
                .Body<NameData>();

            AssertResolvesAreEquals(ParameterResolver.Body<NameData>(), handler.ResolverParam5);
            Assert.Equal(new NameData("John", "Doe"), handler.ResolverParam5.Apply(request, mappedParameters));
        }

        [Fact]
        public void AddingHandlerBodyWithMapper()
        {
            var request = Request.Has(Method.Post)
                .And("/posts/my-post/comment/my-comment".ToMatchableUri())
                .And(Body.From("{\"given\":\"John\",\"family\":\"Doe\"}"))
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Post, "ignored", new List<Action.MappedParameter>
                {
                    new Action.MappedParameter("String", "my-post"),
                    new Action.MappedParameter("String", "my-comment")   
                });

            var handler = CreateRequestHandler(
                    Method.Post,
                    "/posts/{postId}/comment/{commentId}/votes/{votesNumber}",
                    ParameterResolver.Path<string>(0),
                    ParameterResolver.Path<string>(1),
                    ParameterResolver.Path<int>(2),
                    ParameterResolver.Query("page", 10)
                )
                .Body<NameData>(typeof(TestMapper));

            AssertResolvesAreEquals(ParameterResolver.Body<NameData>(), handler.ResolverParam5);
            Assert.Equal(new NameData("John", "Doe"), handler.ResolverParam5.Apply(request, mappedParameters));
        }

        [Fact]
        public void AddingHandlerQuery()
        {
            var request = Request.Has(Method.Post)
                .And("/posts/my-post/comment/my-comment?filter=abc".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", Enumerable.Empty<Action.MappedParameter>().ToList());

            var handler = CreateRequestHandler(
                    Method.Get,
                    "/posts/{postId}/comment/{commentId}/votes/{votesId}",
                    ParameterResolver.Path<string>(0),
                    ParameterResolver.Path<string>(1),
                    ParameterResolver.Path<string>(2),
                    ParameterResolver.Query("page", 10)
                )
                .Query("filter");

            AssertResolvesAreEquals(ParameterResolver.Query<string>("filter"), handler.ResolverParam5);
            Assert.Equal("abc", handler.ResolverParam5.Apply(request, mappedParameters));
        }


        [Fact]
        public void AddingHandlerHeader()
        {
            var hostHeader = RequestHeader.Of("Host", "www.vlingo.io");
            var request = Request.Has(Method.Get)
                .And("/posts/my-post/comment/my-comment".ToMatchableUri())
                .And(Headers.Of(hostHeader))
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", Enumerable.Empty<Action.MappedParameter>().ToList());

            var handler = CreateRequestHandler(
                    Method.Get,
                    "/posts/{postId}/comment/{commentId}/votes/{votesNumber}",
                    ParameterResolver.Path<string>(0),
                    ParameterResolver.Path<string>(1),
                    ParameterResolver.Path<string>(2),
                    ParameterResolver.Query("page", 10)
                )
                .Header("Host");

            AssertResolvesAreEquals(ParameterResolver.Header("Host"), handler.ResolverParam5);
            Assert.Equal(hostHeader, handler.ResolverParam5.Apply(request, mappedParameters));
        }

        public RequestHandler4Test(ITestOutputHelper output) : base(output)
        {
        }

        private RequestHandler4<T, R, U, I> CreateRequestHandler<T, R, U, I>(Method method,
            string path,
            ParameterResolver<T> parameterResolver1,
            ParameterResolver<R> parameterResolver2,
            ParameterResolver<U> parameterResolver3,
            ParameterResolver<I> parameterResolver4)
        {
            return new RequestHandler4<T, R, U, I>(
                method,
                path,
                parameterResolver1,
                parameterResolver2,
                parameterResolver3,
                parameterResolver4,
                ErrorHandler.HandleAllWith(Response.ResponseStatus.InternalServerError),
                DefaultMediaTypeMapper.Instance);
        }
    }
}
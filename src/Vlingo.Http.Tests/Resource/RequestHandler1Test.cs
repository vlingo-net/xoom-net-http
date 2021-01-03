// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Common;
using Vlingo.Common.Serialization;
using Vlingo.Http.Resource;
using Vlingo.Http.Tests.Sample.User;
using Xunit;
using Xunit.Abstractions;
using Action = Vlingo.Http.Resource.Action;

namespace Vlingo.Http.Tests.Resource
{
    public class RequestHandler1Test : RequestHandlerTestBase
    {
        [Fact]
        public void HandlerWithOneParam()
        {
            var handler = CreateRequestHandler(
                    Method.Get,
                    "/posts/{postId}",
                    ParameterResolver.Path<string>(0))
                .Handle(postId =>
                    Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(
                        $"{postId}"))));

            var response = handler
                .Execute(Request.WithMethod(Method.Get), "my-post", Logger).Outcome;

            Assert.NotNull(handler);
            Assert.Equal(Method.Get, handler.Method);
            Assert.Equal("/posts/{postId}", handler.Path);
            Assert.Equal(typeof(string), handler.Resolver.ParamClass);
            AssertResponsesAreEquals(
                Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized("my-post")), response);
        }

        [Fact]
        public void ThrowExceptionWhenNoHandlerIsDefined()
        {
            var handler = CreateRequestHandler(
                Method.Get,
                "/posts/{postId}",
                ParameterResolver.Path<string>(0));

            var exception = Assert.Throws<HandlerMissingException>(() => handler.Execute(Request.WithMethod(Method.Get),
                "my-post", Logger));
            Assert.Equal("No handler defined for GET /posts/{postId}", exception.Message);
        }

        [Fact]
        public void ActionSignature()
        {
            var handler = CreateRequestHandler(
                Method.Get,
                "/posts/{postId}",
                ParameterResolver.Path<string>(0));

            Assert.Equal("String postId", handler.ActionSignature);
        }

        [Fact]
        public void ActionSignatureWithEmptyParamNameThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>(() => CreateRequestHandler(Method.Get, "/posts/{}", ParameterResolver.Path<string>(0))
                .Handle(postId => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(postId)))));
            Assert.Equal("Empty path parameter name for GET /posts/{}", exception.Message);
        }

        [Fact]
        public void ActionSignatureWithBlankParamNameThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>(() => CreateRequestHandler(Method.Get, "/posts/{ }", ParameterResolver.Path<string>(0))
                .Handle(postId => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(postId)))));
            Assert.Equal("Empty path parameter name for GET /posts/{ }", exception.Message);
        }

        [Fact]
        public void ActionWithoutParamNameShouldNotThrowException()
        {
            var handler = CreateRequestHandler(Method.Post, "/posts", ParameterResolver.Body<string>())
                .Handle(postId => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(postId))));

            Assert.Equal("", handler.ActionSignature);
        }

        [Fact]
        public void ExecuteWithRequestAndMappedParameters()
        {
            var request = Request.Has(Method.Get)
                .And("/posts/my-post/comments/my-comment".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>
                {
                    new Action.MappedParameter("string", "my-post"),
                    new Action.MappedParameter("string", "my-comment"),
                });
            var handler = CreateRequestHandler(
                    Method.Get,
                    "/posts/{postId}",
                    ParameterResolver.Path<string>(0))
                .Handle(postId
                    => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(
                        $"{postId}"))));

            var response = handler.Execute(request, mappedParameters, Logger).Outcome;

            AssertResponsesAreEquals(
                Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized("my-post")),
                response);
        }
        
        [Fact]
        public void ExecuteWithRequestAndMappedParametersWithWrongSignatureType()
        {
            var request = Request.Has(Method.Get)
                .And("/posts/my-post".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>
                {
                    new Action.MappedParameter("String", "my-post")
                });
            var handler = CreateRequestHandler(Method.Get, "/posts/{postId}", ParameterResolver.Path<int>(0))
                .Handle(postId => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized("it is my-post"))));

            var exception = Assert.Throws<ArgumentException>(() => handler.Execute(request, mappedParameters, Logger));
            Assert.Equal("Value my-post is of mimeType String instead of Int32", exception.Message);
        }

        [Fact]
        public void AddingHandlerParam()
        {
            var request = Request.Has(Method.Get)
                .And("/user/admin/picture/2".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>
                {
                    new Action.MappedParameter("String", "admin"),
                    new Action.MappedParameter("int", 1)
                });

            var handler = CreateRequestHandler(
                        Method.Get,
                        "/user/{userId}/picture/{pictureId}",
                        ParameterResolver.Path<string>(0))
                    .Param<int>();

            AssertResolvesAreEquals(ParameterResolver.Path<int>(1), handler.ResolverParam2);
            Assert.Equal(1, handler.ResolverParam2.Apply(request, mappedParameters));
        }

        [Fact]
        public void AddingHandlerBody()
        {
            var request = Request.Has(Method.Post)
                .And("/user/admin/name".ToMatchableUri())
                .And(Body.From("{\"given\":\"John\",\"family\":\"Doe\"}"))
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Post, "ignored", new List<Action.MappedParameter>
                {
                    new Action.MappedParameter("String", "admin")
                });

            var handler = CreateRequestHandler(
                    Method.Post,
                    "/user/{userId}/picture/{pictureId}",
                    ParameterResolver.Path<string>(0))
                .Body<NameData>();

            AssertResolvesAreEquals(ParameterResolver.Body<NameData>(), handler.ResolverParam2);
            Assert.Equal(new NameData("John", "Doe"), handler.ResolverParam2.Apply(request, mappedParameters));
        }

        [Fact]
        public void AddingHandlerBodyWithMapper()
        {
            var request = Request.Has(Method.Post)
                .And("/user/admin/name".ToMatchableUri())
                .And(Body.From("{\"given\":\"John\",\"family\":\"Doe\"}"))
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Post, "ignored", new List<Action.MappedParameter>
                {
                    new Action.MappedParameter("String", "admin")
                });
            
#pragma warning disable 618
            var handler = CreateRequestHandler(
                    Method.Post,
                    "/user/{userId}/picture/{pictureId}",
                    ParameterResolver.Path<string>(0))
                .Body<NameData>(typeof(TestMapper));
#pragma warning restore 618

            AssertResolvesAreEquals(ParameterResolver.Body<NameData>(), handler.ResolverParam2);
            Assert.Equal(new NameData("John", "Doe"), handler.ResolverParam2.Apply(request, mappedParameters));
        }
        
        [Fact]
        public void AddingHandlerBodyWithMediaTypeMapper()
        {
            var request = Request.Has(Method.Post)
                .And("/user/admin/name".ToMatchableUri())
                .And(Body.From("{\"given\":\"John\",\"family\":\"Doe\"}"))
                .And(RequestHeader.Of(RequestHeader.ContentType, "application/json"))
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Post, "ignored", new List<Action.MappedParameter>
                {
                    new Action.MappedParameter("String", "admin")
                });

            var handler1 = new RequestHandler0(Method.Get, "/user/admin/name").Body<NameData>();

            AssertResolvesAreEquals(ParameterResolver.Body<NameData>(DefaultMediaTypeMapper.Instance), handler1.Resolver);
            Assert.Equal(new NameData("John", "Doe"), handler1.Resolver.Apply(request, mappedParameters));
        }

        [Fact]
        public void AddingHandlerQuery()
        {
            var request = Request.Has(Method.Post)
                .And("/user/admin?filter=abc".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored",
                    Enumerable.Empty<Action.MappedParameter>().ToList());

            var handler = CreateRequestHandler(
                    Method.Get,
                    "/user/{userId}",
                    ParameterResolver.Path<string>(0))
                .Query("filter");

            AssertResolvesAreEquals(ParameterResolver.Query<string>("filter"), handler.ResolverParam2);
            Assert.Equal("abc", handler.ResolverParam2.Apply(request, mappedParameters));
        }


        [Fact]
        public void AddingHandlerHeader()
        {
            var hostHeader = RequestHeader.Of("Host", "www.vlingo.io");
            var request = Request.Has(Method.Get)
                .And("/user/admin".ToMatchableUri())
                .And(Headers.Of(hostHeader))
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored",
                    Enumerable.Empty<Action.MappedParameter>().ToList());

            var handler = CreateRequestHandler(
                    Method.Get,
                    "/user/{userId}",
                    ParameterResolver.Path<string>(0))
                .Header("Host");

            AssertResolvesAreEquals(ParameterResolver.Header("Host"), handler.ResolverParam2);
            Assert.Equal(hostHeader, handler.ResolverParam2.Apply(request, mappedParameters));
        }

        public RequestHandler1Test(ITestOutputHelper output) : base(output)
        {
        }

        private RequestHandler1<T> CreateRequestHandler<T>(Method method,
            string path,
            ParameterResolver<T> parameterResolver1)
        {
            return new RequestHandler1<T>(
                method,
                path,
                parameterResolver1,
                ErrorHandler.HandleAllWith(Response.ResponseStatus.InternalServerError),
                DefaultMediaTypeMapper.Instance);
        }
    }
}
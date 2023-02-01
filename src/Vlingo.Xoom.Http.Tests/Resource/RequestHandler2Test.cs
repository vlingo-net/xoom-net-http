// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Http.Tests.Sample.User;
using Xunit;
using Xunit.Abstractions;
using Action = Vlingo.Xoom.Http.Resource.Action;

namespace Vlingo.Xoom.Http.Tests.Resource;

public class RequestHandler2Test : RequestHandlerTestBase
{
    [Fact]
    public void HandlerWithOneParam()
    {
        var handler = CreateRequestHandler(
                Method.Get,
                "/posts/{postId}/comment/{commentId}",
                ParameterResolver.Path<string>(0),
                ParameterResolver.Path<string>(1))
            .Handle((postId, commentId) =>
                Completes.WithSuccess(Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized(
                    $"{postId} {commentId}"))));

        var response = handler
            .Execute(Request.WithMethod(Method.Get), "my-post", "my-comment", Logger).Outcome;

        Assert.NotNull(handler);
        Assert.Equal(Method.Get, handler.Method);
        Assert.Equal("/posts/{postId}/comment/{commentId}", handler.Path);
        Assert.Equal(typeof(string), handler.ResolverParam1.ParamClass);
        Assert.Equal(typeof(string), handler.ResolverParam2.ParamClass);
        AssertResponsesAreEquals(
            Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized("my-post my-comment")), response);
    }

    [Fact]
    public void ThrowExceptionWhenNoHandlerIsDefined()
    {
        var handler = CreateRequestHandler(
            Method.Get,
            "/posts/{postId}/comment/{commentId}",
            ParameterResolver.Path<string>(0),
            ParameterResolver.Path<string>(1));

        var exception = Assert.Throws<HandlerMissingException>(() => handler.Execute(Request.WithMethod(Method.Get),
            "my-post",
            "my-comment", Logger));
        Assert.Equal("No handler defined for GET /posts/{postId}/comment/{commentId}",
            exception.Message);
    }

    [Fact]
    public void ActionSignature()
    {
        var handler = CreateRequestHandler(
            Method.Get,
            "/posts/{postId}/comment/{commentId}",
            ParameterResolver.Path<string>(0),
            ParameterResolver.Path<string>(1));

        Assert.Equal("String postId, String commentId", handler.ActionSignature);
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
                "/posts/{postId}/comment/{commentId}",
                ParameterResolver.Path<string>(0),
                ParameterResolver.Path<string>(1))
            .Handle((postId, commentId)
                => Completes.WithSuccess(Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized(
                    $"{postId} {commentId}"))));

        var response = handler.Execute(request, mappedParameters, Logger).Outcome;

        AssertResponsesAreEquals(
            Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized("my-post my-comment")),
            response);
    }

    [Fact]
    public void AddingHandlerParam()
    {
        var request = Request.Has(Method.Get)
            .And("/posts/my-post/comment/my-comment/votes/10".ToMatchableUri())
            .And(Version.Http1_1);
        var mappedParameters =
            new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>
            {
                new Action.MappedParameter("String", "my-post"),
                new Action.MappedParameter("String", "my-comment"),
                new Action.MappedParameter("int", 10)
            });

        var
            handler = CreateRequestHandler(
                    Method.Get,
                    "/posts/{postId}/comment/{commentId}/votes/{votesNumber}",
                    ParameterResolver.Path<string>(0),
                    ParameterResolver.Path<string>(1))
                .Param<int>();

        AssertResolvesAreEquals(ParameterResolver.Path<int>(2), handler.ResolverParam3);
        Assert.Equal(10, handler.ResolverParam3.Apply(request, mappedParameters));
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
                "/posts/{postId}/comment/{commentId}",
                ParameterResolver.Path<string>(0),
                ParameterResolver.Path<string>(1))
            .Body<NameData>();

        AssertResolvesAreEquals(ParameterResolver.Body<NameData>(), handler.ResolverParam3);
        Assert.Equal(new NameData("John", "Doe"), handler.ResolverParam3.Apply(request, mappedParameters));
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

#pragma warning disable 618
        var handler = CreateRequestHandler(
                Method.Post,
                "/posts/{postId}/comment/{commentId}",
                ParameterResolver.Path<string>(0),
                ParameterResolver.Path<string>(1))
            .Body<NameData>(typeof(TestMapper));
#pragma warning restore 618

        AssertResolvesAreEquals(ParameterResolver.Body<NameData>(), handler.ResolverParam3);
        Assert.Equal(new NameData("John", "Doe"), handler.ResolverParam3.Apply(request, mappedParameters));
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
                "/posts/{postId}/comment/{commentId}",
                ParameterResolver.Path<string>(0),
                ParameterResolver.Path<string>(1))
            .Query("filter");

        AssertResolvesAreEquals(ParameterResolver.Query<string>("filter"), handler.ResolverParam3);
        Assert.Equal("abc", handler.ResolverParam3.Apply(request, mappedParameters));
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
                "/posts/{postId}/comment/{commentId}",
                ParameterResolver.Path<string>(0),
                ParameterResolver.Path<string>(1))
            .Header("Host");

        AssertResolvesAreEquals(ParameterResolver.Header("Host"), handler.ResolverParam3);
        Assert.Equal(hostHeader, handler.ResolverParam3.Apply(request, mappedParameters));
    }

    public RequestHandler2Test(ITestOutputHelper output) : base(output)
    {
    }

    private RequestHandler2<T, R> CreateRequestHandler<T, R>(Method method,
        string path,
        ParameterResolver<T> parameterResolver1,
        ParameterResolver<R> parameterResolver2)
    {
        return new RequestHandler2<T, R>(
            method,
            path,
            parameterResolver1,
            parameterResolver2,
            ErrorHandler.HandleAllWith(ResponseStatus.InternalServerError),
            DefaultMediaTypeMapper.Instance);
    }
}
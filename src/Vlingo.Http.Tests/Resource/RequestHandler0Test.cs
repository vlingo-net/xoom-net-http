// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Common;
using Vlingo.Http.Resource;
using Vlingo.Http.Tests.Sample.User;
using Xunit;
using Xunit.Abstractions;
using Action = Vlingo.Http.Resource.Action;

namespace Vlingo.Http.Tests.Resource
{
    public class RequestHandler0Test : RequestHandlerTestBase
    {
        [Fact]
        public void SimpleHandler()
        {
            var handler = new RequestHandler0(Method.Get, "/helloworld")
                .Handle(() => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Created)));
            var response = handler.Execute(Request.WithMethod(Method.Get), Logger).Outcome;

            Assert.NotNull(handler);
            Assert.Equal(Method.Get, handler.Method);
            Assert.Equal("/helloworld", handler.Path);
            AssertResponsesAreEquals(Response.Of(Response.ResponseStatus.Created), response);
        }

        [Fact]
        public void SimpleHandlerWithBinaryResponse()
        {
            byte[] body = {1, 2, 1, 2};
            var handler = new RequestHandler0(Method.Get, "/helloworld")
                .Handle(() => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Created, Body.From(body, Body.Encoding.None))));
            var response = handler.Execute(Request.WithMethod(Method.Get), Logger).Outcome;

            Assert.NotNull(handler);
            Assert.Equal(Method.Get, handler.Method);
            Assert.Equal("/helloworld", handler.Path);
            Assert.Equal(Body.From(body, Body.Encoding.None).BinaryContent, response.Entity.BinaryContent);
        }

        [Fact]
        public void ThrowExceptionWhenNoHandlerIsDefined()
        {
            var handler = new RequestHandler0(Method.Get, "/helloworld");

            var exception = Assert.Throws<HandlerMissingException>(() => handler.Execute(Request.WithMethod(Method.Get), Logger));
            Assert.Equal("No handler defined for GET /helloworld", exception.Message);
        }

        [Fact]
        public void ActionSignatureIsEmpty()
        {
            var handler = new RequestHandler0(Method.Get, "/helloworld").Handle(() => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Created)));

            Assert.Equal("", handler.ActionSignature);
        }

        [Fact]
        public void ExecuteWithRequestAndMappedParametersHasToReturnTheSameAsExecute()
        {
            var request = Request.Has(Method.Get)
                .And("/hellworld".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>());
            var handler = new RequestHandler0(Method.Get, "/helloworld")
                .Handle(()  => Completes.WithSuccess(Response.Of(Response.ResponseStatus.Created)));
            var response = handler.Execute(request, mappedParameters, Logger).Outcome;

            Assert.NotNull(handler);
            Assert.Equal(Method.Get, handler.Method);
            Assert.Equal("/helloworld", handler.Path);
            AssertResponsesAreEquals(Response.Of(Response.ResponseStatus.Created), response);
        }

        //region adding handlers to RequestHandler0

        [Fact]
        public void AddingHandlerParam()
        {
            var request = Request.Has(Method.Get)
                .And("/user/admin".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>
                {
                    new Action.MappedParameter("String", "admin")   
                });

            var handler = new RequestHandler0(Method.Get, "/user/{userId}").Param<string>();

            AssertResolvesAreEquals(ParameterResolver.Path<string>(0), handler.Resolver);
            Assert.Equal("admin", handler.Resolver.Apply(request, mappedParameters));
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

            var handler = new RequestHandler0(Method.Get, "/user/admin/name").Body<NameData>();

            AssertResolvesAreEquals(ParameterResolver.Body<NameData>(), handler.Resolver);
            Assert.Equal(new NameData("John", "Doe"), handler.Resolver.Apply(request, mappedParameters));
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
            var handler = new RequestHandler0(Method.Get, "/user/admin/name").Body<NameData>(typeof(TestMapper));
#pragma warning restore 618

            AssertResolvesAreEquals(ParameterResolver.Body<NameData>(new TestMapper()), handler.Resolver);
            Assert.Equal(new NameData("John", "Doe"), handler.Resolver.Apply(request, mappedParameters));
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

#pragma warning disable 618
            var handler = new RequestHandler0(Method.Get, "/user/admin/name").Body<NameData>(typeof(TestMapper));
#pragma warning restore 618

            AssertResolvesAreEquals(ParameterResolver.Body<NameData>(new TestMapper()), handler.Resolver);
            Assert.Equal(new NameData("John", "Doe"), handler.Resolver.Apply(request, mappedParameters));
        }

        [Fact]
        public void AddingHandlerQuery()
        {
            var request = Request.Has(Method.Get)
                .And("/user?filter=abc".ToMatchableUri())
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>());

            var handler = new RequestHandler0(Method.Get, "/user").Query("filter");

            AssertResolvesAreEquals(ParameterResolver.Query<string>("filter"), handler.Resolver);
            Assert.Equal("abc", handler.Resolver.Apply(request, mappedParameters));
        }


        [Fact]
        public void AddingHandlerHeader()
        {
            var hostHeader = RequestHeader.Of("Host", "www.vlingo.io");
            var request = Request.Has(Method.Get)
                .And("/user?filter=abc".ToMatchableUri())
                .And(Headers.Of(hostHeader))
                .And(Version.Http1_1);
            var mappedParameters =
                new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>());

            var handler = new RequestHandler0(Method.Get, "/user").Header("Host");

            AssertResolvesAreEquals(ParameterResolver.Header("Host"), handler.Resolver);
            Assert.Equal(hostHeader, handler.Resolver.Apply(request, mappedParameters));
        }

        public RequestHandler0Test(ITestOutputHelper output) : base(output)
        {
        }
    }
}
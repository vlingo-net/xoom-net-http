// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Http.Media;
using Vlingo.Http.Resource;
using Vlingo.Http.Tests.Sample.User;
using Xunit;
using Action = Vlingo.Http.Resource.Action;

namespace Vlingo.Http.Tests.Resource
{
    public class ParameterResolverTest
    {
        private readonly Request _request;
        private readonly Action.MappedParameters _mappedParameters;

        [Fact]

        public void Path()
        {
            var resolver = ParameterResolver.Path<string>(0);

            var result = resolver.Apply(_request, _mappedParameters);

            Assert.Equal("my-post", result);
            Assert.Equal(ParameterResolver.Type.Path, resolver.Type);
        }

        [Fact]
        public void Body()
        {
            var resolver = ParameterResolver.Body<NameData>();

            var result = resolver.Apply(_request, _mappedParameters);
            var expected = new NameData("John", "Doe");

            Assert.Equal(expected.ToString(), result.ToString());
            Assert.Equal(ParameterResolver.Type.Body, resolver.Type);
        }

        [Fact]
        public void BodyWithContentTypeMapper()
        {
            var mediaTypeMapper = new MediaTypeMapper.Builder()
                .AddMapperFor(ContentMediaType.Json, DefaultJsonMapper.Instance)
                .Build();

            var resolver = ParameterResolver.Body<NameData>(mediaTypeMapper);

            var result = resolver.Apply(_request, _mappedParameters);
            var expected = new NameData("John", "Doe");

            Assert.Equal(expected.ToString(), result.ToString());
            Assert.Equal(ParameterResolver.Type.Body, resolver.Type);
        }

        [Fact]
        public void Header()
        {
            var resolver = ParameterResolver.Header("Host");

            var result = resolver.Apply(_request, _mappedParameters);

            Assert.Equal("Host", result.Name);
            Assert.Equal("www.vlingo.io", result.Value);
            Assert.Equal(ParameterResolver.Type.Header, resolver.Type);
        }

        [Fact]
        public void Query()
        {
            var resolver = ParameterResolver.Query<string>("page");

            var result = resolver.Apply(_request, _mappedParameters);

            Assert.Equal("10", result);
            Assert.Equal(ParameterResolver.Type.Query, resolver.Type);
        }

        [Fact]

        public void QueryWithType()
        {
            var resolver = ParameterResolver.Query<int>("page");

            var result = resolver.Apply(_request, _mappedParameters);

            Assert.Equal(10, result);
            Assert.Equal(ParameterResolver.Type.Query, resolver.Type);
        }

        [Fact]

        public void QueryShouldReturnDefaultWhenItIsNotPresent()
        {
            var resolver = ParameterResolver.Query<int>("pageSize", 50);

            var result = resolver.Apply(_request, _mappedParameters);

            Assert.Equal(50, result);
            Assert.Equal(ParameterResolver.Type.Query, resolver.Type);
        }

        public ParameterResolverTest()
        {
            _request = Request.Has(Method.Post)
                .And(Version.Http1_1)
                .And("/user/my-post?page=10".ToMatchableUri())
                .And(RequestHeader.FromString("Host:www.vlingo.io"))
                .And(Http.Body.From("{\"given\":\"John\",\"family\":\"Doe\"}"));

            _mappedParameters = new Action.MappedParameters(1, Method.Get, "ignored", new List<Action.MappedParameter>
            {
                new Action.MappedParameter("String", "my-post")
            });
        }
    }
}
// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Http.Media;
using Vlingo.Xoom.Http.Resource;
using Xunit;

namespace Vlingo.Xoom.Http.Tests.Resource;

public class MediaTypeMapperTest
{
    [Fact]
    public void Registered_mapper_maps_type()
    {
        var mappedToObject = new Object();
        var mappedToString = "mappedToString";

        var testMapper = new TestMapper<object>(mappedToObject, mappedToString);
        var mediaTypeMapper = new MediaTypeMapper.Builder()
            .AddMapperFor(ContentMediaType.Json, testMapper)
            .Build();

        Assert.Equal(mappedToString, mediaTypeMapper.From(new object(), ContentMediaType.Json));
    }

    [Fact]
    public void Exception_thrown_for_invalid_mapper()
    {
        var mediaTypeMapper = new MediaTypeMapper.Builder().Build();

        Assert.Throws<MediaTypeNotSupportedException>(() =>
            mediaTypeMapper.From(new object(), ContentMediaType.Json));
    }

    [Fact]
    public void Parameters_do_not_affect_mapping()
    {
        var mediaTypeMapper = new MediaTypeMapper.Builder()
            .AddMapperFor(ContentMediaType.Json, DefaultJsonMapper.Instance)
            .Build();
        var contentMediaType = ContentMediaType.ParseFromDescriptor("application/json; charset=UTF-8");

        mediaTypeMapper.From(new object(), contentMediaType);
    }
        
#pragma warning disable 8632
    private class TestMapper<T> : IMapper
    {
        private readonly string _returnString;
        private readonly T _returnObject;

        public TestMapper(T mappedToObject, string mappedToString)
        {
            _returnObject = mappedToObject;
            _returnString = mappedToString;
        }
            
        public object? From(string? data, Type? type) => _returnObject;

        public string? From<T1>(T1 data) => _returnString;
    }
#pragma warning restore 8632
}
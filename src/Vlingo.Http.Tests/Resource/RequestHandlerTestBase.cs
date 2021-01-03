// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Http.Media;
using Vlingo.Http.Resource;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Http.Tests.Resource
{
    public class RequestHandlerTestBase
    {
        protected readonly ILogger Logger;

        protected RequestHandlerTestBase(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            Logger = ConsoleLogger.TestInstance();
        }

        protected void AssertResponsesAreEquals(Response expected, Response actual) => Assert.Equal(expected.ToString(), actual.ToString());

        internal void AssertResolvesAreEquals<T>(ParameterResolver<T> expected, ParameterResolver<T> actual)
        {
            Assert.Equal(expected.Type, actual.Type);
            Assert.Equal(expected.ParamClass, actual.ParamClass);
        }

        protected MediaTypeMapper DefaultMediaTypeMapperForJson()
        {

            return new MediaTypeMapper.Builder()
                .AddMapperFor(ContentMediaType.Json, new DefaultJsonMapper())
                .Build();
        }
    }
}
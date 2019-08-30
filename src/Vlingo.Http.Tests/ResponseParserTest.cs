// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using Vlingo.Http.Tests.Resource;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Http.Tests
{
    public class ResponseParserTest : ResourceTestFixtures
    {
        List<String> _uniqueBodies = new List<string>();
        
        [Fact]
        public void TestThatSingleResponseParses()
        {
            var parser = ResponseParser.ParserFor(ToStream(JohnDoeCreated()).ToArray());

            Assert.True(parser.HasCompleted);
            Assert.True(parser.HasFullResponse);
            Assert.False(parser.IsMissingContent);
            Assert.False(parser.HasMissingContentTimeExpired((long)DateExtensions.GetCurrentMillis() + 100));

            var response = parser.FullResponse;

            Assert.NotNull(response);
            Assert.True(response.Version.IsHttp1_1());
            Assert.Equal(JohnDoeUserSerialized, response.Entity.Content);
        }
        
        private string MultipleResponseBuilder(int amount)
        {
            var builder = new StringBuilder();

            for (var idx = 1; idx <= amount; ++idx)
            {
                var body = (idx % 2 == 0) ? UniqueJaneDoe() : UniqueJohnDoe();
                _uniqueBodies.Add(body);
                builder.Append(CreatedResponse(body));
            }

            return builder.ToString();
        }

        public ResponseParserTest(ITestOutputHelper output) : base(output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
        }
    }
}
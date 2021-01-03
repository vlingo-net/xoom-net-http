// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vlingo.Http.Tests.Resource;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Http.Tests
{
    public class ResponseParserTest : ResourceTestFixtures
    {
        private readonly List<string> _uniqueBodies = new List<string>();

        [Fact]
        public void TestThatSingleResponseParses()
        {
            var parser = ResponseParser.ParserFor(ToStream(JohnDoeCreated()).ToArray());

            Assert.True(parser.HasCompleted);
            Assert.True(parser.HasFullResponse());
            Assert.False(parser.IsMissingContent);
            Assert.False(parser.HasMissingContentTimeExpired((long) DateExtensions.GetCurrentMillis() + 100));

            var response = parser.FullResponse();

            Assert.NotNull(response);
            Assert.True(response.Version.IsHttp1_1());
            Assert.Equal(JohnDoeUserSerialized, response.Entity.Content);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(200)]
        public void TestThatMultipleResponsesParse(int responses)
        {
            var parser = ResponseParser.ParserFor(ToStream(MultipleResponseBuilder(responses)).ToArray());

            Assert.True(parser.HasCompleted);
            Assert.True(parser.HasFullResponse());
            Assert.False(parser.IsMissingContent);
            Assert.False(parser.HasMissingContentTimeExpired((long) DateExtensions.GetCurrentMillis() + 100));

            var count = 0;
            var bodyIterator = _uniqueBodies.GetEnumerator();
            while (parser.HasFullResponse())
            {
                ++count;
                var response = parser.FullResponse();

                Assert.NotNull(response);
                Assert.True(response.Version.IsHttp1_1());
                Assert.True(bodyIterator.MoveNext());
                var body = bodyIterator.Current;
                Assert.Equal(body, response.Entity.Content);
            }

            Assert.Equal(responses, count);
            
            bodyIterator.Dispose();
        }

        [Fact]
        public void TestThatTwoHundredResponsesParseParseNextSucceeds()
        {
            var manyResponses = MultipleResponseBuilder(200);

            var totalLength = manyResponses.Length;
            var alteringEndIndex = 1024;
            var parser = ResponseParser.ParserFor(ToStream(manyResponses.Substring(0, alteringEndIndex)).ToArray());
            var startingIndex = alteringEndIndex;

            while (startingIndex < totalLength)
            {
                alteringEndIndex = startingIndex + 1024 + (int)(DateExtensions.GetCurrentMillis() % startingIndex);
                if (alteringEndIndex > totalLength)
                {
                    alteringEndIndex = totalLength;
                }

                parser.ParseNext(ToStream(manyResponses.Substring(startingIndex, alteringEndIndex - startingIndex)).ToArray());
                startingIndex = alteringEndIndex;
            }

            Assert.True(parser.HasCompleted);
            Assert.True(parser.HasFullResponse());
            Assert.False(parser.IsMissingContent);
            Assert.False(parser.HasMissingContentTimeExpired((long) DateExtensions.GetCurrentMillis() + 100));

            var count = 0;
            var bodyIterator = _uniqueBodies.GetEnumerator();
            while (parser.HasFullResponse())
            {
                ++count;
                var response = parser.FullResponse();

                Assert.NotNull(response);
                Assert.True(response.Version.IsHttp1_1());
                Assert.True(bodyIterator.MoveNext());
                var body = bodyIterator.Current;
                Assert.Equal(body, response.Entity.Content);
            }

            Assert.Equal(200, count);
            
            bodyIterator.Dispose();
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
        }
    }
}
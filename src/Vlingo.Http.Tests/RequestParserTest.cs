// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Text;
using Vlingo.Http.Tests.Resource;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Http.Tests
{
    public class RequestParserTest : ResourceTestFixtures
    {
        private readonly List<string> _uniqueBodies = new List<string>();

        [Fact]
        public void TestThatSingleResponseParses()
        {
            var parser = RequestParser.ParserFor(ToStream(PostJohnDoeUserMessage).ToArray());

            Assert.True(parser.HasCompleted);
            Assert.True(parser.HasFullRequest());
            Assert.False(parser.IsMissingContent);
            Assert.False(parser.HasMissingContentTimeExpired((long) DateExtensions.GetCurrentMillis() + 100));

            var request = parser.FullRequest();

            Assert.NotNull(request);
            Assert.True(request.Method.IsPost());
            Assert.Equal("/users", request.Uri.PathAndQuery);
            Assert.True(request.Version.IsHttp1_1());
            Assert.Equal(JohnDoeUserSerialized, request.Body.Content);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(200)]
        public void TestThatMultipleResponsesParse(int requests)
        {
            var parser = RequestParser.ParserFor(ToStream(MultipleRequestBuilder(requests)).ToArray());

            Assert.True(parser.HasCompleted);
            Assert.True(parser.HasFullRequest());
            Assert.False(parser.IsMissingContent);
            Assert.False(parser.HasMissingContentTimeExpired((long) DateExtensions.GetCurrentMillis() + 100));

            var count = 0;
            var bodyIterator = _uniqueBodies.GetEnumerator();
            while (parser.HasFullRequest())
            {
                ++count;
                var request = parser.FullRequest();

                Assert.NotNull(request);
                Assert.True(request.Method.IsPost());
                Assert.Equal("/users", request.Uri.PathAndQuery);
                Assert.True(request.Version.IsHttp1_1());
                Assert.True(bodyIterator.MoveNext());
                var body = bodyIterator.Current;
                Assert.Equal(body, request.Body.Content);
            }

            Assert.Equal(requests, count);
            
            bodyIterator.Dispose();
        }

        [Fact]
        public void TestThatTwoHundredResponsesParseParseNextSucceeds()
        {
            var manyRequests = MultipleRequestBuilder(200);

            var totalLength = manyRequests.Length;
            var alteringEndIndex = 1024;
            var parser = RequestParser.ParserFor(ToStream(manyRequests.Substring(0, alteringEndIndex)).ToArray());
            var startingIndex = alteringEndIndex;

            while (startingIndex < totalLength)
            {
                alteringEndIndex = startingIndex + 1024 + (int)(DateExtensions.GetCurrentMillis() % startingIndex);
                if (alteringEndIndex > totalLength)
                {
                    alteringEndIndex = totalLength;
                }

                parser.ParseNext(ToStream(manyRequests.Substring(startingIndex, alteringEndIndex - startingIndex)).ToArray());
                startingIndex = alteringEndIndex;
            }

            Assert.True(parser.HasCompleted);
            Assert.True(parser.HasFullRequest());
            Assert.False(parser.IsMissingContent);
            Assert.False(parser.HasMissingContentTimeExpired((long) DateExtensions.GetCurrentMillis() + 100));

            var count = 0;
            var bodyIterator = _uniqueBodies.GetEnumerator();
            while (parser.HasFullRequest())
            {
                ++count;
                var request = parser.FullRequest();

                Assert.NotNull(request);
                Assert.True(request.Method.IsPost());
                Assert.Equal("/users", request.Uri.PathAndQuery);
                Assert.True(request.Version.IsHttp1_1());
                Assert.True(bodyIterator.MoveNext());
                var body = bodyIterator.Current;
                Assert.Equal(body, request.Body.Content);
            }

            Assert.Equal(200, count);
            
            bodyIterator.Dispose();
        }

        private string MultipleRequestBuilder(int amount)
        {
            var builder = new StringBuilder();

            for (var idx = 1; idx <= amount; ++idx)
            {
                var body = (idx % 2 == 0) ? UniqueJaneDoe() : UniqueJohnDoe();
                _uniqueBodies.Add(body);
                builder.Append(PostRequest(body));
            }

            return builder.ToString();
        }

        public RequestParserTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}
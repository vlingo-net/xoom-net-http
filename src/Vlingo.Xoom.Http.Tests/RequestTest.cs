// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Text;
using Vlingo.Xoom.Wire.Message;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests
{
    public class RequestTest
    {
        private readonly IConsumerByteBuffer _consumerByteBuffer = BasicConsumerByteBuffer.Allocate(2, 1024);
        private readonly string _requestOneHeader;
        private readonly string _requestTwoHeadersWithBody;
        private readonly string _requestMultiHeaders;
        private readonly string _requestMultiHeadersWithBody;
        private readonly string _requestQueryParameters;

        [Fact]
        public void testThatRequestCanHaveOneHeader()
        {
            var request = Request.From(ToConsumerByteBuffer(_requestOneHeader));

            Assert.NotNull(request);
            Assert.True(request.Method.IsGet());
            Assert.Equal("/".ToMatchableUri(), request.Uri);
            Assert.True(request.Version.IsHttp1_1());
            Assert.Single((IEnumerable) request.Headers);
            Assert.False(request.Body.HasContent);
        }

        [Fact]
        public void TestThatRequestCanHaveOneHeaderWithBody()
        {
            var request = Request.From(ToConsumerByteBuffer(_requestTwoHeadersWithBody));

            Assert.NotNull(request);
            Assert.True(request.Method.IsPut());
            Assert.Equal("/one/two/three".ToMatchableUri(), request.Uri);
            Assert.True(request.Version.IsHttp1_1());
            Assert.Equal(2, request.Headers.Count);
            Assert.True(request.Body.HasContent);
            Assert.NotNull(request.Body.Content);
            Assert.True(request.Body.HasContent);
        }

        [Fact]
        public void TestThatRequestCanHaveMultipleHeaders()
        {
            var request = Request.From(ToConsumerByteBuffer(_requestMultiHeaders));

            Assert.NotNull(request);
            Assert.True(request.Method.IsGet());
            Assert.Equal("/one".ToMatchableUri(), request.Uri);
            Assert.True(request.Version.IsHttp1_1());
            Assert.Equal(3, request.Headers.Count);
            Assert.False(request.Body.HasContent);
        }

        [Fact]
        public void TestThatAddingABodyAddsContentLengthHeader()
        {
            var exampleContent = Guid.NewGuid().ToString();
            var request = Request.Has(Method.Put).And(Body.From(exampleContent));

            Assert.Equal($"{exampleContent.Length}", request.HeaderOf("Content-Length").Value);
        }

        [Fact]
        public void TestThatRequestCanHaveMutipleHeadersAndBody()
        {
            var request = Request.From(ToConsumerByteBuffer(_requestMultiHeadersWithBody));

            Assert.NotNull(request);
            Assert.True(request.Method.IsPost());
            Assert.Equal("/one/two/".ToMatchableUri(), request.Uri);
            Assert.True(request.Version.IsHttp1_1());
            Assert.Equal(4, request.Headers.Count);
            Assert.Equal(RequestHeader.Host, request.HeaderOf(RequestHeader.Host).Name);
            Assert.Equal(RequestHeader.ContentLength, request.HeaderOf(RequestHeader.ContentLength).Name);
            Assert.Equal(RequestHeader.Accept, request.HeaderOf(RequestHeader.Accept).Name);
            Assert.Equal(RequestHeader.CacheControl, request.HeaderOf(RequestHeader.CacheControl).Name);
            Assert.True(request.Body.HasContent);
            Assert.NotNull(request.Body.Content);
            Assert.True(request.Body.HasContent);
            Assert.Equal("{ text:\"some text\"}", request.Body.ToString());
        }

        [Fact]
        public void TestRejectBogusMethodRequest()
        {
            Assert.Throws<ArgumentException>(() => Request.From(ToConsumerByteBuffer("BOGUS / HTTP/1.1\nHost: test.com\n\n")));
        }

        [Fact]
        public void TestRejectUnsupportedVersionRequest()
        {
            Assert.Throws<ArgumentException>(() => Request.From(ToConsumerByteBuffer("GET / HTTP/0.1\nHost: test.com\n\n")));
        }

        [Fact]
        public void TestRejectBadRequestNoHeader()
        {
            Assert.Throws<ArgumentException>(() => Request.From(ToConsumerByteBuffer("GET / HTTP/1.1\n\n")));
        }

        [Fact]
        public void TestRejectBadRequestMissingLine()
        {
            Assert.Throws<InvalidOperationException>(() => Request.From(ToConsumerByteBuffer("GET / HTTP/1.1\nHost: test.com\n")));
        }

        [Fact]
        public void TestFindHeader()
        {
            var request = Request.From(ToConsumerByteBuffer(_requestTwoHeadersWithBody));

            Assert.NotNull(request.HeaderOf(RequestHeader.Host));
            Assert.Equal(RequestHeader.Host, request.HeaderOf(RequestHeader.Host).Name);
            Assert.Equal(RequestHeader.ContentLength, request.HeaderOf(RequestHeader.ContentLength).Name);
            Assert.Equal("test.com", request.HeaderOf(RequestHeader.Host).Value);
        }

        [Fact]
        public void TestFindHeaders()
        {
            var request = Request.From(ToConsumerByteBuffer(_requestMultiHeaders));

            Assert.NotNull(request.HeaderOf(RequestHeader.Host));
            Assert.Equal(RequestHeader.Host, request.HeaderOf(RequestHeader.Host).Name);
            Assert.Equal("test.com", request.HeaderOf(RequestHeader.Host).Value);

            Assert.NotNull(request.HeaderOf(RequestHeader.Accept));
            Assert.Equal(RequestHeader.Accept, request.HeaderOf(RequestHeader.Accept).Name);
            Assert.Equal("text/plain", request.HeaderOf(RequestHeader.Accept).Value);

            Assert.NotNull(request.HeaderOf(RequestHeader.CacheControl));
            Assert.Equal(RequestHeader.CacheControl, request.HeaderOf(RequestHeader.CacheControl).Name);
            Assert.Equal("no-cache", request.HeaderOf(RequestHeader.CacheControl).Value);
        }

        [Fact]
        public void TestQueryParameters()
        {
            var request = Request.From(ToConsumerByteBuffer(_requestQueryParameters));
            var queryParameters = request.QueryParameters;
            Assert.Equal(4, queryParameters.Names.Count);
            Assert.Equal("1", queryParameters.ValuesOf("one")[0]);
            Assert.Equal("2", queryParameters.ValuesOf("two")[0]);
            Assert.Equal("3", queryParameters.ValuesOf("three")[0]);
            Assert.Equal("NY", queryParameters.ValuesOf("state")[0]);
            Assert.Equal("CO", queryParameters.ValuesOf("state")[1]);
        }

        [Fact]
        public void TestRequestBuilder()
        {
            Assert.Equal(_requestOneHeader,
                Request
                    .Has(Method.Get)
                    .And("/".ToMatchableUri())
                    .And(RequestHeader.WithHost("test.com"))
                    .ToString());

            Assert.Equal(_requestOneHeader,
                Request
                    .WithMethod(Method.Get)
                    .WithUri("/")
                    .WithHeader(RequestHeader.Host, "test.com")
                    .ToString());

            Assert.Equal(_requestTwoHeadersWithBody,
                Request
                    .Has(Method.Put)
                    .And("/one/two/three".ToMatchableUri())
                    .And(RequestHeader.WithHost("test.com"))
                    .And(RequestHeader.WithContentLength(19))
                    .And(Body.From("{ text:\"some text\"}"))
                    .ToString());

            Assert.Equal(_requestTwoHeadersWithBody,
                Request
                    .WithMethod(Method.Put)
                    .WithUri("/one/two/three")
                    .WithHeader(RequestHeader.Host, "test.com")
                    .WithHeader(RequestHeader.ContentLength, 19)
                    .WithBody("{ text:\"some text\"}")
                    .ToString());

            Assert.Equal(_requestMultiHeaders,
                Request
                    .Has(Method.Get)
                    .And("/one".ToMatchableUri())
                    .And(RequestHeader.WithHost("test.com"))
                    .And(RequestHeader.WithAccept("text/plain"))
                    .And(RequestHeader.WithCacheControl("no-cache"))
                    .ToString());

            Assert.Equal(_requestMultiHeaders,
                Request
                    .WithMethod(Method.Get)
                    .WithUri("/one")
                    .WithHeader(RequestHeader.Host, "test.com")
                    .WithHeader(RequestHeader.Accept, "text/plain")
                    .WithHeader(RequestHeader.CacheControl, "no-cache")
                    .ToString());

            Assert.Equal(_requestMultiHeadersWithBody,
                Request
                    .Has(Method.Post)
                    .And("/one/two/".ToMatchableUri())
                    .And(RequestHeader.WithHost("test.com"))
                    .And(RequestHeader.WithContentLength(19))
                    .And(RequestHeader.WithAccept("text/plain"))
                    .And(RequestHeader.WithCacheControl("no-cache"))
                    .And(Body.From("{ text:\"some text\"}"))
                    .ToString());

            Assert.Equal(_requestMultiHeadersWithBody,
                Request
                    .Has(Method.Post)
                    .WithUri("/one/two/")
                    .WithHeader(RequestHeader.Host, "test.com")
                    .WithHeader(RequestHeader.ContentLength, 19)
                    .WithHeader(RequestHeader.Accept, "text/plain")
                    .WithHeader(RequestHeader.CacheControl, "no-cache")
                    .WithBody("{ text:\"some text\"}")
                    .ToString());
        }
        
        [Fact]
        public void TestExtendedCharactersContentLength()
        {
            var asciiWithExtendedCharacters = ExtendedCharactersFixture.AsciiWithExtendedCharacters();

            var request =
                Request
                    .Has(Method.Post)
                    .WithUri("/one/two/")
                    .WithHeader(RequestHeader.Host, "test.com")
                    .WithBody(asciiWithExtendedCharacters);

            var contentLength = int.Parse(request.HeaderValueOr(RequestHeader.ContentLength, "0"));

            Assert.False(contentLength == 0);

            Assert.True(asciiWithExtendedCharacters.Length < contentLength);

            Assert.Equal(Converters.TextToBytes(asciiWithExtendedCharacters).Length, contentLength);
        }

        public RequestTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);

            _requestOneHeader = "GET / HTTP/1.1\nHost: test.com\n\n";

            _requestTwoHeadersWithBody =
                "PUT /one/two/three HTTP/1.1\nHost: test.com\nContent-Length: 19\n\n{ text:\"some text\"}";

            _requestMultiHeaders = "GET /one HTTP/1.1\nHost: test.com\nAccept: text/plain\nCache-Control: no-cache\n\n";

            _requestMultiHeadersWithBody =
                "POST /one/two/ HTTP/1.1\nHost: test.com\nContent-Length: 19\nAccept: text/plain\nCache-Control: no-cache\n\n{ text:\"some text\"}";

            _requestQueryParameters =
                "GET /one/param1?one=1&two=2&three=3&state=NY&state=CO HTTP/1.1\nHost: test.com\n\n";
        }

        private IConsumerByteBuffer ToConsumerByteBuffer(string requestContent)
        {
            _consumerByteBuffer.Clear();
            _consumerByteBuffer.Put(Encoding.UTF8.GetBytes(requestContent));
            _consumerByteBuffer.Flip();
            return _consumerByteBuffer;
        }
    }
}
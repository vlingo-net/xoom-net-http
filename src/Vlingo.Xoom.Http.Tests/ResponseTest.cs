// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Wire.Message;
using Xunit;

namespace Vlingo.Xoom.Http.Tests
{
    public class ResponseTest
    {
        [Fact]
        public void TestResponseWithOneHeaderNoEntity()
        {
            var response = Response.Of(Version.Http1_1, ResponseStatus.Ok, ResponseHeader.WithHeaders(ResponseHeader.CacheControl, "max-age=3600"));

            var facsimile = "HTTP/1.1 200 OK\nCache-Control: max-age=3600\nContent-Length: 0\n\n";

            Assert.Equal(facsimile, response.ToString());
        }

        [Fact]
        public void TestResponseWithOneHeaderAndEntity()
        {
            var body = "{ text : \"some text\" }";
            var response = Response.Of(Version.Http1_1, ResponseStatus.Ok, ResponseHeader.WithHeaders(ResponseHeader.CacheControl, "max-age=3600"), body);

            var facsimile =
                $"HTTP/1.1 200 OK\nCache-Control: max-age=3600\nContent-Length: {body.Length}\n\n{{ text : \"some text\" }}";

            Assert.Equal(facsimile, response.ToString());
        }

        [Fact]
        public void TestBinaryBodyResponseWithOneHeaderAndEntity()
        {
            byte[] body = {1, 2, 1, 2};
            var response = Response.Of(Version.Http1_1, ResponseStatus.Ok,
                Headers.Of(
                    ResponseHeader.Of(ResponseHeader.ContentType, "application/octet-stream"),
                    ResponseHeader.Of(ResponseHeader.ContentLength, body.Length))
                , Body.From(body, Body.Encoding.None));


            Assert.Equal(body, response.Entity.BinaryContent);
        }

        [Fact]
        public void TestResponseWithMultipleHeadersNoEntity()
        {
            var response = Response.Of(Version.Http1_1, ResponseStatus.Ok,
                ResponseHeader.WithHeaders(ResponseHeader.Of(ResponseHeader.ETag, "123ABC")).And(ResponseHeader.Of(ResponseHeader.CacheControl, "max-age=3600")));

            var facsimile = "HTTP/1.1 200 OK\nETag: 123ABC\nCache-Control: max-age=3600\nContent-Length: 0\n\n";

            Assert.Equal(facsimile, response.ToString());
        }

        [Fact]
        public void TestResponseWithMultipleHeadersAndEntity()
        {
            var body = "{ text : \"some text\" }";
            var response = Response.Of(Version.Http1_1, ResponseStatus.Ok,
                ResponseHeader.WithHeaders(ResponseHeader.Of(ResponseHeader.ETag, "123ABC")).And(ResponseHeader.Of(ResponseHeader.CacheControl, "max-age=3600")), body);

            var facsimile =
                $"HTTP/1.1 200 OK\nETag: 123ABC\nCache-Control: max-age=3600\nContent-Length: {body.Length}\n\n{{ text : \"some text\" }}";

            Assert.Equal(facsimile, response.ToString());
        }

        [Fact]
        public void TestThatChunkedResponseIsValid()
        {
            var chunk1 = "ABCDEFGHIJKLMNOPQRSTUVWYYZ0123";
            var chunk2 = "abcdefghijklmnopqrstuvwxyz012345";

            var chunks = $"{chunk1.Length:x8}\r\n{chunk1}\r\n{chunk2.Length:x8}\r\n{chunk2}\r\n0\r\n";
            var responseMultiHeadersWithChunkedBody =
                $"HTTP/1.1 200 OK\nTransfer-Encoding: chunked\nCache-Control: no-cache\n\n{chunks}";

            var response =
                Response.Of(
                    Version.Http1_1,
                    ResponseStatus.Ok,
                    ResponseHeader.WithHeaders(ResponseHeader.Of(ResponseHeader.TransferEncoding, "chunked")).And(ResponseHeader.Of(ResponseHeader.CacheControl, "no-cache")),
                    Body.BeginChunked().AppendChunk(chunk1).AppendChunk(chunk2).End());

            Assert.Equal(responseMultiHeadersWithChunkedBody, response.ToString());
        }
        
        [Fact]
        public void TestItShouldSendNoContentLengthWithInformationalStatusCodes()
        {
            var response = Response.Of(Version.Http1_1, ResponseStatus.Continue);

            var facsimile = "HTTP/1.1 100 Continue\n\n";

            Assert.Equal(facsimile, response.ToString());
        }

        [Fact]
        public void TestItShouldSendNoContentLengthWithNoContentStatusCode()
        {
            var response = Response.Of(Version.Http1_1, ResponseStatus.NoContent);

            var facsimile = "HTTP/1.1 204 No Content\n\n";

            Assert.Equal(facsimile, response.ToString());
        }

        [Fact]
        public void TestItShouldSendNoContentLengthWithNotModifiedStatusCode()
        {
            var response = Response.Of(Version.Http1_1, ResponseStatus.NotModified);

            var facsimile = "HTTP/1.1 304 Not Modified\n\n";

            Assert.Equal(facsimile, response.ToString());
        }
        
        [Fact]
        public void TestExtendedCharactersContentLength()
        {
            var asciiWithExtendedCharacters = ExtendedCharactersFixture.AsciiWithExtendedCharacters();

            var response = Response.Of(ResponseStatus.Ok, Headers.Empty<ResponseHeader>(), asciiWithExtendedCharacters);

            var contentLength = int.Parse(response.HeaderValueOr(RequestHeader.ContentLength, "0"));

            Assert.False(contentLength == 0);

            Assert.True(asciiWithExtendedCharacters.Length < contentLength);

            Assert.Equal(Converters.TextToBytes(asciiWithExtendedCharacters).Length, contentLength);
        }
    }
}
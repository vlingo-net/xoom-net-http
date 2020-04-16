// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Http.Tests
{
    public class ContentTypeTest
    {
        [Fact]
        public void TestThatContentTypeHasMediaTypeOnly()
        {
            var contentType = ContentType.Of("text/html");

            Assert.NotNull(contentType);
            Assert.Equal("text/html", contentType.MediaType);
            Assert.Equal("text/html", contentType.ToString());
            Assert.Equal(string.Empty, contentType.Charset);
            Assert.Equal(string.Empty, contentType.Boundary);
        }

        [Fact]
        public void TestThatContentTypeHasMediaTypeCharsetOnly()
        {
            var contentType = ContentType.Of("text/html", "charset=UTF-8");

            Assert.NotNull(contentType);
            Assert.Equal("text/html", contentType.MediaType);
            Assert.Equal("charset=UTF-8", contentType.Charset);
            Assert.Equal("text/html; charset=UTF-8", contentType.ToString());
            Assert.Equal(string.Empty, contentType.Boundary);
        }

        [Fact]
        public void TestThatContentTypeHasMediaTypeCharsetBoundaryOnly()
        {
            var contentType = ContentType.Of("text/html", "charset=UTF-8", "boundary=something");

            Assert.NotNull(contentType);
            Assert.Equal("text/html", contentType.MediaType);
            Assert.Equal("charset=UTF-8", contentType.Charset);
            Assert.Equal("boundary=something", contentType.Boundary);
            Assert.Equal("text/html; charset=UTF-8; boundary=something", contentType.ToString());
        }
    }
}
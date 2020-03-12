// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Http.Media;
using Xunit;

namespace Vlingo.Http.Tests.Media
{
    public class MediaTypeParserTest
    {
        [Fact]
        public void SimpleTypeEmptyParameters()
        {
            var mediaType = Parse("application/json");
            var mediaTypeExpected = new MediaTypeDescriptor.Builder<MediaTypeTest>(
                    (a, b, c) => new MediaTypeTest(a, b, c))
                .WithMimeType("application")
                .WithMimeSubType("json")
                .Build();

            Assert.Equal(mediaTypeExpected, mediaType);
        }
        
        [Fact]
        public void ParseParameters()
        {
            var mediaTypeDescriptor = Parse("application/*;q=0.8;foo=bar");

            var mediaTypeExpected = new MediaTypeDescriptor.Builder<MediaTypeTest>(
                    (a, b, c) => new MediaTypeTest(a, b, c))
                .WithMimeType("application")
                .WithMimeSubType("*")
                .WithParameter("q", "0.8")
                .WithParameter("foo", "bar")
                .Build();

            Assert.Equal(mediaTypeExpected, mediaTypeDescriptor);
            Assert.Equal("application/*;q=0.8;foo=bar", mediaTypeDescriptor.ToString());
        }
        
        [Fact]
        public void IncorrectFormatUsesEmptyStringAndDefaultQuality()
        {
            var mediaType = Parse("typeOnly");
            var mediaTypeExpected = new MediaTypeDescriptor.Builder<MediaTypeTest>(
                    (a, b, c) => new MediaTypeTest(a, b, c))
                .WithMimeType("")
                .WithMimeSubType("")
                .Build();

            Assert.Equal(mediaTypeExpected, mediaType);
        }
        
        private MediaTypeTest Parse(string descriptor)
        {
            return MediaTypeParser.ParseFrom(descriptor,
                new MediaTypeDescriptor.Builder<MediaTypeTest>(
                    (a, b, c) => new MediaTypeTest(a, b, c)));
        }
        
        private class MediaTypeTest : MediaTypeDescriptor
        {
            public MediaTypeTest(string mimeType, string mimeSubType, IDictionary<String, String> parameters) 
                : base(mimeType, mimeSubType, parameters)
            {
            }
        }
    }
}
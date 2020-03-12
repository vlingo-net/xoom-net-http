// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Http.Media;
using Vlingo.Http.Resource;
using Xunit;

namespace Vlingo.Http.Tests.Media
{
    public class ContentMediaTypeTest
    {
        [Fact]
        public void WildCardsAreNotAllowed()
        {
            Assert.Throws<MediaTypeNotSupportedException>(() => new ContentMediaType("application", "*"));
        }
        
        [Fact]
        public void InvalidMimeTypeNotAllowed()
        {
            Assert.Throws<MediaTypeNotSupportedException>(() => new ContentMediaType("unknownMimeType", "foo"));
        }
        
        [Fact]
        public void BuilderCreates()
        {
            var builder = new MediaTypeDescriptor.Builder<ContentMediaType>((a, b, c) => new ContentMediaType(a, b, c));
            var contentMediaType = builder
                .WithMimeType(ContentMediaType.MimeTypes.Application.ToString().ToLower())
                .WithMimeSubType("json")
                .Build();

            Assert.Equal(ContentMediaType.Json, contentMediaType);
        }
        
        [Fact]
        public void BuiltInTypesHaveCorrectFormat()
        {
            var jsonType = new ContentMediaType("application", "json");
            Assert.Equal(jsonType, ContentMediaType.Json);

            var xmlType = new ContentMediaType("application", "xml");
            Assert.Equal(xmlType, ContentMediaType.Xml);
        }
    }
}
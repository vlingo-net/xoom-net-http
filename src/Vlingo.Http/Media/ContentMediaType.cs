// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Http.Resource;

namespace Vlingo.Http.Media
{
    public class ContentMediaType : MediaTypeDescriptor
    {
        // IANA MIME Type List
        private enum MimeTypes
        {
            Application,
            Audio,
            Font,
            Image,
            Model,
            Text,
            Video,
            Multipart,
            Message
        }

        public ContentMediaType(string mimeType, string mimeSubType)
            : base(mimeType, mimeSubType)
        {
            Validate();
        }

        public ContentMediaType(string mimeType, string mimeSubType, IDictionary<string, string> parameters)
            : base(mimeType, mimeSubType, parameters)
        {
            Validate();
        }

        private void Validate()
        {
            // MimeTypes.valueOf(MimeType);
            if (string.Equals(MimeSubType, "*"))
            {
                throw new MediaTypeNotSupportedException($"Illegal MIME type: {ToString()}");
            }
        }

        public ContentMediaType ToBaseType()
        {
            if (Parameters == null || Parameters.Count == 0)
            {
                return this;
            }

            return new ContentMediaType(MimeType, MimeSubType);
        }

        public static ContentMediaType Json
             => new ContentMediaType(MimeTypes.Application.ToString(), "json");

        public static ContentMediaType Xml
             => new ContentMediaType(MimeTypes.Application.ToString(), "xml");

        public static ContentMediaType ParseFromDescriptor(string contentMediaTypeDescriptor)
            => MediaTypeParser.ParseFrom(
                contentMediaTypeDescriptor,
                new Builder<ContentMediaType>((a, b, c) => new ContentMediaType(a, b, c)));
    }
}

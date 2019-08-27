// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Http.Media
{
    public static class MediaTypeParser
    {
        private const int MIME_TYPE_AND_SUBTYPE_SIZE = 2;
        private const int PARAMETER_VALUE_OFFSET = 1;
        private const int PARAMETER_FIELD_OFFSET = 0;
        private const int PARAMETER_AND_VALUE_SIZE = 2;

        public static T ParseFrom<T>(string mediaTypeDescriptor, MediaTypeDescriptor.Builder<T> builder) where T : MediaTypeDescriptor
        {
            var descriptorParts = mediaTypeDescriptor.Split(MediaTypeDescriptor.ParameterSeparator);
            if (descriptorParts.Length > 1)
            {
                ParseAttributes(builder, new ArraySegment<string>(descriptorParts, 1, descriptorParts.Length-1));
            }

            var mimeParts = descriptorParts[0].Split(MediaTypeDescriptor.MimeSubtypeSeparator);
            if (mimeParts.Length == MIME_TYPE_AND_SUBTYPE_SIZE)
            {
                builder.WithMimeType(mimeParts[0].Trim())
                  .WithMimeSubType(mimeParts[1].Trim());
            }

            return builder.Build();
        }

        private static void ParseAttributes<T>(MediaTypeDescriptor.Builder<T> builder, ArraySegment<string> parameters) where T : MediaTypeDescriptor
        {
            foreach (var parameter in parameters)
            {
                var parameterFieldAndValue = parameter.Split(MediaTypeDescriptor.ParameterAssignment);

                if (parameterFieldAndValue.Length == PARAMETER_AND_VALUE_SIZE)
                {
                    var attributeName = parameterFieldAndValue[PARAMETER_FIELD_OFFSET];
                    var value = parameterFieldAndValue[PARAMETER_VALUE_OFFSET];
                    builder.WithParameter(attributeName, value);
                }
            }
        }

    }
}

// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Http.Media
{
    public static class MediaTypeParser
    {
        private const int MimeTypeAndSubtypeSize = 2;
        private const int ParameterValueOffset = 1;
        private const int ParameterFieldOffset = 0;
        private const int ParameterAndValueSize = 2;

        public static T ParseFrom<T>(string mediaTypeDescriptor, MediaTypeDescriptor.Builder<T> builder) where T : MediaTypeDescriptor
        {
            var descriptorParts = mediaTypeDescriptor.Split(MediaTypeDescriptor.ParameterSeparator);
            if (descriptorParts.Length > 1)
            {
                ParseAttributes(builder, new ArraySegment<string>(descriptorParts, 1, descriptorParts.Length-1));
            }

            var mimeParts = descriptorParts[0].Split(MediaTypeDescriptor.MimeSubtypeSeparator);
            if (mimeParts.Length == MimeTypeAndSubtypeSize)
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

                if (parameterFieldAndValue.Length == ParameterAndValueSize)
                {
                    var attributeName = parameterFieldAndValue[ParameterFieldOffset];
                    var value = parameterFieldAndValue[ParameterValueOffset];
                    builder.WithParameter(attributeName, value);
                }
            }
        }
    }
}

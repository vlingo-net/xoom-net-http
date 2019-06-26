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
    public class ResponseMediaTypeSelector
    {
        private const char ACCEPT_MEDIA_TYPE_SEPARATOR = ',';

        private readonly SortedSet<AcceptMediaType> _responseMediaTypesByPriority;
        private readonly string _mediaTypeDescriptors;

        public ResponseMediaTypeSelector(string mediaTypeDescriptors)
        {
            _mediaTypeDescriptors = mediaTypeDescriptors;
            _responseMediaTypesByPriority = new SortedSet<AcceptMediaType>();
            ParseMediaTypeDescriptors(mediaTypeDescriptors);
        }

        private void ParseMediaTypeDescriptors(string contentTypeList)
        {
            var acceptedContentTypeDescriptors = contentTypeList.Split(ACCEPT_MEDIA_TYPE_SEPARATOR);

            foreach (var acceptedContentTypeDescriptor in acceptedContentTypeDescriptors)
            {
                var acceptMediaType = MediaTypeParser.ParseFrom(
                    acceptedContentTypeDescriptor.Trim(),
                    new MediaTypeDescriptor.Builder<AcceptMediaType>((a, b, c) => new AcceptMediaType(a, b, c)));

                _responseMediaTypesByPriority.Add(acceptMediaType);
            }
        }

        public ContentMediaType SelectType(ContentMediaType[] supportedContentMediaTypes)
        {
            foreach (var responseMediaType in _responseMediaTypesByPriority)
            {
                foreach (var supportedContentMediaType in supportedContentMediaTypes)
                {
                    if (responseMediaType.IsSameOrSuperTypeOf(supportedContentMediaType))
                    {
                        return supportedContentMediaType;
                    }
                }
            }
            throw new MediaTypeNotSupportedException(_mediaTypeDescriptors);
        }


        private class AcceptMediaType : MediaTypeDescriptor, IComparer<AcceptMediaType>
        {
            private const string MIME_TYPE_WILDCARD = "*";
            private const string QUALITY_FACTOR_PARAMETER = "q";
            private const float DEFAULT_QUALITY_FACTOR_VALUE = 1.0f;

            private readonly float _qualityFactor;

            public AcceptMediaType(string mimeType, string mimeSubType)
                : base(mimeType, mimeSubType)
            {
                _qualityFactor = DEFAULT_QUALITY_FACTOR_VALUE;
            }

            public AcceptMediaType(string mimeType, string mimeSubType, IDictionary<string, string> parameters)
                : base(mimeType, mimeSubType, parameters)
            {
                var qualityFactor = DEFAULT_QUALITY_FACTOR_VALUE;

                if (parameters.ContainsKey(QUALITY_FACTOR_PARAMETER))
                {
                    if (float.TryParse(parameters[QUALITY_FACTOR_PARAMETER], out var f))
                    {
                        qualityFactor = f;
                    }
                }

                _qualityFactor = qualityFactor;
            }

            public int Compare(AcceptMediaType x, AcceptMediaType y)
                => -CompareForAscendingOrder(x, y);

            private static int CompareForAscendingOrder(AcceptMediaType x, AcceptMediaType y)
            {
                if (x._qualityFactor == y._qualityFactor)
                {
                    if (x.IsGenericType() && !y.IsGenericType())
                    {
                        return -1;
                    }
                    else if (!x.IsGenericType() && y.IsGenericType())
                    {
                        return 1;
                    }
                    else if (x.IsGenericSubType())
                    {
                        return (y.IsGenericSubType() ? x.CompareParameters(y) : -1);
                    }
                    else
                    {
                        return (y.IsGenericSubType() ? 1 : x.CompareParameters(y));
                    }
                }
                else
                {
                    return x._qualityFactor.CompareTo(y._qualityFactor);
                }
            }

            private int CompareParameters(AcceptMediaType other) => Parameters.Count.CompareTo(other.Parameters.Count);

            internal bool IsSameOrSuperTypeOf(ContentMediaType contentMediaType)
                => (IsGenericType() || string.Equals(MimeType, contentMediaType.MimeType))
                    && (IsGenericSubType() || string.Equals(MimeSubType, contentMediaType.MimeSubType));

            private bool IsGenericSubType() => string.Equals(MimeSubType, MIME_TYPE_WILDCARD);

            private bool IsGenericType() => string.Equals(MimeType, MIME_TYPE_WILDCARD);

            
        }
    }
}

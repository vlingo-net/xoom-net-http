// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Http.Resource;

namespace Vlingo.Http.Media
{
    public class ResponseMediaTypeSelector
    {
        private const char AcceptMediaTypeSeparator = ',';

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
            var acceptedContentTypeDescriptors = contentTypeList.Split(AcceptMediaTypeSeparator);

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

        internal class AcceptMediaType : MediaTypeDescriptor, IComparer<AcceptMediaType>, IComparable<AcceptMediaType>
        {
            private const string MimeTypeWildcard = "*";
            private const string QualityFactorParameter = "q";
            private const float DefaultQualityFactorValue = 1.0f;

            private readonly float _qualityFactor;

            public AcceptMediaType(string mimeType, string mimeSubType)
                : base(mimeType, mimeSubType)
            {
                _qualityFactor = DefaultQualityFactorValue;
            }

            public AcceptMediaType(string mimeType, string mimeSubType, IDictionary<string, string> parameters)
                : base(mimeType, mimeSubType, parameters)
            {
                var qualityFactor = DefaultQualityFactorValue;

                if (parameters.ContainsKey(QualityFactorParameter))
                {
                    if (float.TryParse(parameters[QualityFactorParameter], out var f))
                    {
                        qualityFactor = f;
                    }
                }

                _qualityFactor = qualityFactor;
            }

            public int Compare(AcceptMediaType x, AcceptMediaType y)
                => -CompareForAscendingOrder(x, y);
            
            public int CompareTo(AcceptMediaType other) => -CompareForAscendingOrder(this, other);

            private static int CompareForAscendingOrder(AcceptMediaType x, AcceptMediaType y)
            {
                if (x._qualityFactor == y._qualityFactor)
                {
                    if (x.IsGenericType() && !y.IsGenericType())
                    {
                        return -1;
                    }

                    if (!x.IsGenericType() && y.IsGenericType())
                    {
                        return 1;
                    }

                    if (x.IsGenericSubType())
                    {
                        return (y.IsGenericSubType() ? x.CompareParameters(y) : -1);
                    }

                    return (y.IsGenericSubType() ? 1 : x.CompareParameters(y));
                }

                return x._qualityFactor.CompareTo(y._qualityFactor);
            }

            private int CompareParameters(AcceptMediaType other) => Parameters.Count.CompareTo(other.Parameters.Count);

            internal bool IsSameOrSuperTypeOf(ContentMediaType contentMediaType)
                => (IsGenericType() || string.Equals(MimeType, contentMediaType.MimeType))
                    && (IsGenericSubType() || string.Equals(MimeSubType, contentMediaType.MimeSubType));

            private bool IsGenericSubType() => string.Equals(MimeSubType, MimeTypeWildcard);

            private bool IsGenericType() => string.Equals(MimeType, MimeTypeWildcard);
        }
    }
}
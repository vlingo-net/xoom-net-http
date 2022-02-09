// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections;
using System.Linq;

namespace Vlingo.Xoom.Http
{
    /// <summary>
    /// Contains the ordered list of content encodings that have been applied to a piece of
    /// content.  To reverse any such compression, these encodings should be applied in reverse order
    /// </summary>
    public class ContentEncoding
    {
        public ContentEncodingMethod[] EncodingMethods { get; }

        public ContentEncoding(params ContentEncodingMethod[] encodingMethods) => EncodingMethods = encodingMethods;

        public ContentEncoding() => EncodingMethods = new ContentEncodingMethod[0];

        public static ContentEncoding ParseFromHeader(string? headerValue)
        {
            if (!string.IsNullOrEmpty(headerValue))
            {
                var methods = headerValue?.Split(',');
                var parsedMethods = methods?.Select(ContentEncodingMethodHelper.Parse)
                    .Where(o => o.IsPresent)
                    .Select(o => o.Get())
                    .ToArray();

                if (parsedMethods != null) return new ContentEncoding(parsedMethods);
            }

            return new ContentEncoding();
        }
        
        public static ContentEncoding None() => new ContentEncoding();

        public override bool Equals(object? obj)
        {
            if (this == obj)
            {
                return true;
            }
            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            var that = (ContentEncoding) obj;
            return ((IStructuralEquatable)EncodingMethods).Equals(that.EncodingMethods, StructuralComparisons.StructuralEqualityComparer);
        }

        protected bool Equals(ContentEncoding other) => ((IStructuralEquatable)EncodingMethods).Equals(other.EncodingMethods, StructuralComparisons.StructuralEqualityComparer);

        public override int GetHashCode() => EncodingMethods.GetHashCode();
    }
}
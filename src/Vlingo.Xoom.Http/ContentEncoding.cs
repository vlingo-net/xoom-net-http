// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

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

        public static ContentEncoding ParseFromHeader(string headerValue)
        {
            var methods = headerValue.Split(',');
            var parsedMethods = methods.Select(ContentEncodingMethodHelper.Parse)
                .Where(o => o.IsPresent)
                .Select(o => o.Get())
                .ToArray();

            return new ContentEncoding(parsedMethods);
        }
        
        public static ContentEncoding None() => new ContentEncoding();
    }
}
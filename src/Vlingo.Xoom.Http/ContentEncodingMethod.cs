// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http
{
    public enum ContentEncodingMethod
    {
        Gzip,
        Compress,
        Deflate,
        Brotli
    }

    public static class ContentEncodingMethodHelper
    {
        public static Optional<ContentEncodingMethod> Parse(string value)
        {
            if (Enum.TryParse(value.ToLower().Trim() == "br" ? "brotli" : value.Trim(), true, out ContentEncodingMethod parsed))
            {
                return Optional.Of(parsed);
            }
            
            return Optional.Empty<ContentEncodingMethod>();
        }
    }
}
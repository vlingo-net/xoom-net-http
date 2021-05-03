// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Xoom.Http.Tests
{
    public class ContentEncodingMethodTest
    {
        [Fact]
        public void MethodParseReturnsMethod()
        {
            var method = "gzip";
            var result = ContentEncodingMethodHelper.Parse(method);
            Assert.True(result.IsPresent);
            Assert.Equal(ContentEncodingMethod.Gzip, result.Get());
        }

        [Fact]
        public void MethodParseReturnsEmpty()
        {
            var method = "jarjar";
            var result = ContentEncodingMethodHelper.Parse(method);
            Assert.False(result.IsPresent);
        }
    }
}
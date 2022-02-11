// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Xoom.Http.Tests;

public class ContentEncodingTest
{
    [Fact]
    public void CreateEncodingFrom()
    {
        var results = ContentEncoding.ParseFromHeader("gzip, br");
        ContentEncodingMethod[] expectedMethods =
        {
            ContentEncodingMethod.Gzip, ContentEncodingMethod.Brotli
        };
            
        Assert.Equal(expectedMethods, results.EncodingMethods);
    }

    [Fact]
    public void CreateEncodingSkipsUnkownEncoding()
    {
        var results = ContentEncoding.ParseFromHeader("gzip, br, foo");
        ContentEncodingMethod[] expectedMethods = {
            ContentEncodingMethod.Gzip, ContentEncodingMethod.Brotli
        };
            
        Assert.Equal(expectedMethods, results.EncodingMethods);
    }

    [Fact]
    public void CreateEncodingEmpty()
    {
        ContentEncoding results = ContentEncoding.ParseFromHeader("");
        ContentEncodingMethod[] expectedMethods = {};
        Assert.Equal(expectedMethods, results.EncodingMethods);
    }
}
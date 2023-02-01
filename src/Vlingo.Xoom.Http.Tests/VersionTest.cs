// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;

namespace Vlingo.Xoom.Http.Tests;

public class VersionTest
{
    [Fact]
    public void TestVersion1Dot1()
    {
        var version = Version.From("HTTP/1.1");

        Assert.True(version.IsHttp1_1());
        Assert.False(version.IsHttp2_0());
    }
        
    [Fact]
    public void TestVersion2Dot0()
    {
        var version = Version.From("HTTP/2.0");

        Assert.True(version.IsHttp2_0());
        Assert.False(version.IsHttp1_1());
    }
        
    [Fact]
    public void TestUnsupportedVersion0Dot1()
    {
        Assert.Throws<ArgumentException>(() => Version.From("HTTP/0.1"));
    }

    [Fact]
    public void TestUnsupportedVersion2Dot1()
    {
        Assert.Throws<ArgumentException>(() => Version.From("HTTP/2.1"));
    }

    [Fact]
    public void TestUnsupportedGarbage()
    {
        Assert.Throws<ArgumentException>(() => Version.From("Blah/Blah"));
    }
}
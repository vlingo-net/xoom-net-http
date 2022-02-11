// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Xoom.Http.Tests;

public class MethodExtensionsTest
{
    [Fact]
    public void TestPost()
    {
        var method = "POST".ToMethod();
        Assert.True(method.IsPost());

        Assert.False(method.IsConnect());
        Assert.False(method.IsDelete());
        Assert.False(method.IsGet());
        Assert.False(method.IsHead());
        Assert.False(method.IsOptions());
        Assert.False(method.IsPatch());
        Assert.False(method.IsPut());
        Assert.False(method.IsTrace());
    }

    [Fact]
    public void TestGet()
    {
        var method = "GET".ToMethod();
        Assert.True(method.IsGet());

        Assert.False(method.IsConnect());
        Assert.False(method.IsDelete());
        Assert.False(method.IsHead());
        Assert.False(method.IsOptions());
        Assert.False(method.IsPatch());
        Assert.False(method.IsPost());
        Assert.False(method.IsPut());
        Assert.False(method.IsTrace());
    }

    [Fact]
    public void TestPut()
    {
        var method = "PUT".ToMethod();
        Assert.True(method.IsPut());

        Assert.False(method.IsConnect());
        Assert.False(method.IsDelete());
        Assert.False(method.IsGet());
        Assert.False(method.IsHead());
        Assert.False(method.IsOptions());
        Assert.False(method.IsPatch());
        Assert.False(method.IsPost());
        Assert.False(method.IsTrace());
    }

    [Fact]
    public void TestPatch()
    {
        var method = "PATCH".ToMethod();
        Assert.True(method.IsPatch());

        Assert.False(method.IsConnect());
        Assert.False(method.IsDelete());
        Assert.False(method.IsGet());
        Assert.False(method.IsHead());
        Assert.False(method.IsOptions());
        Assert.False(method.IsPut());
        Assert.False(method.IsPost());
        Assert.False(method.IsTrace());
    }

    [Fact]
    public void TestDelete()
    {
        var method = "DELETE".ToMethod();
        Assert.True(method.IsDelete());

        Assert.False(method.IsConnect());
        Assert.False(method.IsGet());
        Assert.False(method.IsHead());
        Assert.False(method.IsOptions());
        Assert.False(method.IsPatch());
        Assert.False(method.IsPut());
        Assert.False(method.IsPost());
        Assert.False(method.IsTrace());
    }

    [Fact]
    public void TestHead()
    {
        var method = "HEAD".ToMethod();
        Assert.True(method.IsHead());

        Assert.False(method.IsConnect());
        Assert.False(method.IsDelete());
        Assert.False(method.IsGet());
        Assert.False(method.IsOptions());
        Assert.False(method.IsPatch());
        Assert.False(method.IsPut());
        Assert.False(method.IsPost());
        Assert.False(method.IsTrace());
    }

    [Fact]
    public void TestTrace()
    {
        var method = "TRACE".ToMethod();
        Assert.True(method.IsTrace());

        Assert.False(method.IsConnect());
        Assert.False(method.IsDelete());
        Assert.False(method.IsGet());
        Assert.False(method.IsHead());
        Assert.False(method.IsOptions());
        Assert.False(method.IsPatch());
        Assert.False(method.IsPut());
        Assert.False(method.IsPost());
    }

    [Fact]
    public void TestOptions()
    {
        var method = "OPTIONS".ToMethod();
        Assert.True(method.IsOptions());

        Assert.False(method.IsConnect());
        Assert.False(method.IsDelete());
        Assert.False(method.IsGet());
        Assert.False(method.IsHead());
        Assert.False(method.IsPatch());
        Assert.False(method.IsPut());
        Assert.False(method.IsPost());
        Assert.False(method.IsTrace());
    }

    [Fact]
    public void TestConnect()
    {
        var method = "CONNECT".ToMethod();
        Assert.True(method.IsConnect());

        Assert.False(method.IsDelete());
        Assert.False(method.IsGet());
        Assert.False(method.IsHead());
        Assert.False(method.IsOptions());
        Assert.False(method.IsPatch());
        Assert.False(method.IsPut());
        Assert.False(method.IsPost());
        Assert.False(method.IsTrace());
    }
}
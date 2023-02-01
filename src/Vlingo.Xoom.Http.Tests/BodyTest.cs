// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Text;
using Vlingo.Xoom.Wire.Channel;
using Xunit;

namespace Vlingo.Xoom.Http.Tests;

public class BodyTest
{
    private const string BinaryTextBodyText = "This is some text to render as bytes encoded in Base64. Phew!";

    [Fact]
    public void TestThatBodyHasContent()
    {
        var content = "{ text:\\\"some text\\\"}\"";

        var body = Body.From(content);

        Assert.NotNull(body);
        Assert.NotNull(body.Content);
        Assert.True(body.HasContent);
        Assert.Equal(content, body.Content);
    }

    [Fact]
    public void TestThatBodyHasNoContent()
    {
        var body = Body.Empty;

        Assert.NotNull(body);
        Assert.NotNull(body.Content);
        Assert.False(body.HasContent);
        Assert.Equal("", body.Content);
    }

    [Fact]
    public void TestThatByteArrayBodyEncodesAsBase64()
    {
        var bodyBytes = Encoding.UTF8.GetBytes(BinaryTextBodyText);
        var body = Body.From(bodyBytes);

        Assert.NotNull(body);
        Assert.NotNull(body.Content);
        Assert.True(body.HasContent);

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(body.Content));
        Assert.Equal(BinaryTextBodyText, decoded);
    }

    [Fact]
    public void TestThatFlippedMemoryStreamBodyEncodesAsBase64()
    {
        var buffer = new MemoryStream(1000);
        buffer.Write(Encoding.UTF8.GetBytes(BinaryTextBodyText));
        buffer.Flip();
        var body = Body.From(buffer);

        Assert.NotNull(body);
        Assert.NotNull(body.Content);
        Assert.True(body.HasContent);

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(body.Content));
        Assert.Equal(BinaryTextBodyText, decoded);
    }

    [Fact]
    public void TestThatNotFlippedMemoryStreamBodyEncodesAsBase64()
    {
        var buffer = new MemoryStream(1000);
        buffer.Write(Encoding.UTF8.GetBytes(BinaryTextBodyText));
        var body = Body.From(buffer);

        Assert.NotNull(body);
        Assert.NotNull(body.Content);
        Assert.True(body.HasContent);

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(body.Content));
        Assert.Equal(BinaryTextBodyText, decoded);
    }

    [Fact]
    public void TestThatByteArrayBodyEncodesAsUtf8()
    {
            
        var bodyBytes = Encoding.UTF8.GetBytes(BinaryTextBodyText);
        var body = Body.From(bodyBytes, Body.Encoding.UTF8);

        Assert.NotNull(body);
        Assert.NotNull(body.Content);
        Assert.True(body.HasContent);

        Assert.Equal(BinaryTextBodyText, body.Content);
    }

    [Fact]
    public void TestThatFlippedMemoryStreamBodyEncodesAsUtf8()
    {
        var buffer = new MemoryStream(1000);
        buffer.Write(Encoding.UTF8.GetBytes(BinaryTextBodyText));
        buffer.Flip();
        var body = Body.From(buffer, Body.Encoding.UTF8);

        Assert.NotNull(body);
        Assert.NotNull(body.Content);
        Assert.True(body.HasContent);

        Assert.Equal(BinaryTextBodyText, body.Content);
    }

    [Fact]
    public void TestThatNotFlippedMemoryStreamBodyEncodesAsUtf8()
    {
        var buffer = new MemoryStream(1000);
        buffer.Write(Encoding.UTF8.GetBytes(BinaryTextBodyText));
        var body = Body.From(buffer, Body.Encoding.UTF8);

        Assert.NotNull(body);
        Assert.NotNull(body.Content);
        Assert.True(body.HasContent);

        Assert.Equal(BinaryTextBodyText, body.Content);
    }

    [Fact]
    public void TestThatBytesEncodeAsExpectedUtf8()
    {
        sbyte[] pdfBytes = {37, 80, 68, 70, 45, 49, 46, 52, 10, 37, -30, -29, -49, -45, 10, 49};

        var body = Body.From((byte[]) (Array)pdfBytes, Body.Encoding.UTF8);

        Assert.NotNull(body);
        Assert.NotNull(body.Content);
        Assert.True(body.HasContent);
        Assert.StartsWith("%PDF", body.Content);
    }

    [Fact]
    public void TestThatBinaryBodyIsNotEncoded()
    {
        sbyte[] pdfBytes = {37, 80, 68, 70, 45, 49, 46, 52, 10, 37, -30, -29, -49, -45, 10, 49};

        var body = Body.From((byte[]) (Array)pdfBytes, Body.Encoding.None);

        Assert.NotNull(body);
        Assert.NotNull(body.Content);
        Assert.True(body.HasContent);
        Assert.Equal(body.BinaryContent, (byte[]) (Array)pdfBytes);
    }
}
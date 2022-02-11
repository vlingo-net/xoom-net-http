// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Xoom.Http.Tests;

public class ChunkedBodyTest
{
    private const string Chunk1 = "ABCDEFGHIJKLMNOPQRSTUVWYYZ0123";
    private const string Chunk2 = "abcdefghijklmnopqrstuvwxyz012345";

    [Fact]
    public void TestThatChunkedBodyChunks()
    {
        var body =
            Body
                .BeginChunked()
                .AppendChunk(Chunk1)
                .AppendChunk(Chunk2)
                .End();

        Assert.Contains(AsChunk(Chunk1), body.Content);
        Assert.Contains(AsChunk(Chunk2), body.Content);
    }
        
    private string AsChunk(string content) => $"{content.Length:x8}\r\n{content}\r\n";
}
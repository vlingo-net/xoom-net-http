// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Http.Resource;
using Vlingo.Http.Resource.Sse;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Http.Tests.Resource.Sse
{
    public class SseClientTest
    {
        private readonly SseClient _client;
        private readonly MockRequestResponseContext _context;
        
        [Fact(Skip = "In progress")]
        public void TestThatClientCloses()
        {
            var abandonSafely = _context.Channel.ExpectAbandon(1);

            _client.Close();

            Assert.Equal(1, abandonSafely.ReadFrom<int>("count"));
            Assert.Equal(1, _context.Channel.AbandonCount.Get());
        }

        public SseClientTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);

            Configuration.Define();
            _context = new MockRequestResponseContext(new MockResponseSenderChannel());
            _client = new SseClient(_context);
        }
    }
}
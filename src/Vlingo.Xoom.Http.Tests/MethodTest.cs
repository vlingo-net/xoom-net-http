// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Xoom.Http.Tests
{
    public class MethodTest
    {
        [Fact]
        public void TestPost()
        {
            var method = Method.From("POST");
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
            var method = Method.From("GET");
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
            var method = Method.From("PUT");
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
            var method = Method.From("PATCH");
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
            var method = Method.From("DELETE");
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
            var method = Method.From("HEAD");
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
            var method = Method.From("TRACE");
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
            var method = Method.From("OPTIONS");
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
            var method = Method.From("CONNECT");
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
}
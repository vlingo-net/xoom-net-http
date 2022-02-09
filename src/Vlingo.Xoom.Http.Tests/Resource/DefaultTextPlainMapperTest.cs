// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Http.Resource;
using Xunit;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class DefaultTextPlainMapperTest
    {
        [Fact]
        public void TestFromObjectToStringUsesToString()
        {
            var mapper = new DefaultTextPlainMapper();
            Assert.Equal("toStringResult", mapper.From(new ObjectForTest()));
        }

        [Fact]
        public void TestDeserializationToNonStringFails()
        {
            var mapper = new DefaultTextPlainMapper();
            Assert.Throws<InvalidOperationException>(() => mapper.From("some string", typeof(ObjectForTest)));
        }

        [Fact]
        public void TestDeserializationToStringSucceed()
        {
            var mapper = new DefaultTextPlainMapper();
            var canBeSerialized = mapper.From("some string", typeof(string));
            Assert.Equal("some string", canBeSerialized);
        }
    }
    
    internal class ObjectForTest
    {
        public override string ToString() => "toStringResult";
    }
}
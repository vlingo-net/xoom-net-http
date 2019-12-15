// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Linq;
using Xunit;

namespace Vlingo.Http.Tests
{
    public class QueryParametersParserTest
    {
        [Fact]
        public void TestParseNullQuery()
        {
            string query = null;
            var qp = new QueryParameters(query);
            Assert.Equal(0, qp.Names.Count);
        }

        [Fact]
        public void TestParseEmptyQuery()
        {
            var query = "";
            var qp = new QueryParameters(query);
            Assert.Equal(0, qp.Names.Count);
        }

        [Fact]
        public void TestParseSingleSimpleValuedParameterQuery()
        {
            var query = "color=red";
            var qp = new QueryParameters(query);
            Assert.Equal(1, qp.Names.Count);
            var name = qp.Names.First();
            Assert.Equal("color", name);
            var values = qp.ValuesOf(name);
            Assert.Equal(1, values.Count);
            var value = values[0];
            Assert.Equal("red", value);
        }

        [Fact]
        public void TestParseSingleListValuedParameterQuery()
        {
            var query = "colors=red,blue,green";
            var qp = new QueryParameters(query);
            Assert.Equal(1, qp.Names.Count);
            var name = qp.Names.First();
            Assert.Equal("colors", name);
            var values = qp.ValuesOf(name);
            Assert.Equal(1, values.Count);
            var value = values[0];
            Assert.Equal("red,blue,green", value);
        }

        [Fact]
        public void TestParseMultiParameterQuery()
        {
            var query = "color=red&size=medium";
            var qp = new QueryParameters(query);
            Assert.Equal(2, qp.Names.Count);

            var names = qp.Names.ToList();
            var name0 = names[0];
            Assert.Equal("color", name0);
            var values0 = qp.ValuesOf(name0);
            Assert.Equal(1, values0.Count);
            var value0 = values0[0];
            Assert.Equal("red", value0);

            var name1 = names[1];
            Assert.Equal("size", name1);
            var values1 = qp.ValuesOf(name1);
            Assert.Equal(1, values1.Count);
            var value1 = values1[0];
            Assert.Equal("medium", value1);
        }

        [Fact]
        public void TestParseParameterWithMultipleAmpersand()
        {
            var query = "color=red&&size=medium";
            var qp = new QueryParameters(query);
            Assert.Equal(2, qp.Names.Count);
            
            var names = qp.Names.ToList();
            var name0 = names[0];
            Assert.Equal("color", name0);
            var values0 = qp.ValuesOf(name0);
            Assert.Equal(1, values0.Count);
            var value0 = values0[0];
            Assert.Equal("red", value0);

            var name1 = names[1];
            Assert.Equal("size", name1);
            var values1 = qp.ValuesOf(name1);
            Assert.Equal(1, values1.Count);
            var value1 = values1[0];
            Assert.Equal("medium", value1);
        }

        [Fact]
        public void TestParseParametersWithAParamWithoutValue()
        {
            var query = "size";
            var qp = new QueryParameters(query);
            Assert.Equal(1, qp.Names.Count);
            
            var names = qp.Names.ToList();
            var name0 = names[0];
            Assert.Equal("size", name0);
            var values0 = qp.ValuesOf(name0);
            Assert.Equal(1, values0.Count);
            var value0 = values0[0];
            Assert.Null(value0);
        }

        [Fact]
        public void TestContainsKeyTrueCase()
        {
            var query = "size";
            var qp = new QueryParameters(query);

            Assert.True(qp.ContainsKey("size"));
        }

        [Fact]
        public void TestContainsKeyFalseCase()
        {
            var query = "size";
            var qp = new QueryParameters(query);

            Assert.False(qp.ContainsKey("color"));
        }
    }
}
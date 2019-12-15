// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Http.Tests
{
    public class RequestHeaderTest
    {
        [Fact]
        public void TestHeaderNameValue() {
            var header = RequestHeader.Of(RequestHeader.Accept, "text/plain");
    
            Assert.Equal(RequestHeader.Accept, header.Name);
            Assert.Equal("text/plain", header.Value);
        }
  
        [Fact]
        public void TestParseHeader() {
            var header = RequestHeader.FromString("Accept: text/plain");
    
            Assert.Equal(RequestHeader.Accept, header.Name);
            Assert.Equal("text/plain", header.Value);
        }
  
        [Fact]
        public void TestParseSpaceyHeader() {
            var header = RequestHeader.FromString("  Accept:    text/plain  ");
    
            Assert.Equal(RequestHeader.Accept, header.Name);
            Assert.Equal("text/plain", header.Value);
        }

        [Fact]
        public void TestParseLowerCaseContentLength() {
            var header = RequestHeader.FromString("content-length: 10");

            Assert.Equal(10, header.IfContentLength);
        }

        [Fact]
        public void TestEqualsCaseInsensitive() {
            var header1 = RequestHeader.FromString("Content-length: 10");
            var header2 = RequestHeader.FromString("content-length: 10");

            Assert.Equal(header1, header2);
        }

        [Fact]
        public void TestParseHeaderWithMultipleValueStrings() {
            var header = RequestHeader.FromString("Cookie: $Version=1; Skin=new;");
    
            Assert.Equal(RequestHeader.Cookie, header.Name);
            Assert.Equal("$Version=1; Skin=new;", header.Value);
        }
  
        [Fact]
        public void TestParseHeaderWithMultipleValueStringsAndColons() {
            var header = RequestHeader.FromString("Accept-Datetime: Thu, 31 May 2007 20:35:00 GMT");
    
            Assert.Equal(RequestHeader.AcceptDatetime, header.Name);
            Assert.Equal("Thu, 31 May 2007 20:35:00 GMT", header.Value);
        }
    }
}
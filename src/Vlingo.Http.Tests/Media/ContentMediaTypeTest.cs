// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Http.Media;
using Vlingo.Http.Resource;
using Xunit;

namespace Vlingo.Http.Tests.Media
{
    public class ContentMediaTypeTest
    {
        [Fact]
        public void WildCardsAreNotAllowed() {
            Assert.Throws<MediaTypeNotSupportedException>(() => new ContentMediaType("application", "*"));
        }
    }
}
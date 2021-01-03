// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Http.Media;
using Xunit;

namespace Vlingo.Http.Tests.Media
{
    public class ResponseContentMediaTypeSelectorTest
    {
        [Fact]
        public void Single_media_type_matches()
        {
            var specificTypeAccepted = "application/json";
            var selector = new ResponseMediaTypeSelector(specificTypeAccepted);
            var selected = selector.SelectType(new[]{ContentMediaType.Json});
            Assert.Equal(ContentMediaType.Json, selected);
        }
        
        [Fact]
        public void Wild_card_media_type_matches()
        {
            var xmlAndJsonSuperTypeAccepted = "application/*";
            var selector = new ResponseMediaTypeSelector(xmlAndJsonSuperTypeAccepted);
            var selected = selector.SelectType(new[]{ContentMediaType.Json});
            Assert.Equal(ContentMediaType.Json, selected);
        }
        
        [Fact]
        public void Generic_media_type_select_by_order_of_media_type()
        {
            var xmlAndJsonSuperTypeAccepted = "application/*";
            var selector = new ResponseMediaTypeSelector(xmlAndJsonSuperTypeAccepted);
            var selected = selector.SelectType(new[]{ContentMediaType.Xml, ContentMediaType.Json});
            Assert.Equal(ContentMediaType.Xml, selected);
        }
        
        [Fact]
        public void Specific_media_type_select_highest_ranked() {
            var jsonHigherPriorityXmlLowerPriorityAccepted = "application/xml;q=0.8, application/json";
            var selector = new ResponseMediaTypeSelector(jsonHigherPriorityXmlLowerPriorityAccepted);
            var selected = selector.SelectType(new[]{ContentMediaType.Xml, ContentMediaType.Json});
            Assert.Equal(ContentMediaType.Json, selected);
        }
    }
}
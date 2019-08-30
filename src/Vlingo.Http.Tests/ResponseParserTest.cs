// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Text;

namespace Vlingo.Http.Tests
{
    public class ResponseParserTest
    {
        private string MultipleResponseBuilder(int amount)
        {
            var builder = new StringBuilder();

            /*for (var idx = 1; idx <= amount; ++idx)
            {
                var body = (idx % 2 == 0) ? uniqueJaneDoe() : uniqueJohnDoe();
                uniqueBodies.add(body);
                builder.append(this.createdResponse(body));
            }*/

            return builder.ToString();
        }
    }
}
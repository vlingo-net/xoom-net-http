// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Http
{
    public sealed class PlainBody : Body
    {
        /// <summary>
        /// Construct my default state.
        /// </summary>
        public PlainBody()
        {
        }
        
        /// <summary>
        /// Construct my default state with the <paramref name="body"/> as content.
        /// </summary>
        /// <param name="body">the string body content</param>
        public PlainBody(string body) : base(body)
        {
        }
    }
}
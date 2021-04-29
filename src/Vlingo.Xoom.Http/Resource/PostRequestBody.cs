// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Http.Media;

namespace Vlingo.Xoom.Http.Resource
{
    public class PostRequestBody
    {
        private readonly Body _body;
        private readonly ContentMediaType _mediaType;

        public PostRequestBody(Body body, ContentMediaType mediaType)
        {
            _body = body;
            _mediaType = mediaType;
        }

        public override bool Equals(object? obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var that = (PostRequestBody) obj;
            return _body.Equals(that._body) && _mediaType.Equals(that._mediaType);
        }

        protected bool Equals(PostRequestBody other) => 
            Equals(_body, other._body) && Equals(_mediaType, other._mediaType);

        public override int GetHashCode() => 31 * _body.GetHashCode() + _mediaType.GetHashCode();
    }
}
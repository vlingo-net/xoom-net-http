// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Http.Media;

namespace Vlingo.Xoom.Http
{
    public class RequestData
    {
        private readonly Body _body;
        private readonly ContentMediaType _mediaType;
        private readonly ContentEncoding _contentEncoding;

        public RequestData(Body body, ContentMediaType mediaType, ContentEncoding contentEncoding)
        {
            _body = body;
            _mediaType = mediaType;
            _contentEncoding = contentEncoding;
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

            var that = (RequestData) obj;
            return _body.Equals(that._body) && _mediaType.Equals(that._mediaType);
        }

        protected bool Equals(RequestData other) => 
            Equals(_body, other._body) && Equals(_mediaType, other._mediaType);

        public override int GetHashCode() => 31 * _body.GetHashCode() + _mediaType.GetHashCode();
    }
}
// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
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
        public Body Body { get; }
        public ContentMediaType MediaType { get; }
        public ContentEncoding ContentEncoding { get; }

        public RequestData(Body body, ContentMediaType mediaType, ContentEncoding contentEncoding)
        {
            Body = body;
            MediaType = mediaType;
            ContentEncoding = contentEncoding;
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
            return Body.Equals(that.Body) && MediaType.Equals(that.MediaType);
        }

        protected bool Equals(RequestData other) => 
            Equals(Body, other.Body) && Equals(MediaType, other.MediaType);

        public override int GetHashCode() => 31 * Body.GetHashCode() + MediaType.GetHashCode();
    }
}
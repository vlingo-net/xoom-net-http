// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Http.Media;

namespace Vlingo.Http.Resource
{
    public class Content
    {
        public string Data { get; }
        public ContentMediaType ContentMediaType { get; }

        public Content(string data, ContentMediaType contentMediaType)
        {
            Data = data;
            ContentMediaType = contentMediaType;
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(this, o))
            {
                return true;
            }

            if (o == null || GetType() != o.GetType())
            {
                return false;
            }

            var content = (Content)o;

            return string.Equals(Data, content.Data) && Equals(ContentMediaType, content.ContentMediaType);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Data?.GetHashCode() ?? 0);
                hash = hash * 23 + (ContentMediaType?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}

// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Http
{
    public class ContentPacket
    {
        public string Content { get; }
        public int Utf8ExtraLength { get; }

        public ContentPacket(string content, int utf8ExtraLength)
        {
            Content = content;
            Utf8ExtraLength = utf8ExtraLength;
        }
    }
}
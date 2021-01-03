// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Http
{
    public sealed class BinaryBody : Body
    {
        private byte[] _binaryContent;
        
        public BinaryBody() => _binaryContent = new byte[0];

        public BinaryBody(byte[] body) => _binaryContent = body;

        public override byte[] BinaryContent => _binaryContent;

        public override string Content => BytesToBase64(_binaryContent);

        public override bool HasContent => _binaryContent.Length != 0;

        public override string ToString() => Content;
    }
}
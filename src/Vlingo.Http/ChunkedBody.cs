// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text;

namespace Vlingo.Http
{
    /// <summary>
    /// An HTTP response body that provides a multi-chunk format. You may create
    /// one chunk in each instance, or multiple chunks.
    /// </summary>
    public class ChunkedBody : Body
    {
        private readonly StringBuilder _content;
        
        /// <summary>
        /// Gets self after appending the <paramref name="body"/> as the chunk.
        /// </summary>
        /// <param name="body">the <see cref="Body"/> with content to append as the chunk</param>
        /// <returns><see cref="ChunkedBody"/></returns>
        public ChunkedBody AppendChunk(Body body) => AppendChunk(body.Content);

        /// <summary>
        /// Answer a new <see cref="Body"/> as a <see cref="PlainBody"/> with my content.
        /// </summary>
        /// <returns><see cref="ChunkedBody"/></returns>
        public Body AsPlainBody() => new PlainBody(Content);

        /// <summary>
        /// Gets self after appending the <paramref name="chunk"/>.
        /// </summary>
        /// <param name="chunk">The string content to append as the chunk</param>
        /// <returns><see cref="ChunkedBody"/></returns>
        public ChunkedBody AppendChunk(string chunk)
        {
            _content
                .Append(chunk.Length.ToString("x8"))
                .Append("\r\n")
                .Append(chunk)
                .Append("\r\n");

            return this;
        }

        /// <summary>
        /// Gets self after appending the <paramref name="chunk"/>.
        /// </summary>
        /// <param name="chunk">the <code>byte[]</code> content to append as the chunk</param>
        /// <returns><see cref="ChunkedBody"/></returns>
        public ChunkedBody AppendChunk(byte[] chunk) => throw new NotImplementedException("Adding chunks in the form of byte[] is not yet supported");

        /// <summary>
        /// Gets my content as string.
        /// </summary>
        public override string Content => ToString();

        public override byte[] BinaryContent => System.Text.Encoding.UTF8.GetBytes(ToString());

        public override bool IsComplex { get; } = true;

        /// <summary>
        /// Gets self after appending the end chunk, which is a length of 0.
        /// </summary>
        /// <returns><see cref="ChunkedBody"/></returns>
        public ChunkedBody End()
        {
            _content.Append(0).Append("\r\n");
            return this;
        }
        
        public override bool HasContent => _content.Length > 0;

        public override string ToString() => _content.ToString();

        /// <summary>
        /// Construct my default state.
        /// </summary>
        internal ChunkedBody() => _content = new StringBuilder();
    }
}
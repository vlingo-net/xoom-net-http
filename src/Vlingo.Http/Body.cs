// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Http
{
    /// <summary>
    /// An HTTP request/response body, with concrete subclasses <see cref="PlainBody"/> and <see cref="ChunkedBody"/>.
    /// </summary>
    public abstract class Body
    {
        private readonly string _content;
        
        public enum Encoding
        {
            Base64,
            UTF8,
            None
        }

        /// <summary>
        /// An empty <see cref="PlainBody"/>
        /// </summary>
        public static Body Empty { get; } = new PlainBody();
        
        /// <summary>
        /// Gets <see cref="ChunkedBody"/> prepared to receive chunks of content.
        /// </summary>
        /// <returns><see cref="ChunkedBody"/></returns>
        public static ChunkedBody BeginChunked() => new ChunkedBody();
        
        /// <summary>
        /// Gets a new <see cref="ChunkedBody"/> with <see cref="Body"/> content as the initial chunk.
        /// </summary>
        /// <param name="body">the <see cref="Body"/> content to add as a chunk</param>
        /// <returns><see cref="ChunkedBody"/></returns>
        public static ChunkedBody BeginChunkedWith(Body body) => BeginChunked().AppendChunk(body);
        
        /// <summary>
        /// Gets a new <see cref="ChunkedBody"/> with <see cref="Body"/> content as the initial chunk.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <returns><see cref="ChunkedBody"/></returns>
        public static ChunkedBody BeginChunkedWith(string content) => Body.BeginChunked().AppendChunk(content);

        /// <summary>
        /// Answer a new <code>Body</code> with binary content using <paramref name="encoding"/>.
        /// </summary>
        /// <param name="body">The byte[] content.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to use.</param>
        /// <returns></returns>
        public static Body From(byte[] body, Encoding encoding)
        {
            switch (encoding)
            {
                case Encoding.Base64:
                    return new PlainBody(BytesToBase64(body));
                case Encoding.UTF8:
                    return new PlainBody(BytesToUTF8(body));
                case Encoding.None:
                    return new BinaryBody(body);
            }
            
            throw new ArgumentException($"Unmapped encoding: {encoding}", nameof(encoding));
        }

        /// <summary>
        /// Answer a new <code>Body</code> with binary content encoded as a Base64 <code>string</code>.
        /// </summary>
        /// <param name="body">The byte[] content.</param>
        /// <returns></returns>
        public static Body From(byte[] body) => From(body, Encoding.Base64);

        /// <summary>
        /// Answer a new <code>Body</code> with text content, which is a <code>TextBody</code>.
        /// </summary>
        /// <param name="body">The string content.</param>
        /// <returns></returns>
        public static Body From(string body) => new PlainBody(body);

        /// <summary>
        /// Gets whether or not this <see cref="Body"/> content is complex.
        /// A <see cref="PlainBody"/> is not complex. A <see cref="ChunkedBody"/> is complex.
        /// </summary>
        public virtual bool IsComplex { get; } = false;

        /// <summary>
        /// Gets the content as a string
        /// </summary>
        public virtual string Content => _content;
        
        public virtual byte[] BinaryContent => System.Text.Encoding.UTF8.GetBytes(_content);

        public virtual bool HasContent => !string.IsNullOrEmpty(Content);

        public override string ToString() => Content;

        internal static string BytesToBase64(byte[] body)
            => Convert.ToBase64String(body);

        internal static string BytesToUTF8(byte[] body)
            => System.Text.Encoding.UTF8.GetString(body);

        protected Body(string body)
        {
            _content = body;
        }

        protected Body()
        {
            _content = string.Empty;
        }
    }
}

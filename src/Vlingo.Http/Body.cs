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
    /// An HTTP request/response body, with concrete subclasses <code>BinaryBody</code> and <code>TextBody</code>.
    /// </summary>
    public class Body
    {
        public enum Encoding
        {
            Base64,
            UTF8
        }

        /// <summary>
        /// An empty body.
        /// </summary>
        public static Body Empty { get; } = new Body();

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
                    return new Body(BytesToBase64(body));
                case Encoding.UTF8:
                    return new Body(BytesToUTF8(body));
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
        public static Body From(string body) => new Body(body);

        private static string BytesToBase64(byte[] body)
            => Convert.ToBase64String(body);

        private static string BytesToUTF8(byte[] body)
            => System.Text.Encoding.UTF8.GetString(body);

        public bool HasContent => !string.IsNullOrEmpty(Content);

        public override string ToString() => Content;

        internal string Content { get; }

        private Body(string body)
        {
            Content = body;
        }

        private Body()
        {
            Content = string.Empty;
        }
    }
}

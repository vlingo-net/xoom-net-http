// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Text;

namespace Vlingo.Xoom.Http
{
    /// <summary>
    /// An HTTP compliant content type.
    /// </summary>
    /// <remarks>
    /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Type
    /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types
    /// https://tools.ietf.org/html/rfc7231#section-3.1.1.1
    /// https://tools.ietf.org/html/rfc7231#section-3.1.1.5
    /// </remarks>
    public class ContentType
    {
        /// <summary>
        /// Answer a new <see cref="ContentType"/> with only a <paramref name="mediaType"/>.
        /// </summary>
        /// <param name="mediaType">The media type</param>
        /// <returns><see cref="ContentType"/></returns>
        public static ContentType Of(string mediaType) => new ContentType(mediaType, "", "");
        
        /// <summary>
        /// Answer a new <see cref="ContentType"/> with only a <paramref name="mediaType"/> and <paramref name="charset"/>.
        /// </summary>
        /// <param name="mediaType">The media type</param>
        /// <param name="charset">The character set</param>
        /// <returns><see cref="ContentType"/></returns>
        public static ContentType Of(string mediaType, string charset) => new ContentType(mediaType, charset, "");

        /// <summary>
        /// Answer a new <see cref="ContentType"/> with all of <paramref name="mediaType"/>, <paramref name="charset"/> and <paramref name="boundary"/>.
        /// </summary>
        /// <param name="mediaType">The media type</param>
        /// <param name="charset">The character set</param>
        /// <param name="boundary">The boundary</param>
        /// <returns><see cref="ContentType"/></returns>
        public static ContentType Of(string mediaType, string charset, string boundary) => new ContentType(mediaType, charset, boundary);
        
        /// <summary>
        /// Answer myself as <see cref="RequestHeader"/>.
        /// </summary>
        /// <returns><see cref="RequestHeader"/></returns>
        public RequestHeader ToRequestHeader() => RequestHeader.WithContentType(ToString());
        
        /// <summary>
        /// Answer myself as <see cref="ResponseHeader"/>.
        /// </summary>
        /// <returns><see cref="ResponseHeader"/></returns>
        public ResponseHeader ToResponseHeader() => ResponseHeader.WithContentType(ToString());

        public string MediaType { get; }
        public string Charset { get; }
        public string Boundary { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(MediaType))
            {
                builder.Append(MediaType);
                if (!string.IsNullOrEmpty(Charset))
                {
                    builder.Append("; ").Append(Charset);
                }
                if (!string.IsNullOrEmpty(Boundary))
                {
                    builder.Append("; ").Append(Boundary);
                }
            }
            return builder.ToString();
        }

        private ContentType(string mediaType, string charset, string boundary)
        {
            MediaType = mediaType;
            Charset = charset;
            Boundary = boundary;
        }
    }
}
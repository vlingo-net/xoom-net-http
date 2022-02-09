// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Http
{
    public class ResponseHeader : Header
    {
        public const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        public const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
        public const string AccessControlExposeHeaders = "Access-Control-Expose-Headers";
        public const string AccessControlMaxAge = "Access-Control-Max-Age";
        public const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        public const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        public const string AcceptPatch = "Accept-Patch";
        public const string AcceptRanges = "Accept-Ranges";
        public const string Age = "Age";
        public const string Allow = "Allow";
        public const string AltSvc = "Alt-Svc";
        public const string CacheControl = "Cache-Control";
        public const string Connection = "Connection";
        public const string ContentDisposition = "Content-Disposition";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentLanguage = "Content-Language";
        public const string ContentLength = "Content-Length";
        public const string ContentLocation = "Content-Location";
        public const string ContentMD5 = "Content-MD5";
        public const string ContentRange = "Content-Range";
        public const string ContentType = "Content-Type";
        public const string Date = "Date";
        public const string ETag = "ETag";
        public const string Expires = "Expires";
        public const string LastModified = "Last-Modified";
        public const string Link = "Link";
        public const string Location = "Location";
        public const string P3P = "P3P";
        public const string Pragma = "Pragma";
        public const string ProxyAuthenticate = "Proxy-Authenticate";
        public const string PublicKeyPins = "Public-Key-Pins";
        public const string RetryAfter = "Retry-After";
        public const string Server = "Server";
        public const string SetCookie = "Set-Cookie";
        public const string StrictTransportSecurity = "Strict-Transport-Security";
        public const string Trailer = "Trailer";
        public const string TransferEncoding = "Transfer-Encoding";
        public const string Tk = "Tk";
        public const string Upgrade = "Upgrade";
        public const string Vary = "Vary";
        public const string Via = "Via";
        public const string Warning = "Warning";
        public const string WWWAuthenticate = "WWW-Authenticate";
        public const string XFrameOptions = "X-Frame-Options";

        // Common non-standard response header names
        public const string ContentSecurityPolicy = "Content-Security-Policy";
        public const string XContentSecurityPolicy = "X-Content-Security-Policy";
        public const string XWebKitCSP = "X-WebKit-CSP";
        public const string Refresh = "Refresh";
        public const string Status = "Status";
        public const string TimingAllowOrigin = "Timing-Allow-Origin";
        public const string UpgradeInsecureRequests = "Upgrade-Insecure-Requests";
        public const string XContentDuration = "X-Content-Duration";
        public const string XContentTypeOptions = "X-Content-Type-Options";
        public const string XPoweredBy = "X-Powered-By";
        public const string XRequestID = "X-Request-ID";
        public const string XCorrelationID = "X-Correlation-ID";
        public const string XUACompatible = "X-UA-Compatible";
        public const string XXSSProtection = "X-XSS-Protection";

        public static ResponseHeader From(string textLine)
        {
            var colonIndex = textLine.IndexOf(':');

            if (colonIndex == -1)
            {
                throw new ArgumentException($"Not a header: {textLine}");
            }

            return new ResponseHeader(textLine.Substring(0, colonIndex).Trim(), textLine.Substring(colonIndex + 1).Trim());
        }

        public static Headers<ResponseHeader> WithHeaders(string name, string value) => Headers.Of(Of(name, value));

        public static Headers<ResponseHeader> WithHeaders(ResponseHeader header) => Headers.Of(header);

        public static ResponseHeader WithContentLength(int length) => new ResponseHeader(ContentLength, length.ToString());

        public static ResponseHeader WithContentLength(string body) => new ResponseHeader(ContentLength, body.Length.ToString());

        public static ResponseHeader WithContentLength(byte[] body) => new ResponseHeader(ContentLength, body.Length.ToString());

        public static ResponseHeader WithContentType(string type) => new ResponseHeader(ContentType, type);

        public static ResponseHeader WithCorrelationId(string? correlationId) => new ResponseHeader(XCorrelationID, correlationId);

        public static ResponseHeader Of(string name, string? value) => new ResponseHeader(name, value);

        public static ResponseHeader Of(string name, int value) => new ResponseHeader(name, value.ToString());

        public static ResponseHeader Of(string name, long value) => new ResponseHeader(name, value.ToString());

        public int IfContentLength =>
            string.Equals(Name, ContentLength, StringComparison.InvariantCultureIgnoreCase)
                ? Value == null ? -1 : int.Parse(Value)
                : -1;

        public bool IsKeepAliveConnection =>
            string.Equals(Name, Connection, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(Value, "keep-alive", StringComparison.InvariantCultureIgnoreCase);
        
        public bool IsStreamContentType =>
            string.Equals(Name, ContentType, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(Value, "-stream", StringComparison.InvariantCultureIgnoreCase);
        
        public bool IsTransferEncodingChunked =>
            string.Equals(Name, TransferEncoding, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(Value, "chunked", StringComparison.InvariantCultureIgnoreCase);

        private ResponseHeader(string name, string? value)
            : base(name, value)
        {
        }
    }
}

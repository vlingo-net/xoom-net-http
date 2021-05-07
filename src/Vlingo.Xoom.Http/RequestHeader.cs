// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.


using System;
using Vlingo.Xoom.Wire.Message;

namespace Vlingo.Xoom.Http
{
    public class RequestHeader : Header
    {
        public const string Accept = "Accept";
        public const string AcceptCharset = "Accept-Charset";
        public const string AcceptEncoding = "Accept-Encoding";
        public const string AcceptLanguage = "Accept-Language";
        public const string AcceptDatetime = "Accept-Datetime";
        public const string AccessControlRequestMethod = "Access-Control-Request-Method";
        public const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        public const string Authorization = "Authorization";
        public const string CacheControl = "Cache-Control";
        public const string Connection = "Connection";
        public const string Cookie = "Cookie";
        public const string ContentLength = "Content-Length";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentMD5 = "Content-MD5";
        public const string ContentType = "Content-Type";
        public const string Date = "Date";
        public const string Expect = "Expect";
        public const string Forwarded = "Forwarded";
        public const string From = "From";
        public const string Host = "Host";
        public const string IfMatch = "If-Match";
        public const string IfModifiedSince = "If-Modified-Since";
        public const string IfNoneMatch = "If-None-Match";
        public const string IfRange = "If-Range";
        public const string IfUnmodifiedSince = "If-Unmodified-Since";
        public const string LastEventID = "Last-Event-ID";
        public const string MaxForwards = "Max-Forwards";
        public const string Origin = "Origin";
        public const string Pragma = "Pragma";
        public const string ProxyAuthorization = "Proxy-Authorization";
        public const string Range = "Range";
        public const string Referer = "Referer";
        public const string TE = "TE";
        public const string UserAgent = "User-Agent";
        public const string Upgrade = "Upgrade";
        public const string Via = "Via";
        public const string Warning = "Warning";

        // Common non-standard request header names
        public const string XRequestedWith = "X-Requested-With";
        public const string DNT = "DNT";
        public const string XForwardedFor = "X-Forwarded-For";
        public const string XForwardedHost = "X-Forwarded-Host";
        public const string XForwardedProto = "X-Forwarded-Proto";
        public const string FrontEndHttps = "Front-End-Https";
        public const string XHttpMethodOverride = "X-Http-Method-Override";
        public const string XATTDeviceId = "X-ATT-DeviceId";
        public const string XWapProfile = "X-Wap-Profile";
        public const string ProxyConnection = "Proxy-Connection";
        public const string XUIDH = "X-UIDH";
        public const string XCsrfToken = "X-Csrf-Token";
        public const string XRequestID = "X-Request-ID";
        public const string XCorrelationID = "X-Correlation-ID";

        public static RequestHeader FromString(string textLine)
        {
            var colonIndex = textLine.IndexOf(":", StringComparison.InvariantCulture);

            if (colonIndex == -1)
            {
                throw new ArgumentException("Not a header: " + textLine);
            }

            return new RequestHeader(textLine.Substring(0, colonIndex).Trim(), textLine.Substring(colonIndex + 1).Trim());
        }

        public static RequestHeader WithAccept(string type) => new RequestHeader(Accept, type);

        public static RequestHeader WithCacheControl(string option) => new RequestHeader(CacheControl, option);

        public static RequestHeader WithConnection(string value) => new RequestHeader(Connection, value);
        
        public static RequestHeader WithContentLength(int length) => new RequestHeader(ContentLength, length.ToString());

        public static RequestHeader WithContentLength(string body) => new RequestHeader(ContentLength, Converters.EncodedLength(body).ToString());

        public static RequestHeader WithContentLength(byte[] body) => new RequestHeader(ContentLength, body.Length.ToString());

        public static RequestHeader WithContentType(string type) => new RequestHeader(ContentType, type);

        public static RequestHeader WithCorrelationId(string correlationId) => new RequestHeader(XCorrelationID, correlationId);

        public static RequestHeader WithContentEncoding(params string[] encodingMethods) =>
            encodingMethods.Length > 0 ?
                new RequestHeader(ContentEncoding, string.Join(",", encodingMethods)) :
                new RequestHeader(ContentEncoding, "");

        public static RequestHeader WithHost(string host) => new RequestHeader(Host, host);
        
        public static RequestHeader WithKeepAlive() => new RequestHeader(Connection, ValueKeepAlive);

        public static RequestHeader Of(string name, string value) => new RequestHeader(name, value);

        internal int IfContentLength
            => Name.Equals(ContentLength, StringComparison.InvariantCultureIgnoreCase)
                ? Value == null ? 0 : int.Parse(Value)
                : 0;

        private RequestHeader(string name, string value)
            : base(name, value)
        {
        }
    }
}

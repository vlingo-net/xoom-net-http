// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Http
{
    public class Request
    {
        public Body? Body { get; }
        public Headers<RequestHeader> Headers { get; }
        public Method? Method { get; }
        public Uri? Uri { get; }
        public Version? Version { get; }

        public static Request From(byte[] requestContent)
            => RequestParser.ParseFor(requestContent).FullRequest;

        // ===========================================
        // fluent API follows
        // ===========================================

        public static Request Has(Method method) => new Request(method);

        public Request And(Body body) => new Request(Method, Uri, Version, Headers, body);

        public Request And(RequestHeader header)
            => new Request(
                Method,
                Uri,
                Version,
                Http.Headers.Empty<RequestHeader>().And(Headers).And(header),
                Body);

        public Request And(Headers<RequestHeader> headers) => new Request(Method, Uri, Version, headers, Body);

        public Request And(Uri uri) => new Request(Method, uri, Version, Headers, Body);

        public Request And(Version version) => new Request(Method, Uri, version, Headers, Body);

        // ===========================================
        // less fluent API follows
        // ===========================================

        public static Request WithMethod(Method method) => new Request(method);

        public Request WithBody(string body) => new Request(Method, Uri, Version, Headers, Body.From(body));

        public Request WithHeader(string name, string value)
            => new Request(
                Method,
                Uri,
                Version,
                Http.Headers.Empty<RequestHeader>().And(Headers).And(RequestHeader.Of(name, value)),
                Body);

        public Request WithHeader(string name, int value) => WithHeader(name, value.ToString());

        public Request WithUri(string uri) => new Request(Method, new Uri(uri, UriKind.RelativeOrAbsolute), Version, Headers, Body);

        public Request WithVersion(string version) => new Request(Method, Uri, Version.From(version), Headers, Body);

        // ===========================================
        // instance
        // ===========================================

        public Header HeaderOf(string name) => Headers.HeaderOf(name);

        public string HeaderValueOr(string name, string defaultValue) => HeaderOf(name)?.Value ?? defaultValue;

        public QueryParameters QueryParameters => new QueryParameters(Uri.Query);

        public override string ToString()
            => $"{Method} {Uri} {Version}\n{Headers}\n{Body}";

        internal Request(Method? method, Uri? uri, Version? version, Headers<RequestHeader> headers, Body? body)
        {
            Method = method;
            Uri = uri;
            Version = version;
            Body = body;

            if(body != null && body.HasContent && headers.HeaderOf("Content-Length") == null)
            {
                Headers = headers.And("Content-Length", $"{body.Content.Length}");
            }
            else
            {
                Headers = headers;
            }
        }

        private Request(Method method)
            : this(method, new Uri("/", UriKind.Relative), Version.Http1_1, Http.Headers.Empty<RequestHeader>(), Body.Empty)
        { }
    }
}
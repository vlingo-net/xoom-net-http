// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Http
{
    public class Version
    {
        internal const string HTTP_1_1 = "HTTP/1.1";
        private const string HTTP_2_0 = "HTTP/2.0";

        private readonly string _version;
        private Version(string version)
        {
            _version = version;
        }

        public static Version Http1_1 { get; } = new Version(HTTP_1_1);

        public static Version Http2_0 { get; } = new Version(HTTP_2_0);

        public static Version From(string version)
        {
            if(string.Equals(version, HTTP_1_1, StringComparison.InvariantCultureIgnoreCase))
            {
                return Http1_1;
            }
            else if(string.Equals(version, HTTP_2_0, StringComparison.InvariantCultureIgnoreCase))
            {
                return Http2_0;
            }

            throw new ArgumentException($"Unsupported HTTP/version: {version}");
        }

        public bool IsHttp1_1() => string.Equals(_version, HTTP_1_1);

        public bool IsHttp2_0() => string.Equals(_version, HTTP_2_0);

        public override string ToString() => _version;
    }
}

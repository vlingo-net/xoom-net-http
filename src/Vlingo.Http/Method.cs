// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Http
{
    public class Method
    {
        public string Name { get; }

        private Method(string name)
        {
            Name = name;
        }

        public static Method CONNECT { get; } = new Method("CONNECT");
        public static Method DELETE { get; } = new Method("DELETE");
        public static Method GET { get; } = new Method("GET");
        public static Method HEAD { get; } = new Method("HEAD");
        public static Method OPTIONS { get; } = new Method("OPTIONS");
        public static Method PATCH { get; } = new Method("PATCH");
        public static Method POST { get; } = new Method("POST");
        public static Method PUT { get; } = new Method("PUT");
        public static Method TRACE { get; } = new Method("TRACE");

        public static Method From(string methodNameText)
        {
            var name = (methodNameText ?? string.Empty).ToUpperInvariant();
            switch (name)
            {
                case "CONNECT":
                    return CONNECT;
                case "DELETE":
                    return DELETE;
                case "GET":
                    return GET;
                case "HEAD":
                    return HEAD;
                case "OPTIONS":
                    return OPTIONS;
                case "PATCH":
                    return PATCH;
                case "POST":
                    return POST;
                case "PUT":
                    return PUT;
                case "TRACE":
                    return TRACE;
                default:
                    throw new ArgumentException($"{Response.ResponseStatus.MethodNotAllowed.GetDescription()}\n\n${methodNameText}");
            }
        }

        public bool IsCONNECT() => MethodEquals("CONNECT");
        public bool IsDELETE() => MethodEquals("DELETE");
        public bool IsGET() => MethodEquals("GET");
        public bool IsHEAD() => MethodEquals("HEAD");
        public bool IsOPTIONS() => MethodEquals("OPTIONS");
        public bool IsPATCH() => MethodEquals("PATCH");
        public bool IsPOST() => MethodEquals("POST");
        public bool IsPUT() => MethodEquals("PUT");
        public bool IsTRACE() => MethodEquals("TRACE");

        public override string ToString() => Name;

        private bool MethodEquals(string name) => string.Equals(Name, name);
    }
}

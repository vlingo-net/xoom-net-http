// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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

        public static Method Connect { get; } = new Method("CONNECT");
        public static Method Delete { get; } = new Method("DELETE");
        public static Method Get { get; } = new Method("GET");
        public static Method Head { get; } = new Method("HEAD");
        public static Method Options { get; } = new Method("OPTIONS");
        public static Method Patch { get; } = new Method("PATCH");
        public static Method Post { get; } = new Method("POST");
        public static Method Put { get; } = new Method("PUT");
        public static Method Trace { get; } = new Method("TRACE");

        public static Method From(string? methodNameText)
        {
            var name = (methodNameText ?? string.Empty).ToUpperInvariant();
            switch (name)
            {
                case "CONNECT":
                    return Connect;
                case "DELETE":
                    return Delete;
                case "GET":
                    return Get;
                case "HEAD":
                    return Head;
                case "OPTIONS":
                    return Options;
                case "PATCH":
                    return Patch;
                case "POST":
                    return Post;
                case "PUT":
                    return Put;
                case "TRACE":
                    return Trace;
                default:
                    throw new ArgumentException($"{Response.ResponseStatus.MethodNotAllowed.GetDescription()}\n\n${methodNameText}");
            }
        }

        public bool IsConnect() => MethodEquals("CONNECT");
        public bool IsDelete() => MethodEquals("DELETE");
        public bool IsGet() => MethodEquals("GET");
        public bool IsHead() => MethodEquals("HEAD");
        public bool IsOptions() => MethodEquals("OPTIONS");
        public bool IsPatch() => MethodEquals("PATCH");
        public bool IsPost() => MethodEquals("POST");
        public bool IsPut() => MethodEquals("PUT");
        public bool IsTrace() => MethodEquals("TRACE");

        public override string ToString() => Name;

        private bool MethodEquals(string name) => string.Equals(Name, name);
    }
}

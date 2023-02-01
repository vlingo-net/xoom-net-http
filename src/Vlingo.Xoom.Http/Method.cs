// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Http;

public enum Method
{
    Connect,
    Delete,
    Get,
    Head,
    Options,
    Patch,
    Post,
    Put,
    Trace
}
    
public static class MethodExtensions
{
    public static Method ToMethod(this string? methodNameText)
    {
        var name = (methodNameText ?? string.Empty).ToUpperInvariant();
        switch (name)
        {
            case "CONNECT":
                return Method.Connect;
            case "DELETE":
                return Method.Delete;
            case "GET":
                return Method.Get;
            case "HEAD":
                return Method.Head;
            case "OPTIONS":
                return Method.Options;
            case "PATCH":
                return Method.Patch;
            case "POST":
                return Method.Post;
            case "PUT":
                return Method.Put;
            case "TRACE":
                return Method.Trace;
            default:
                throw new ArgumentException($"{ResponseStatus.MethodNotAllowed.GetDescription()}\n\n${methodNameText}");
        }
    }

    public static bool IsConnect(this Method? method) => method == Method.Connect;
    public static bool IsConnect(this Method method) => method == Method.Connect;
    public static bool IsDelete(this Method? method) => method == Method.Delete;
    public static bool IsDelete(this Method method) => method == Method.Delete;
    public static bool IsGet(this Method? method) => method == Method.Get;
    public static bool IsGet(this Method method) => method == Method.Get;
    public static bool IsHead(this Method? method) => method == Method.Head;
    public static bool IsHead(this Method method) => method == Method.Head;
    public static bool IsOptions(this Method? method) => method == Method.Options;
    public static bool IsOptions(this Method method) => method == Method.Options;
    public static bool IsPatch(this Method? method) => method == Method.Patch;
    public static bool IsPatch(this Method method) => method == Method.Patch;
    public static bool IsPost(this Method? method) => method == Method.Post;
    public static bool IsPost(this Method method) => method == Method.Post;
    public static bool IsPut(this Method? method) => method == Method.Put;
    public static bool IsPut(this Method method) => method == Method.Put;
    public static bool IsTrace(this Method? method) => method == Method.Trace;
    public static bool IsTrace(this Method method) => method == Method.Trace;

    public static string Name(this Method method) => method.ToString().ToUpperInvariant();
    public static string? Name(this Method? method) => method?.ToString().ToUpperInvariant();
}
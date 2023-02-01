// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Http.Resource;

public class DefaultTextPlainMapper : IMapper
{
    public static IMapper Instance => new DefaultTextPlainMapper();
        
    public object? From(string? data, Type? type)
    {
        if (type == typeof(string))
        {
            return data;
        }

        throw new InvalidOperationException("Cannot deserialize text into type");
    }

    public string? From<T>(T data) => data?.ToString();
}
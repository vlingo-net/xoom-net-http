// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Http.Resource.Serialization;

namespace Vlingo.Http.Resource
{
    public class DefaultJsonMapper : IMapper
    {
        public static DefaultJsonMapper Instance { get; } = new DefaultJsonMapper();

        public object? From(string? data, Type type)
        {
            if(type == typeof(string))
            {
                return data;
            }

            return JsonSerialization.Deserialized(data, type);
        }

        public string From<T>(T data)
            => JsonSerialization.Serialized(data);
    }
}

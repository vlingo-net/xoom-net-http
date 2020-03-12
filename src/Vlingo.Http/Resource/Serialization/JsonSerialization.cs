// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Vlingo.Http.Resource.Serialization
{
    public static class JsonSerialization
    {
        public static T Deserialized<T>(string serialization)
            => JsonConvert.DeserializeObject<T>(serialization);
        
        public static T Deserialized<T>(string serialization, JsonSerializerSettings settings)
            => JsonConvert.DeserializeObject<T>(serialization, settings);

        public static object? Deserialized(string serialization, Type type)
            => JsonConvert.DeserializeObject(serialization, type);
        
        public static object? Deserialized(string serialization, Type type, JsonSerializerSettings settings)
            => JsonConvert.DeserializeObject(serialization, type, settings);

        public static List<T> DeserializedList<T>(string serialization)
            => JsonConvert.DeserializeObject<List<T>>(serialization);
        
        public static List<T>? DeserializedList<T>(string serialization, JsonSerializerSettings settings)
            => JsonConvert.DeserializeObject<List<T>>(serialization, settings);

        public static string Serialized<T>(T instance)
            => JsonConvert.SerializeObject(instance);
        
        public static string Serialized<T>(T instance, JsonSerializerSettings settings)
            => JsonConvert.SerializeObject(instance, settings);
    }
}

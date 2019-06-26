// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Vlingo.Http.Serialization
{
    public static class JsonSerialization
    {
        public static T Deserialized<T>(string serialization)
            => JsonConvert.DeserializeObject<T>(serialization);

        public static List<T> DeserializedList<T>(string serialization)
            => JsonConvert.DeserializeObject<List<T>>(serialization);

        public static string Serialized<T>(T instance)
            => JsonConvert.SerializeObject(instance);
    }
}

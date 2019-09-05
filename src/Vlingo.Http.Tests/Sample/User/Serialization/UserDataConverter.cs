// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vlingo.Http.Resource;

namespace Vlingo.Http.Tests.Sample.User.Serialization
{
    public class UserDataConverter : JsonConverter<UserData>
    {
        public override void WriteJson(JsonWriter writer, UserData value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override UserData ReadJson(JsonReader reader, Type objectType, UserData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Load the JSON for the Result into a JObject
            JObject jo = JObject.Load(reader);

            // Read the properties which will be used as constructor parameters

            // Construct the Result object using the non-default constructor
            

            // (If anything else needs to be populated on the result object, do that here)

            // Return the result
            return null;
        }
    }
}
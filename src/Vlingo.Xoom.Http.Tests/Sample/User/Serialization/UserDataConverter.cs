// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vlingo.Http.Tests.Sample.User.Serialization
{
    public class UserDataConverter : JsonConverter<UserData>
    {
        public override bool CanWrite { get; } = false;

        public override void WriteJson(JsonWriter writer, UserData value, JsonSerializer serializer)
        {
        }

        public override UserData ReadJson(JsonReader reader, Type objectType, UserData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var id = jo["Id"].ToObject<string>();
            var nameData = jo["NameData"].ToObject<NameData>();
            var contactData = jo["ContactData"].ToObject<ContactData>();
            
            return UserData.From(id, nameData, contactData);
        }
    }
}
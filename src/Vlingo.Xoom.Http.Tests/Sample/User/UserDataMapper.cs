// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Newtonsoft.Json;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Http.Tests.Sample.User.Serialization;

namespace Vlingo.Xoom.Http.Tests.Sample.User
{
    public class UserDataMapper : IMapper
    {
        private readonly JsonSerializerSettings _settings;

        public UserDataMapper()
        {
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new UserDataConverter());
        }
        
        public object From(string data, Type type) => JsonSerialization.Deserialized(data, type, _settings);

        public string From<T>(T data) => JsonSerialization.Serialized(data, _settings);
    }
}
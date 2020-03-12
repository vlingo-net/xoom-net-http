// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common.Serialization;
using Vlingo.Http.Resource;

namespace Vlingo.Http.Tests.Resource
{
    public class TestMapper : IMapper
    {
        public object From(string data, Type type) => JsonSerialization.Deserialized(data, type);

        public string From<T>(T data) => JsonSerialization.Serialized(data);
    }
}
// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public static class UserStateFactory
    {
        public static State NonExisting() => new State(null, null, null);

        public static State From(Name name, Contact contact) => new State(NextId(), name, contact);

        public static State From(string id, Name name, Contact contact) => new State(id, name, contact);

        public static void ResetId() => State.NextId.Set(0);

        public static string NextId()
        {
            var id = State.NextId.IncrementAndGet();
            return $"{id:D3}";
        }
    }
}
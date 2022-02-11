// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Http.Tests.Sample.User.Model;

public static class UserStateFactory
{
    public static UserState NonExisting() => new UserState(null, null, null);

    public static UserState From(Name name, Contact contact) => new UserState(NextId(), name, contact);

    public static UserState From(string id, Name name, Contact contact) => new UserState(id, name, contact);

    public static void ResetId() => UserState.NextId.Set(0);

    public static string NextId()
    {
        var id = UserState.NextId.IncrementAndGet();
        return $"{id:D3}";
    }
}
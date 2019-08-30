// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public interface IUser
    {
        ICompletes<State> WithContact(Contact contact);
        ICompletes<State> WithName(Name name);
        State NonExisting();
        State From(Name name, Contact contact);
        State From(string id, Name name, Contact contact);
        void ResetId();
        string NextId();
    }
}
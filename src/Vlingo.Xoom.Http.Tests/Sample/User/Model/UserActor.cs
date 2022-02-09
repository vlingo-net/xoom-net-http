// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http.Tests.Sample.User.Model
{
    public class UserActor : Actor, IUser
    {
        private UserState _userState;

        public UserActor(UserState userState) => _userState = userState;

        public ICompletes<UserState> WithContact(Contact contact)
        {
            _userState = _userState.WithContact(contact);
            return Completes().With(_userState);
        }

        public ICompletes<UserState> WithName(Name name)
        {
            _userState = _userState.WithName(name);
            return Completes().With(_userState);
        }
    }
}
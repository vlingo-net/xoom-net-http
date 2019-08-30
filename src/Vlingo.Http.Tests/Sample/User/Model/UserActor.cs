// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Common;

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public class UserActor : Actor, IUser
    {
        private State _state;

        public UserActor(State state)
        {
            _state = state;
        }
        
        public ICompletes<State> WithContact(Contact contact)
        {
            _state = _state.WithContact(contact);
            return Completes().With(_state);
        }

        public ICompletes<State> WithName(Name name)
        {
            _state = _state.WithName(name);
            return Completes().With(_state);
        }

        public State NonExisting() => UserStateFactory.NonExisting();

        public State From(Name name, Contact contact) => UserStateFactory.From(name, contact);

        public State From(string id, Name name, Contact contact) => UserStateFactory.From(id, name, contact);

        public void ResetId() => UserStateFactory.ResetId();

        public string NextId() => UserStateFactory.NextId();
    }
}
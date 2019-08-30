// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public class UserRepository
    {
        private static UserRepository _instance;
        
        private Dictionary<string, State> _users;
        
        private static volatile object _lockSync = new object();

        private UserRepository()
        {
            _users = new Dictionary<string, State>();
        }

        public static UserRepository Instance()
        {
            lock (_lockSync)
            {
                if (_instance == null)
                {
                    _instance = new UserRepository();
                }

                return _instance;
            }
        }

        public static void Reset() => _instance = null;
        
        public State UserOf(string userId)
        {
            var userState = _users[userId];
            return userState == null ? UserStateFactory.NonExisting() : userState;
        }

        public IEnumerable<State> Users => _users.Values;

        public void Save(State userState) => _users.Add(userState.Id, userState);
    }
}
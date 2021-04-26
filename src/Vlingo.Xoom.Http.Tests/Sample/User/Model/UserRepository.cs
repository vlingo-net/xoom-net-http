// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
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
        
        private readonly Dictionary<string, UserState> _users;
        
        private static volatile object _lockSync = new object();

        private UserRepository()
        {
            _users = new Dictionary<string, UserState>();
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
        
        public UserState UserOf(string userId)
        {
            var userState = _users[userId];
            return userState == null ? UserStateFactory.NonExisting() : userState;
        }

        public IEnumerable<UserState> Users => _users.Values;

        public void Save(UserState userState) => _users[userState.Id] = userState;
    }
}
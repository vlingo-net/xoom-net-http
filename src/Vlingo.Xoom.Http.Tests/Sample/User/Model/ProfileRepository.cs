// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Xoom.Http.Tests.Sample.User.Model
{
    public class ProfileRepository
    {
        private static ProfileRepository _instance;

        private readonly Dictionary<string, ProfileState> _profiles;
        
        private static volatile object _lockSync = new object();
        
        public static ProfileRepository Instance()
        {
            lock (_lockSync)
            {
                if (_instance == null)
                {
                    _instance = new ProfileRepository();
                }

                return _instance;
            }
        }

        public ProfileState ProfileOf(string userId)
        {
            var profileState = _profiles[userId];
    
            return profileState == null ? ProfileStateFactory.NonExisting() : profileState;
        }
        
        public void Save(ProfileState profileState) => _profiles.Add(profileState.Id, profileState);

        private ProfileRepository() => _profiles = new Dictionary<string, ProfileState>();
    }
}
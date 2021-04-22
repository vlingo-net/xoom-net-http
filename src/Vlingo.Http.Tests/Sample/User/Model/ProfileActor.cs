// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public class ProfileActor : Actor, IProfile
    {
        private ProfileState _state;

        public ProfileActor(ProfileState profileState) => _state = profileState;

        public ICompletes<ProfileState> WithTwitterAccount(string twitterAccount)
        {
            _state = _state.WithTwitterAccount(twitterAccount);
            return Completes().With(_state);
        }

        public ICompletes<ProfileState> WithLinkedInAccount(string linkedInAccount)
        {
            _state = _state.WithLinkedInAccount(linkedInAccount);
            return Completes().With(_state);
        }

        public ICompletes<ProfileState> WithWebSite(string website)
        {
            _state = _state.WithWebSite(website);
            return Completes().With(_state);
        }
    }
}
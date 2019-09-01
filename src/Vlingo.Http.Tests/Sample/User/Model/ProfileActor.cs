// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Common;

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public class ProfileActor : Actor, IProfile
    {
        private ProfileState _state;
        
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

        public ProfileState From(string id, string twitterAccount, string linkedInAccount, string website) =>
            ProfileStateFactory.From(id, twitterAccount, linkedInAccount, website);
        
        public ProfileState NonExisting() => ProfileStateFactory.NonExisting();
    }
}
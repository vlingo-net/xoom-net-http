// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public class ProfileState
    {
        public string Id { get; }
        public string LinkedInAccount { get; }
        public string TwitterAccount { get; }
        public string Website { get; }

        public bool DoesNotExist => Id == null;

        public ProfileState WithTwitterAccount(string twitterAccount) => new ProfileState(Id, twitterAccount, LinkedInAccount, Website);

        public ProfileState WithLinkedInAccount(string linkedInAccount) => new ProfileState(Id, TwitterAccount, linkedInAccount, Website);

        public ProfileState WithWebSite(string website) => new ProfileState(Id, TwitterAccount, LinkedInAccount, website);

        public ProfileState(string id, string twitterAccount, string linkedInAccount, string website)
        {
            Id = id;
            TwitterAccount = twitterAccount;
            LinkedInAccount = linkedInAccount;
            Website = website;
        }
    }
}
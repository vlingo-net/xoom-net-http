// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Http.Tests.Sample.User.Model;

namespace Vlingo.Xoom.Http.Tests.Sample.User
{
    public class ProfileData
    {
        public string LinkedInAccount { get; }
        public string TwitterAccount { get; }
        public string Website { get; }

        public static ProfileData From(ProfileState profile) => new ProfileData(profile.TwitterAccount, profile.LinkedInAccount, profile.Website);

        public ProfileData(string twitterAccount, string linkedInAccount, string website)
        {
            TwitterAccount = twitterAccount;
            LinkedInAccount = linkedInAccount;
            Website = website;
        }
    }
}
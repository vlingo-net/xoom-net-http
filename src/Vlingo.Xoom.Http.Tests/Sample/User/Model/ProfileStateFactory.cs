// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Http.Tests.Sample.User.Model
{
    public static class ProfileStateFactory
    {
        public static ProfileState From(string id, string twitterAccount, string linkedInAccount, string website) => 
            new ProfileState(id, twitterAccount, linkedInAccount, website);

        public static ProfileState NonExisting() => new ProfileState(null, null, null, null);
    }
}
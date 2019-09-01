// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public interface IProfile
    {
        ICompletes<ProfileState> WithTwitterAccount(string twitterAccount);
        ICompletes<ProfileState> WithLinkedInAccount(string linkedInAccount);
        ICompletes<ProfileState> WithWebSite(string website);
        ProfileState From(string id, string twitterAccount, string linkedInAccount, string website);
        ProfileState NonExisting();
    }
}
// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Http.Tests.Sample.User.Model
{
    public class Name
    {
        public string Given { get; }
        public string Family { get; }

        public static Name From(string given, string family) => new Name(given, family);
  
        public Name(string given, string family)
        {
            Given = given;
            Family = family;
        }

        public override string ToString() => $"Name[given={Given}, family={Family}]";
    }
}
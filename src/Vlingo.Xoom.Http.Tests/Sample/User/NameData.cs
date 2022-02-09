// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Http.Tests.Sample.User.Model;

namespace Vlingo.Xoom.Http.Tests.Sample.User
{
    public class NameData
    {
        public string Given { get; }
        public string Family { get; }

        public static NameData From(string given, string family) => new NameData(given, family);

        public static NameData From(Name name) => new NameData(name.Given, name.Family);
  
        public NameData(string given, string family)
        {
            Given = given;
            Family = family;
        }

        public override string ToString() => $"NameData[given={Given}, family={Family}]";

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (!(obj is NameData)) return false;
            var nameData = (NameData) obj;
            return Given.Equals(nameData.Given) && Family.Equals(nameData.Family);
        }

        public override int GetHashCode() => 13 * Given.GetHashCode() + Family.GetHashCode();
    }
}
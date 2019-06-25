// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Http
{
    public class Header
    {
        protected Header(string name, string value)
        {
            Name = name;
            Value = value;
        }

        internal string Name { get; }
        internal string Value { get; }

        public bool MatchesName(Header header)
            => string.Equals(Name, header.Name, StringComparison.InvariantCultureIgnoreCase);

        public override bool Equals(object obj)
        {
            var other = obj as Header;
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }

            return string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(Value, other.Value);
        }

        public override int GetHashCode()
            => 13 * Name.GetHashCode() + Value.GetHashCode();

        public override string ToString()
             => $"{Name}: {Value}";
    }
}

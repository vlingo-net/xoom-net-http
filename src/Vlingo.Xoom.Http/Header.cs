// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
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
        public static string ValueWildcardAny { get; } = "*";
        public static string ValueBr { get; } = "br";
        public static string ValueClose { get; } = "close";
        public static string ValueCompress { get; } = "compress";
        public static string ValueDeflate { get; } = "deflate";
        public static string ValueGZip { get; } = "gzip";
        public static string ValueIdentity { get; } = "identity";
        public static string ValueISO_8859_15 { get; } = "iso-8859-15";
        public static string ValueKeepAlive { get; } = "keep-alive";
        public static string ValueUTF_8 { get; } = "utf-8";
            
        protected Header(string name, string? value)
        {
            Name = name;
            Value = value;
        }

        internal string Name { get; }
        internal string? Value { get; }

        public bool MatchesNameOf(Header header)
            => string.Equals(Name, header.Name, StringComparison.InvariantCultureIgnoreCase);
        
        public bool MatchesNameOf(string name)
            => string.Equals(Name, name, StringComparison.InvariantCultureIgnoreCase);
        
        public bool MatchesValueOf(Header header)
            => string.Equals(Value, header.Value, StringComparison.InvariantCultureIgnoreCase);
        
        public bool MatchesValueOf(string value)
            => string.Equals(Value, value, StringComparison.InvariantCultureIgnoreCase);

        public override bool Equals(object? obj)
        {
            var other = obj as Header;
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }

            return MatchesNameOf(other) && MatchesValueOf(other);
        }

        public override int GetHashCode()
            => 13 * Name.GetHashCode() + (Value != null ? Value.GetHashCode() : 0);

        public override string ToString()
             => $"{Name}: {Value}";
    }
}

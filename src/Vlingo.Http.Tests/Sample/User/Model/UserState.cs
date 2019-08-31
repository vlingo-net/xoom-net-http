// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public class UserState
    {
        public static AtomicInteger NextId { get; } = new AtomicInteger(0);

        public string Id { get; }
        public Name Name { get; }
        public Contact Contact { get; }
    
        public bool DoesNotExist()
        {
            return Id == null;
        }

        public UserState WithContact(Contact contact) => new UserState(Id, Name, contact);

        public UserState WithName(Name name) => new UserState(Id, name, Contact);

        public override string ToString() => $"User.State[id={Id}, name={Name}, contact={Contact}]";

        public UserState(string id, Name name, Contact contact)
        {
            Id = id;
            Name = name;
            Contact = contact;
        }
    }
}
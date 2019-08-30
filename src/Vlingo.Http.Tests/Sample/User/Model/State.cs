// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public class State {
        public static AtomicInteger NextId { get; } = new AtomicInteger(0);

        public string Id { get; }
        public Name Name { get; }
        public Contact Contact { get; }
    
        public bool DoesNotExist()
        {
            return Id == null;
        }

        public State WithContact(Contact contact) => new State(Id, Name, contact);

        public State WithName(Name name) => new State(Id, name, Contact);

        public override string ToString() => $"User.State[id={Id}, name={Name}, contact={Contact}]";

        public State(string id, Name name, Contact contact) {
            Id = id;
            Name = name;
            Contact = contact;
        }
    }
}
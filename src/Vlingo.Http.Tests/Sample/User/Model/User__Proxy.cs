// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public class User__Proxy : IUser
    {
        private const string WithContactRepresentation1 = "WithContact(Vlingo.Http.Tests.Sample.User.Model.Contact)";
        private const string WithNameRepresentation2 = "WithName(Vlingo.Http.Tests.Sample.User.Model.Name)";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public User__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public ICompletes<UserState> WithContact(
            Contact contact)
        {
            if (!_actor.IsStopped)
            {
                Action<IUser> cons128873 = __ => __.WithContact(contact);
                var completes = new BasicCompletes<UserState>(_actor.Scheduler);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, completes, WithContactRepresentation1);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IUser>(_actor,
                        cons128873, completes, WithContactRepresentation1));
                }

                return completes;
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, WithContactRepresentation1));
            }

            return null;
        }

        public ICompletes<UserState> WithName(
            Name name)
        {
            if (!_actor.IsStopped)
            {
                Action<IUser> cons128873 = __ => __.WithName(name);
                var completes = new BasicCompletes<UserState>(_actor.Scheduler);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, completes, WithNameRepresentation2);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IUser>(_actor,
                        cons128873, completes, WithNameRepresentation2));
                }

                return completes;
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, WithNameRepresentation2));
            }

            return null;
        }
    }
}
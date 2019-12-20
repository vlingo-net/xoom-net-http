using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vlingo.Actors;
using Vlingo.Common;

namespace Vlingo.Http.Tests.Sample.User.Model
{
    public class User__Proxy : Vlingo.Http.Tests.Sample.User.Model.IUser
    {
        private const string WithContactRepresentation1 = "WithContact(Vlingo.Http.Tests.Sample.User.Model.Contact)";
        private const string WithNameRepresentation2 = "WithName(Vlingo.Http.Tests.Sample.User.Model.Name)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public User__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public Vlingo.Common.ICompletes<Vlingo.Http.Tests.Sample.User.Model.UserState> WithContact(
            Vlingo.Http.Tests.Sample.User.Model.Contact contact)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Tests.Sample.User.Model.IUser> cons128873 = __ => __.WithContact(contact);
                var completes = new BasicCompletes<Vlingo.Http.Tests.Sample.User.Model.UserState>(this.actor.Scheduler);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, completes, WithContactRepresentation1);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Http.Tests.Sample.User.Model.IUser>(this.actor,
                        cons128873, completes, WithContactRepresentation1));
                }

                return completes;
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, WithContactRepresentation1));
            }

            return null;
        }

        public Vlingo.Common.ICompletes<Vlingo.Http.Tests.Sample.User.Model.UserState> WithName(
            Vlingo.Http.Tests.Sample.User.Model.Name name)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Tests.Sample.User.Model.IUser> cons128873 = __ => __.WithName(name);
                var completes = new BasicCompletes<Vlingo.Http.Tests.Sample.User.Model.UserState>(this.actor.Scheduler);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, completes, WithNameRepresentation2);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Http.Tests.Sample.User.Model.IUser>(this.actor,
                        cons128873, completes, WithNameRepresentation2));
                }

                return completes;
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, WithNameRepresentation2));
            }

            return null;
        }
    }
}
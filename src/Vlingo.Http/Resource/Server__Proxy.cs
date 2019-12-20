using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vlingo.Actors;
using Vlingo.Common;

namespace Vlingo.Http.Resource
{
    public class Server__Proxy : Vlingo.Http.Resource.IServer
    {
        private const string StartWithRepresentation1 = "StartWith(Vlingo.Actors.Stage)";

        private const string StartWithRepresentation2 =
            "StartWith(Vlingo.Actors.Stage, Vlingo.Http.Resource.HttpProperties)";

        private const string StartWithRepresentation3 =
            "StartWith(Vlingo.Actors.Stage, Vlingo.Http.Resource.Resources, int, Vlingo.Http.Resource.Configuration.SizingConf, Vlingo.Http.Resource.Configuration.TimingConf)";

        private const string StartWithRepresentation4 =
            "StartWith(Vlingo.Actors.Stage, Vlingo.Http.Resource.Resources, Vlingo.Http.Filters, int, Vlingo.Http.Resource.Configuration.SizingConf, Vlingo.Http.Resource.Configuration.TimingConf)";

        private const string StartWithRepresentation5 =
            "StartWith(Vlingo.Actors.Stage, Vlingo.Http.Resource.Resources, Vlingo.Http.Filters, int, Vlingo.Http.Resource.Configuration.SizingConf, Vlingo.Http.Resource.Configuration.TimingConf, string, string)";

        private const string ShutDownRepresentation6 = "ShutDown()";
        private const string StartUpRepresentation7 = "StartUp()";
        private const string ConcludeRepresentation8 = "Conclude()";
        private const string StopRepresentation9 = "Stop()";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public Server__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public bool IsStopped => false;

        public Vlingo.Http.Resource.IServer StartWith(Vlingo.Actors.Stage stage)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Resource.IServer> cons128873 = __ => __.StartWith(stage);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, null, StartWithRepresentation1);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Http.Resource.IServer>(this.actor, cons128873,
                        StartWithRepresentation1));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, StartWithRepresentation1));
            }

            return null!;
        }

        public Vlingo.Http.Resource.IServer StartWith(Vlingo.Actors.Stage stage,
            Vlingo.Http.Resource.HttpProperties properties)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Resource.IServer> cons128873 = __ => __.StartWith(stage, properties);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, null, StartWithRepresentation2);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Http.Resource.IServer>(this.actor, cons128873,
                        StartWithRepresentation2));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, StartWithRepresentation2));
            }

            return null!;
        }

        public Vlingo.Http.Resource.IServer StartWith(Vlingo.Actors.Stage stage,
            Vlingo.Http.Resource.Resources resources, int port, Vlingo.Http.Resource.Configuration.SizingConf sizing,
            Vlingo.Http.Resource.Configuration.TimingConf timing)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Resource.IServer> cons128873 = __ =>
                    __.StartWith(stage, resources, port, sizing, timing);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, null, StartWithRepresentation3);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Http.Resource.IServer>(this.actor, cons128873,
                        StartWithRepresentation3));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, StartWithRepresentation3));
            }

            return null!;
        }

        public Vlingo.Http.Resource.IServer StartWith(Vlingo.Actors.Stage stage,
            Vlingo.Http.Resource.Resources resources, Vlingo.Http.Filters filters, int port,
            Vlingo.Http.Resource.Configuration.SizingConf sizing, Vlingo.Http.Resource.Configuration.TimingConf timing)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Resource.IServer> cons128873 = __ =>
                    __.StartWith(stage, resources, filters, port, sizing, timing);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, null, StartWithRepresentation4);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Http.Resource.IServer>(this.actor, cons128873,
                        StartWithRepresentation4));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, StartWithRepresentation4));
            }

            return null!;
        }

        public Vlingo.Http.Resource.IServer StartWith(Vlingo.Actors.Stage stage,
            Vlingo.Http.Resource.Resources resources, Vlingo.Http.Filters filters, int port,
            Vlingo.Http.Resource.Configuration.SizingConf sizing, Vlingo.Http.Resource.Configuration.TimingConf timing,
            string severMailboxTypeName, string channelMailboxTypeName)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Resource.IServer> cons128873 = __ => __.StartWith(stage, resources, filters, port,
                    sizing, timing, severMailboxTypeName, channelMailboxTypeName);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, null, StartWithRepresentation5);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Http.Resource.IServer>(this.actor, cons128873,
                        StartWithRepresentation5));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, StartWithRepresentation5));
            }

            return null!;
        }

        public Vlingo.Common.ICompletes<bool> ShutDown()
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Resource.IServer> cons128873 = __ => __.ShutDown();
                var completes = new BasicCompletes<bool>(this.actor.Scheduler);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, completes, ShutDownRepresentation6);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Http.Resource.IServer>(this.actor, cons128873, completes,
                        ShutDownRepresentation6));
                }

                return completes;
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, ShutDownRepresentation6));
            }

            return null!;
        }

        public Vlingo.Common.ICompletes<bool> StartUp()
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Resource.IServer> cons128873 = __ => __.StartUp();
                var completes = new BasicCompletes<bool>(this.actor.Scheduler);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, completes, StartUpRepresentation7);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Http.Resource.IServer>(this.actor, cons128873, completes,
                        StartUpRepresentation7));
                }

                return completes;
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, StartUpRepresentation7));
            }

            return null!;
        }

        public void Conclude()
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Resource.IServer> cons128873 = __ => __.Conclude();
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, null, ConcludeRepresentation8);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Http.Resource.IServer>(this.actor, cons128873,
                        ConcludeRepresentation8));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, ConcludeRepresentation8));
            }
        }

        public void Stop()
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Resource.IServer> cons128873 = __ => __.Stop();
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, null, StopRepresentation9);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Http.Resource.IServer>(this.actor, cons128873, StopRepresentation9));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, StopRepresentation9));
            }
        }
    }
}
// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Http.Resource
{
    public class Server__Proxy : IServer
    {
        private const string StartWithRepresentation1 = "StartWith(Vlingo.Xoom.Actors.Stage)";

        private const string StartWithRepresentation2 =
            "StartWith(Vlingo.Xoom.Actors.Stage, Vlingo.Http.Resource.HttpProperties)";

        private const string StartWithRepresentation3 =
            "StartWith(Vlingo.Xoom.Actors.Stage, Vlingo.Http.Resource.Resources, int, Vlingo.Http.Resource.Configuration.SizingConf, Vlingo.Http.Resource.Configuration.TimingConf)";

        private const string StartWithRepresentation4 =
            "StartWith(Vlingo.Xoom.Actors.Stage, Vlingo.Http.Resource.Resources, Vlingo.Http.Filters, int, Vlingo.Http.Resource.Configuration.SizingConf, Vlingo.Http.Resource.Configuration.TimingConf)";

        private const string StartWithRepresentation5 =
            "StartWith(Vlingo.Xoom.Actors.Stage, Vlingo.Http.Resource.Resources, Vlingo.Http.Filters, int, Vlingo.Http.Resource.Configuration.SizingConf, Vlingo.Http.Resource.Configuration.TimingConf, string, string)";

        private const string ShutDownRepresentation6 = "ShutDown()";
        private const string StartUpRepresentation7 = "StartUp()";
        private const string ConcludeRepresentation8 = "Conclude()";
        private const string StopRepresentation9 = "Stop()";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public Server__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public bool IsStopped => false;

        public IServer StartWith(Stage stage)
        {
            if (!_actor.IsStopped)
            {
                Action<IServer> cons128873 = __ => __.StartWith(stage);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, null, StartWithRepresentation1);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IServer>(_actor, cons128873,
                        StartWithRepresentation1));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, StartWithRepresentation1));
            }

            return null!;
        }

        public IServer StartWith(Stage stage,
            HttpProperties properties)
        {
            if (!_actor.IsStopped)
            {
                Action<IServer> cons128873 = __ => __.StartWith(stage, properties);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, null, StartWithRepresentation2);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IServer>(_actor, cons128873,
                        StartWithRepresentation2));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, StartWithRepresentation2));
            }

            return null!;
        }

        public IServer StartWith(Stage stage,
            Resources resources, int port, Configuration.SizingConf sizing,
            Configuration.TimingConf timing)
        {
            if (!_actor.IsStopped)
            {
                Action<IServer> cons128873 = __ =>
                    __.StartWith(stage, resources, port, sizing, timing);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, null, StartWithRepresentation3);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IServer>(_actor, cons128873,
                        StartWithRepresentation3));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, StartWithRepresentation3));
            }

            return null!;
        }

        public IServer StartWith(Stage stage,
            Resources resources, Filters filters, int port,
            Configuration.SizingConf sizing, Configuration.TimingConf timing)
        {
            if (!_actor.IsStopped)
            {
                Action<IServer> cons128873 = __ =>
                    __.StartWith(stage, resources, filters, port, sizing, timing);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, null, StartWithRepresentation4);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IServer>(_actor, cons128873,
                        StartWithRepresentation4));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, StartWithRepresentation4));
            }

            return null!;
        }

        public IServer StartWith(Stage stage,
            Resources resources, Filters filters, int port,
            Configuration.SizingConf sizing, Configuration.TimingConf timing,
            string severMailboxTypeName, string channelMailboxTypeName)
        {
            if (!_actor.IsStopped)
            {
                Action<IServer> cons128873 = __ => __.StartWith(stage, resources, filters, port,
                    sizing, timing, severMailboxTypeName, channelMailboxTypeName);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, null, StartWithRepresentation5);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IServer>(_actor, cons128873,
                        StartWithRepresentation5));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, StartWithRepresentation5));
            }

            return null!;
        }

        public ICompletes<bool> ShutDown()
        {
            if (!_actor.IsStopped)
            {
                Action<IServer> cons128873 = __ => __.ShutDown();
                var completes = new BasicCompletes<bool>(_actor.Scheduler);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, completes, ShutDownRepresentation6);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IServer>(_actor, cons128873, completes,
                        ShutDownRepresentation6));
                }

                return completes;
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, ShutDownRepresentation6));
            }

            return null!;
        }

        public ICompletes<bool> StartUp()
        {
            if (!_actor.IsStopped)
            {
                Action<IServer> cons128873 = __ => __.StartUp();
                var completes = new BasicCompletes<bool>(_actor.Scheduler);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, completes, StartUpRepresentation7);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IServer>(_actor, cons128873, completes,
                        StartUpRepresentation7));
                }

                return completes;
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, StartUpRepresentation7));
            }

            return null!;
        }

        public void Conclude()
        {
            if (!_actor.IsStopped)
            {
                Action<IServer> cons128873 = __ => __.Conclude();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, null, ConcludeRepresentation8);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IServer>(_actor, cons128873,
                        ConcludeRepresentation8));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, ConcludeRepresentation8));
            }
        }

        public void Stop()
        {
            if (!_actor.IsStopped)
            {
                Action<IServer> cons128873 = __ => __.Stop();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, null, StopRepresentation9);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<IServer>(_actor, cons128873, StopRepresentation9));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, StopRepresentation9));
            }
        }
    }
}
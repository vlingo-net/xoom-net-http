// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;

namespace Vlingo.Http.Resource
{
    public class Dispatcher__Proxy : IDispatcher
    {
        private const string RepresentationConclude = "Conclude()";
        private const string DispatchForRepresentation = "DispatchFor(Vlingo.Http.Context)";
        private const string StopRepresentation = "Stop()";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public Dispatcher__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public bool IsStopped => _actor.IsStopped;

        public void DispatchFor(Context context)
        {
            if (!_actor.IsStopped)
            {
                Action<IDispatcher> consumer = actor => actor.DispatchFor(context);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, DispatchForRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDispatcher>(_actor, consumer, DispatchForRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, DispatchForRepresentation));
            }
        }

        public void Stop()
        {
            if (!_actor.IsStopped)
            {
                Action<IDispatcher> consumer = actor => actor.Stop();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, StopRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDispatcher>(_actor, consumer, StopRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, StopRepresentation));
            }
        }
    }
}

// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource
{
    public class Dispatcher__Proxy : IDispatcher
    {
        private const string RepresentationConclude0 = "Conclude()";
        private const string DispatchForRepresentation1 = "DispatchFor(Vlingo.Xoom.Http.Context)";
        private const string StopRepresentation2 = "Stop()";

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
                    _mailbox.Send(_actor, consumer, null, DispatchForRepresentation1);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDispatcher>(_actor, consumer, DispatchForRepresentation1));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, DispatchForRepresentation1));
            }
        }

        public void Stop()
        {
            if (!_actor.IsStopped)
            {
                Action<IDispatcher> consumer = actor => actor.Stop();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, StopRepresentation2);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDispatcher>(_actor, consumer, StopRepresentation2));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, StopRepresentation2));
            }
        }
        
        public void Conclude()
        {
            if (!_actor.IsStopped)
            {
                Action<IStoppable> consumer = actor => actor.Conclude();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, RepresentationConclude0);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IStoppable>(_actor, consumer, RepresentationConclude0));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, RepresentationConclude0));
            }
        }
    }
}

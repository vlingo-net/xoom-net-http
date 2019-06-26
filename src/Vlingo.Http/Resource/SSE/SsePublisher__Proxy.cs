// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;

namespace Vlingo.Http.Resource.SSE
{
    public class SsePublisher__Proxy : ISsePublisher
    {
        private const string SubscribeRepresentation = "Subscribe(Vlingo.Http.Resource.SSE.SseSubscriber)";
        private const string UnsubscribeRepresentation = "Unsubscribe(Vlingo.Http.Resource.SSE.SseSubscriber)";
        private const string StopRepresentation = "Stop()";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public SsePublisher__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public bool IsStopped => _actor.IsStopped;

        public void Stop()
        {
            if (!_actor.IsStopped)
            {
                Action<ISsePublisher> consumer = actor => actor.Stop();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, StopRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<ISsePublisher>(_actor, consumer, StopRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, StopRepresentation));
            }
        }

        public void Subscribe(SseSubscriber subscriber)
        {
            if (!_actor.IsStopped)
            {
                Action<ISsePublisher> consumer = actor => actor.Subscribe(subscriber);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, SubscribeRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<ISsePublisher>(_actor, consumer, SubscribeRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, SubscribeRepresentation));
            }
        }

        public void Unsubscribe(SseSubscriber subscriber)
        {
            if (!_actor.IsStopped)
            {
                Action<ISsePublisher> consumer = actor => actor.Unsubscribe(subscriber);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, UnsubscribeRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<ISsePublisher>(_actor, consumer, UnsubscribeRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, UnsubscribeRepresentation));
            }
        }
    }
}

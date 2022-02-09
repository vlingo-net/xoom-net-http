// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource.Sse
{
    public class SsePublisher__Proxy : ISsePublisher
    {
        private const string RepresentationConclude0 = "Conclude()";
        private const string SubscribeRepresentation1 = "Subscribe(Vlingo.Xoom.Http.Resource.SSE.SseSubscriber)";
        private const string UnsubscribeRepresentation2 = "Unsubscribe(Vlingo.Xoom.Http.Resource.SSE.SseSubscriber)";
        private const string StopRepresentation3 = "Stop()";

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
                    _mailbox.Send(_actor, consumer, null, StopRepresentation3);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<ISsePublisher>(_actor, consumer, StopRepresentation3));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, StopRepresentation3));
            }
        }

        public void Subscribe(SseSubscriber subscriber)
        {
            if (!_actor.IsStopped)
            {
                Action<ISsePublisher> consumer = actor => actor.Subscribe(subscriber);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, SubscribeRepresentation1);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<ISsePublisher>(_actor, consumer, SubscribeRepresentation1));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, SubscribeRepresentation1));
            }
        }

        public void Unsubscribe(SseSubscriber subscriber)
        {
            if (!_actor.IsStopped)
            {
                Action<ISsePublisher> consumer = actor => actor.Unsubscribe(subscriber);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, UnsubscribeRepresentation2);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<ISsePublisher>(_actor, consumer, UnsubscribeRepresentation2));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, UnsubscribeRepresentation2));
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

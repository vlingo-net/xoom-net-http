// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Wire.Message;

namespace Vlingo.Http.Resource
{
    public class ClientConsumer__Proxy : IClientConsumer
    {
        private const string RepresentationConclude0 = "Conclude()";
        private const string RequestWithRepresentation1 = "RequestWith(Vlingo.Http.Request)";
        private const string ConsumeRepresentation3 = "Consume(Vlingo.Wire.Message.IConsumerByteBuffer)";
        private const string IntervalSignalRepresentation4 = "IntervalSignal(Vlingo.Actors.IScheduled<object>, object)";
        private const string StopRepresentation5 = "Stop()";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public ClientConsumer__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public bool IsStopped => _actor.IsStopped;

        public void Consume(IConsumerByteBuffer buffer)
        {
            if (!_actor.IsStopped)
            {
                Action<IClientConsumer> consumer = actor => actor.Consume(buffer);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, ConsumeRepresentation3);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IClientConsumer>(_actor, consumer, ConsumeRepresentation3));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, ConsumeRepresentation3));
            }
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            if (!_actor.IsStopped)
            {
                Action<IClientConsumer> consumer = actor => actor.IntervalSignal(scheduled, data);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, IntervalSignalRepresentation4);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IClientConsumer>(_actor, consumer, IntervalSignalRepresentation4));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, IntervalSignalRepresentation4));
            }
        }

        public ICompletes<Response> RequestWith(Request request, ICompletes<Response> completes)
        {
            if (!_actor.IsStopped)
            {
                Action<IClientConsumer> consumer = actor => actor.RequestWith(request, completes);
                var innerCompletes = new BasicCompletes<Response>(_actor.Scheduler);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, innerCompletes, RequestWithRepresentation1);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IClientConsumer>(_actor, consumer, innerCompletes, RequestWithRepresentation1));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, RequestWithRepresentation1));
            }

            return null;
        }

        public void Stop()
        {
            if (!_actor.IsStopped)
            {
                Action<IClientConsumer> consumer = actor => actor.Stop();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, StopRepresentation5);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IClientConsumer>(_actor, consumer, StopRepresentation5));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, StopRepresentation5));
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
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, RepresentationConclude0));
            }
        }
    }
}

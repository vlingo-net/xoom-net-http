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
        private const string RepresentationConclude = "Conclude()";
        private const string RequestWithRepresentation = "RequestWith(Vlingo.Http.Request)";
        private const string ConsumeRepresentation = "Consume(Vlingo.Wire.Message.IConsumerByteBuffer)";
        private const string IntervalSignalRepresentation = "IntervalSignal(Vlingo.Actors.IScheduled<object>, object)";
        private const string StopRepresentation = "Stop()";

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
                    _mailbox.Send(_actor, consumer, null, ConsumeRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IClientConsumer>(_actor, consumer, ConsumeRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, ConsumeRepresentation));
            }
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            if (!_actor.IsStopped)
            {
                Action<IClientConsumer> consumer = actor => actor.IntervalSignal(scheduled, data);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, IntervalSignalRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IClientConsumer>(_actor, consumer, IntervalSignalRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, IntervalSignalRepresentation));
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
                    _mailbox.Send(_actor, consumer, innerCompletes, RequestWithRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IClientConsumer>(_actor, consumer, innerCompletes, RequestWithRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, RequestWithRepresentation));
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
                    _mailbox.Send(_actor, consumer, null, StopRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IClientConsumer>(_actor, consumer, StopRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters.FailedDelivery(new DeadLetter(_actor, StopRepresentation));
            }
        }
    }
}

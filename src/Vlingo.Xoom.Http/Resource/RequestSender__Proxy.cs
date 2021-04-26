// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource
{
    public class RequestSender__Proxy : IRequestSender
    {
        private const string RepresentationConclude = "Conclude()";
        private const string SendRequestRepresentation = "SendRequest(Request)";
        private const string StopRepresentation = "Stop()";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public RequestSender__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public bool IsStopped => _actor.IsStopped;

        public void Conclude()
        {
            if (!_actor.IsStopped)
            {
                Action<IStoppable> consumer = actor => actor.Conclude();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, RepresentationConclude);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IStoppable>(_actor, consumer, RepresentationConclude));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, RepresentationConclude));
            }
        }

        public void SendRequest(Request request)
        {
            if (!_actor.IsStopped)
            {
                Action<IRequestSender> consumer = actor => actor.SendRequest(request);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, SendRequestRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IRequestSender>(_actor, consumer, SendRequestRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, SendRequestRepresentation));
            }
        }

        public void Stop()
        {
            if (!_actor.IsStopped)
            {
                Action<IStoppable> consumer = actor => actor.Stop();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, StopRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IStoppable>(_actor, consumer, StopRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, StopRepresentation));
            }
        }
    }
}

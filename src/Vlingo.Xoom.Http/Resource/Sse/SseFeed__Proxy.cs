// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource.Sse
{
    public class SseFeed__Proxy : ISseFeed
    {
        private const string ToRepresentation = "To(ICollection<Vlingo.Xoom.Http.Resource.SSE.SseSubscriber>)";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public SseFeed__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public void To(System.Collections.Generic.ICollection<SseSubscriber> subscribers)
        {
            if (!_actor.IsStopped)
            {
                Action<ISseFeed> consumer = actor => actor.To(subscribers);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, ToRepresentation);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<ISseFeed>(_actor, consumer, ToRepresentation));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, ToRepresentation));
            }
        }
    }
}

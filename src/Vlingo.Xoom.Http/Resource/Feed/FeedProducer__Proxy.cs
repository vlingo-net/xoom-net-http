// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource.Feed;

public class FeedProducer__Proxy : IFeedProducer
{
    private const string ProduceFeedForRepresentation1 =
        "ProduceFeedFor(Vlingo.Xoom.Http.Resource.Feed.FeedProductRequest)";

    private readonly Actor _actor;
    private readonly IMailbox _mailbox;

    public FeedProducer__Proxy(Actor actor, IMailbox mailbox)
    {
        _actor = actor;
        _mailbox = mailbox;
    }

    public void ProduceFeedFor(FeedProductRequest request)
    {
        if (!_actor.IsStopped)
        {
            Action<IFeedProducer> cons1617155091 = __ => __.ProduceFeedFor(request);
            if (_mailbox.IsPreallocated)
                _mailbox.Send(_actor, cons1617155091, null, ProduceFeedForRepresentation1);
            else
                _mailbox.Send(new LocalMessage<IFeedProducer>(_actor, cons1617155091, ProduceFeedForRepresentation1));
        }
        else
        {
            _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, ProduceFeedForRepresentation1));
        }
    }
}
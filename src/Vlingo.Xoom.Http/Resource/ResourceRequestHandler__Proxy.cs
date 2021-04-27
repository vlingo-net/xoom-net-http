// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource
{
    public class ResourceRequestHandler__Proxy : IResourceRequestHandler
    {
        private const string HandleForRepresentation1 = "HandleFor<T>(Vlingo.Xoom.Http.Context, Action<T>)";
        private const string HandleForRepresentation2 = "HandleFor(Vlingo.Xoom.Http.Context, Action.MappedParameters, RequestHandler)";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public ResourceRequestHandler__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public void HandleFor<T>(Context context, Action<T>? consumer)
            where T : ResourceHandler
        {
            if (!_actor.IsStopped)
            {
                Action<IResourceRequestHandler> cons128873 = __ =>
                    __.HandleFor<T>(context, consumer);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, null, HandleForRepresentation1);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<IResourceRequestHandler>(_actor, cons128873,
                            HandleForRepresentation1));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, HandleForRepresentation1));
            }
        }

        public void HandleFor(Context context, Action.MappedParameters mappedParameters, RequestHandler handler)
        {
            if (!_actor.IsStopped)
            {
                Action<IResourceRequestHandler> cons128873 = __ => __.HandleFor(context, mappedParameters, handler);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons128873, null, HandleForRepresentation2);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<IResourceRequestHandler>(_actor, cons128873,
                            HandleForRepresentation2));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, HandleForRepresentation2));
            }
        }
    }
}
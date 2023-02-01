// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Xoom.Http.Resource.Feed;

public class FeedResourceDispatcher : ConfigurationResource
{
    public FeedResourceDispatcher(string name, Type resourceHandlerClass, int handlerPoolSize,
        IList<Action> actions) : base(name, resourceHandlerClass, handlerPoolSize, actions)
    {
    }

    public override void DispatchToHandlerWith(Context context, Action.MappedParameters? mappedParameters)
    {
        Action<FeedResource> consumer;

        try
        {
            switch (mappedParameters?.ActionId)
            {
                case 0
                    : // GET /feeds/{feedName}/{feedItemId} feed(String feedName, String feedProductId, Class<? extends Actor> feedProducerClass, int feedProductElements)
                    consumer = handler => handler.Feed((string) mappedParameters.Mapped[0].Value!,
                        (string) mappedParameters.Mapped[1].Value!, (Type) mappedParameters.Mapped[2].Value!,
                        (int) mappedParameters.Mapped[3].Value!);
                    PooledHandler.HandleFor(context, consumer);
                    break;
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Action mismatch: Request: {context.Request}Parameters: {mappedParameters}", e);
        }
    }
}
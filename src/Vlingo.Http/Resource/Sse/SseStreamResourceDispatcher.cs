// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Http.Resource.Sse
{
    public class SseStreamResourceDispatcher : ConfigurationResource<SseStreamResource>
    {
        public SseStreamResourceDispatcher(
          string name,
          Type resourceHandlerClass,
          int handlerPoolSize,
          IList<Action> actions)
            : base(name, resourceHandlerClass, handlerPoolSize, actions)
        {
        }

        public override void DispatchToHandlerWith(Context context, Action.MappedParameters mappedParameters)
        {
            try
            {
                Action<SseStreamResource> consumer;
                switch (mappedParameters.ActionId)
                {
                    case 0: // GET /eventstreams/{streamName}
                        consumer = handler => handler.SubscribeToStream(
                            (string)mappedParameters.Mapped[0].Value!,
                            (Type)mappedParameters.Mapped[1].Value!,
                            (int)mappedParameters.Mapped[2].Value!,
                            (int)mappedParameters.Mapped[3].Value!,
                            (string)mappedParameters.Mapped[4].Value!);

                        PooledHandler.HandleFor(context, consumer);
                        break;

                    case 1: // DELETE /eventstreams/{streamName}/{id}
                        consumer = handler => handler.UnsubscribeFromStream(
                            (string)mappedParameters.Mapped[0].Value!,
                            (string)mappedParameters.Mapped[1].Value!);

                        PooledHandler.HandleFor(context, consumer);
                        break;
                }
            }
            catch(Exception ex)
            {
                throw new ArgumentException($"Action mismatch: Request: {context.Request} Parameters: {mappedParameters}", ex);
            }
        }
    }
}

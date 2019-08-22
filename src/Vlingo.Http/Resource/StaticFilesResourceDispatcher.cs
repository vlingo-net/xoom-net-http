// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Http.Resource
{
    public class StaticFilesResourceDispatcher : ConfigurationResource
    {
        public StaticFilesResourceDispatcher(
            string name,
            Type resourceHandlerClass,
            int handlerPoolSize,
            IList<Action> actions)
            : base(name, resourceHandlerClass, handlerPoolSize, actions)
        {
        }

        public override void DispatchToHandlerWith(Context context, Action.MappedParameters mappedParameters)
        {
            Action<StaticFilesResource> consumer = null;

            try
            {
                switch (mappedParameters.ActionId)
                {
                    case 0: // GET %root%{path} ServeFile(string root, string paths, string contentFilePath)
                        consumer = handler => handler.ServeFile((string)mappedParameters.Mapped[0].Value, (string)mappedParameters.Mapped[1].Value, (string)mappedParameters.Mapped[2].Value);
                        PooledHandler.HandleFor(context, consumer);
                        break;
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("Action mismatch: Request: " + context.Request + " Parameters: " + mappedParameters);
            }
        }
    }
}

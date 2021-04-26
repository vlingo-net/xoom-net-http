// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Xoom.Http.Resource
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

        public override void DispatchToHandlerWith(Context context, Action.MappedParameters? mappedParameters)
        {
            try
            {
                switch (mappedParameters?.ActionId)
                {
                    case 0: // GET %root%{path} ServeFile(string root, string paths, string contentFilePath)
                        if (mappedParameters.Mapped.Count == 3)
                        {
                            PooledHandler.HandleFor<StaticFilesResource>(context, handler => handler.ServeFile((string) mappedParameters.Mapped[0].Value!, (string) mappedParameters.Mapped[1].Value!, (string) mappedParameters.Mapped[2].Value!));
                        }
                        else
                        {
                            PooledHandler.HandleFor<StaticFilesResource>(context, handler => handler.ServeFile(string.Empty, (string) mappedParameters.Mapped[0].Value!, (string) mappedParameters.Mapped[1].Value!));

                        }
                        break;
                }
            }
            catch (Exception)
            {
                throw new ArgumentException($"Action mismatch: Request: {context.Request} Parameters: {mappedParameters}");
            }
        }
    }
}

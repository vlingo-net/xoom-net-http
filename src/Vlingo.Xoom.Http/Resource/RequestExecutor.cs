// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http.Resource
{
    internal abstract class RequestExecutor
    {
        internal static ICompletes<Response> ExecuteRequest(
            Func<ICompletes<Response>?> executeAction,
            IErrorHandler errorHandler,
            ILogger logger)
        {

            try
            {
                return executeAction.Invoke()?
                    .RecoverFrom(ex => ResourceErrorProcessor.ResourceHandlerError(errorHandler, logger, ex))!;
            }
            catch (Exception ex)
            {
                return Completes.WithFailure(ResourceErrorProcessor.ResourceHandlerError(errorHandler, logger, ex));
            }
        }
    }
}

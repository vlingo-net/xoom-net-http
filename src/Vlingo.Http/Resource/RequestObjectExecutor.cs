// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Http.Resource
{
    internal abstract class RequestObjectExecutor
    {
        internal static ICompletes<Response> ExecuteRequest(
            Request request,
            MediaTypeMapper mediaTypeMapper,
            Func<ICompletes<IObjectResponse>> executeAction,
            IErrorHandler errorHandler,
            ILogger logger)
        {
            try
            {
                return executeAction.Invoke()
                  .AndThen(objectResponse => ToResponse(objectResponse, request, mediaTypeMapper, errorHandler, logger));
            }
            catch (Exception ex)
            {
                return Completes.WithFailure(ResourceErrorProcessor.ResourceHandlerError(errorHandler, logger, ex));
            }
        }

        internal static Response ToResponse(
            IObjectResponse objectResponse,
            Request request,
            MediaTypeMapper mediaTypeMapper,
            IErrorHandler errorHandler,
            ILogger logger)
            => Success.Of<Exception, Response>(objectResponse.ResponseFrom(request, mediaTypeMapper))
                .Resolve(
                    ex => ResourceErrorProcessor.ResourceHandlerError(errorHandler, logger, ex),
                    response => response);
    }
}

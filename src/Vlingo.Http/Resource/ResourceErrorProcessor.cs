// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;

namespace Vlingo.Http.Resource
{
    public class ResourceErrorProcessor
    {
        public static Response DefaultErrorResponse() => Response.Of(ResponseStatus.InternalServerError);

        public static Response ResourceHandlerError(IErrorHandler errorHandler, ILogger logger, Exception exception)
        {
            Response response;
            try
            {
                logger.Error("Exception thrown by Resource execution", exception);
                response = errorHandler != null ?
                    errorHandler.Handle(exception) :
                    DefaultErrorHandler.Instance.Handle(exception);
            }
            catch
            {
                logger.Error("Exception thrown by error handler when handling error", exception);
                response = DefaultErrorResponse();
            }
            return response;
        }
    }
}
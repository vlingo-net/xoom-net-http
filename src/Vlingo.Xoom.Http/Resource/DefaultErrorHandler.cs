// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Http.Resource;

public static class DefaultErrorHandler
{
    private static Func<Exception, Response> Handler = ex =>
    {
        if (ex is MediaTypeNotSupportedException)
        {
            return Response.Of(ResponseStatus.UnsupportedMediaType);
        }
        else if (ex is ArgumentException)
        {
            return Response.Of(ResponseStatus.BadRequest);
        }
        else
        {
            return Response.Of(ResponseStatus.InternalServerError);
        }
    };

    public static IErrorHandler Instance { get; } = new ErrorHandlerImpl(Handler);
}
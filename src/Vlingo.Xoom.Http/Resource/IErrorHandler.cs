// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Http.Resource;

public interface IErrorHandler
{
    Response Handle(Exception error);
}

public static class ErrorHandler
{
    public static IErrorHandler HandleAllWith(ResponseStatus status)
        => new ErrorHandlerImpl(err => Response.Of(status));
}

internal class ErrorHandlerImpl : IErrorHandler
{
    private readonly Func<Exception, Response> _handler;

    public ErrorHandlerImpl(Func<Exception, Response> handler)
    {
        _handler = handler;
    }

    public Response Handle(Exception error)
        => _handler.Invoke(error);
}
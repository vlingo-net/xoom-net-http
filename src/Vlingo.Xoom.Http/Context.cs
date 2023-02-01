// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Wire.Channel;

namespace Vlingo.Xoom.Http;

public class Context
{
    private readonly ICompletesEventually _completes;

    public Context(RequestResponseContext? requestResponseContext, Request? request, ICompletesEventually completes)
    {
        ClientContext = requestResponseContext;
        Request = request;
        _completes = completes;
    }

    public Context(Request? request, ICompletesEventually completes)
        : this(null, request, completes)
    {
    }

    public Context(ICompletesEventually completes)
        : this(null, completes)
    {
    }

    public RequestResponseContext? ClientContext { get; }

    public bool HasClientContext => ClientContext != null;

    public Request? Request { get; }

    public bool HasRequest => Request != null;

    public override string ToString() => $"Context [completes={_completes}, request=\n{Request}]";

    internal ICompletesEventually Completes => _completes;
}
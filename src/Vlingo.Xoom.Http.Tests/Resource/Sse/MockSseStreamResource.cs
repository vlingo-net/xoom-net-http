// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Resource.Sse;

namespace Vlingo.Xoom.Http.Tests.Resource.Sse;

public class MockSseStreamResource : SseStreamResource
{
    private readonly ICompletesEventually _completes;

    private Request _request;

    public readonly MockRequestResponseContext RequestResponseContext;
        
    public MockSseStreamResource(World world) : base(world)
    {
        _completes = world.CompletesFor(new BasicCompletes<string>(world.Stage.Scheduler));
        RequestResponseContext = new MockRequestResponseContext(new MockResponseSenderChannel());
    }

    public void NextRequest(Request request) => _request = request;

    protected override ICompletesEventually Completes => _completes;

    public override Context Context => new Context(RequestResponseContext, _request, _completes);
}
// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Http.Resource.Sse;

namespace Vlingo.Http.Tests.Resource.Sse
{
    public class MockSseStreamResource : SseStreamResource
    {
        private ICompletesEventually _completes;

        private Request _request;

        public MockRequestResponseContext _requestResponseContext;
        
        public MockSseStreamResource(World world) : base(world)
        {
            _completes = world.CompletesFor(new BasicCompletes<string>(world.Stage.Scheduler));
            _requestResponseContext = new MockRequestResponseContext(new MockResponseSenderChannel());
        }

        public void NextRequest(Request request) => _request = request;

        protected override ICompletesEventually Completes => _completes;

        public override Context Context => new Context(_requestResponseContext, _request, _completes);
    }
}
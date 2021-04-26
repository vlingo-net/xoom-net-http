// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;

namespace Vlingo.Http.Tests.Resource
{
    using Vlingo.Http.Resource;
    
    public class TestDispatcher : IDispatcher
    {
        private readonly Resources _resources;
        private readonly ILogger _logger;

        public TestDispatcher(Resources resources, ILogger logger)
        {
            _resources = resources;
            _logger = logger;
        }


        public void Conclude()
        {
        }

        public void Stop()
        {
        }

        public bool IsStopped => false;

        public void DispatchFor(Context context) => _resources.DispatchMatching(context, _logger);
    }
}
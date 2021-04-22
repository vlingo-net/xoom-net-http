// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Http.Resource
{
    public class AgentDispatcherPool : AbstractDispatcherPool
    {
        private readonly AtomicLong _dispatcherPoolIndex;
        private readonly long _dispatcherPoolSize;
        
        public AgentDispatcherPool(Stage stage, Resources resources, int dispatcherPoolSize) : base(stage, resources, dispatcherPoolSize)
        {
            _dispatcherPoolIndex = new AtomicLong(-1);
            _dispatcherPoolSize = dispatcherPoolSize;
        }

        public override IDispatcher Dispatcher()
        {
            var index = (int) (_dispatcherPoolIndex.IncrementAndGet() % _dispatcherPoolSize);
            return DispatcherPool[index];
        }
    }
}
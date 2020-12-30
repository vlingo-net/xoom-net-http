// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;

namespace Vlingo.Http.Resource
{
    /// <summary>
    /// Default behavior for all <see cref="IDispatcherPool"/> implementations.
    /// </summary>
    public abstract class AbstractDispatcherPool : IDispatcherPool
    {
        protected readonly IDispatcher[] DispatcherPool;

        protected AbstractDispatcherPool(Stage stage, Resources resources, int dispatcherPoolSize)
        {
            DispatcherPool = new IDispatcher[dispatcherPoolSize];

            for (var idx = 0; idx < dispatcherPoolSize; ++idx)
            {
                DispatcherPool[idx] = Http.Resource.Dispatcher.StartWith(stage, resources);
            }
        }
        
        public void Close()
        {
            foreach (var dispatcher in DispatcherPool)
            {
                dispatcher.Stop();
            }
        }

        public abstract IDispatcher Dispatcher();
    }
}
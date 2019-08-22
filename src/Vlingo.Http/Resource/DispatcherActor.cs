// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;

namespace Vlingo.Http.Resource
{
    public class DispatcherActor : Actor, IDispatcher
    {
        private readonly Resources _resources;

        public DispatcherActor(Resources resources)
        {
            _resources = resources;
            AllocateHandlerPools();
        }

        public void DispatchFor(Context context)
            => _resources.DispatchMatching(context, Logger);

        private void AllocateHandlerPools()
        {
            foreach (var resource in _resources.NamedResources.Values)
            {
                resource.AllocateHandlerPool(Stage);
            }
        }
    }
}

// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
using Vlingo.Common;

namespace Vlingo.Http.Resource
{
    public abstract class Resource
    {
        public string Name { get; }
        public int HandlerPoolSize { get; }

        private readonly IResourceRequestHandler[] _handlerPool;
        private readonly AtomicLong _handlerPoolIndex;

        public Resource(string name, int handlerPoolSize)
        {
            Name = name;
            HandlerPoolSize = handlerPoolSize;
            _handlerPool = new IResourceRequestHandler[handlerPoolSize];
            _handlerPoolIndex = new AtomicLong(0);
        }

        public abstract void DispatchToHandlerWith(Context context, Action.MappedParameters? mappedParameters);
        internal abstract Action.MatchResults MatchWith(Method? method, Uri? uri);
        protected abstract ResourceHandler ResourceHandlerInstance(Stage stage);

        internal void AllocateHandlerPool(Stage stage)
        {
            for (var i = 0; i < HandlerPoolSize; ++i)
            {
                _handlerPool[i] = stage.ActorFor<IResourceRequestHandler>(
                    Definition.Has<ResourceRequestHandlerActor>(
                        Definition.Parameters(ResourceHandlerInstance(stage))));
            }
        }

        protected IResourceRequestHandler PooledHandler
        {
            get
            {
                var index = (int)(_handlerPoolIndex.IncrementAndGet() % HandlerPoolSize);
                return _handlerPool[index];
            }
        }
    }
}

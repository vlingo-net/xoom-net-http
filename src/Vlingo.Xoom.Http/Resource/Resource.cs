// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http.Resource
{
    public abstract class Resource : IResource
    {
        private readonly IResourceRequestHandler[] _handlerPool;
        private readonly AtomicLong _handlerPoolIndex;

        public Resource(string name, int handlerPoolSize)
        {
            Name = name;
            HandlerPoolSize = handlerPoolSize;
            _handlerPool = new IResourceRequestHandler[handlerPoolSize];
            _handlerPoolIndex = new AtomicLong(0);
        }
        
        public string Name { get; }
        public int HandlerPoolSize { get; }

        public abstract void DispatchToHandlerWith(Context context, Action.MappedParameters? mappedParameters);
        public abstract Action.MatchResults MatchWith(Method? method, Uri? uri);
        public abstract void Log(ILogger logger);

        public abstract ResourceHandler ResourceHandlerInstance(Stage stage);

        public void AllocateHandlerPool(Stage stage)
        {
            for (var i = 0; i < HandlerPoolSize; ++i)
            {
                _handlerPool[i] = stage.ActorFor<IResourceRequestHandler>(
                    () => new ResourceRequestHandlerActor(ResourceHandlerInstance(stage)));
            }
        }

        public IResourceRequestHandler PooledHandler
        {
            get
            {
                var index = (int)(_handlerPoolIndex.IncrementAndGet() % HandlerPoolSize);
                return _handlerPool[index];
            }
        }
    }
}
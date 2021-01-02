// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;

namespace Vlingo.Http.Resource
{
    public abstract class DynamicResourceHandler
    {
        protected DynamicResourceHandler(Stage stage)
        {
            Stage = stage;
            Logger = Stage.World.DefaultLogger;
        }

        public abstract Resource Routes { get; }

        public void SetContext(Context context) => Context = context;
        
        public Context Context { get; private set; }
        
        public ILogger Logger { get; }
        
        public Stage Stage { get; }
    }
}
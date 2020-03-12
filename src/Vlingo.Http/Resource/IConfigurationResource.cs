// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;

namespace Vlingo.Http.Resource
{
    public interface IConfigurationResource : IResource
    {
        Type ResourceHandlerClass { get; }
        IReadOnlyList<Action> Actions { get; }
        IResourceRequestHandler PooledHandler { get; }
        ResourceHandler ResourceHandlerInstance(Stage stage);
    }
}
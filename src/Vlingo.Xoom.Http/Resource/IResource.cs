// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource
{
    public interface IResource
    {
        string Name { get; }
        int HandlerPoolSize { get; }
        void DispatchToHandlerWith(Context context, Action.MappedParameters? mappedParameters);
        Action.MatchResults MatchWith(Method? method, Uri? uri);
        void Log(ILogger logger);
        void AllocateHandlerPool(Stage stage);
    }
}
// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;

namespace Vlingo.Http.Resource
{
    public interface IDispatcher : IStoppable
    {
        void DispatchFor(Context context);
    }

    public static class Dispatcher
    {
        public static IDispatcher StartWith(Stage stage, Resources resources)
            => stage.ActorFor<IDispatcher>(
                Definition.Has<DispatcherActor>(
                    Definition.Parameters(resources)));
    }
}

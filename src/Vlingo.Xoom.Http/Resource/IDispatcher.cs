// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource;

public interface IDispatcher : IStoppable
{
    void DispatchFor(Context context);
}

public static class Dispatcher
{
    public static IDispatcher StartWith(Stage stage, Resources resources)
        => stage.ActorFor<IDispatcher>(() => new DispatcherActor(resources));
}
﻿// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;

namespace Vlingo.Http.Resource.Sse
{
    public interface ISsePublisher : IStoppable
    {
        void Subscribe(SseSubscriber subscriber);
        void Unsubscribe(SseSubscriber subscriber);
    }
}

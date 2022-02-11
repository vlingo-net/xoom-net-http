// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Http.Resource;
using IDispatcher = Vlingo.Xoom.Http.Resource.IDispatcher;

namespace Vlingo.Xoom.Http.Tests.Resource;

public class TestDispatcher : IDispatcher
{
    private readonly Resources _resources;
    private readonly ILogger _logger;

    public TestDispatcher(Resources resources, ILogger logger)
    {
        _resources = resources;
        _logger = logger;
    }


    public void Conclude()
    {
    }

    public void Stop()
    {
    }

    public bool IsStopped => false;

    public void DispatchFor(Context context) => _resources.DispatchMatching(context, _logger);
}
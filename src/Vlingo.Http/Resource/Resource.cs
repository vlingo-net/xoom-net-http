// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;

namespace Vlingo.Http.Resource
{
    public abstract class Resource<T>
    {
        protected IResourceRequestHandler PooledHandler { get; }

        public abstract void DispatchToHandlerWith(Context context, Action.MappedParameters mappedParameters);
    }
}

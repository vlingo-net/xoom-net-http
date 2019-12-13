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
    public abstract class ResourceHandler
    {
        private Context? _context;
        private Stage? _stage;

        public virtual Resource Routes() => throw new NotSupportedException("Undefined resource; must override.");

        protected ResourceHandler() { }

        protected virtual ICompletesEventually? Completes => Context?.Completes;

        public virtual Context? Context
        {
            get => _context;
            set => _context = value;
        }

        protected internal virtual ILogger? Logger => _stage?.World.DefaultLogger;

        public virtual Scheduler? Scheduler => _stage?.Scheduler;

        public virtual Stage? Stage { get => _stage; set => _stage = value; }
    }
}

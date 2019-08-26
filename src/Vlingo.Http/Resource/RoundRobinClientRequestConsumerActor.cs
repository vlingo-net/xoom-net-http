// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Wire.Message;

namespace Vlingo.Http.Resource
{
    /// <summary>
    /// Round-robin router of <see cref="IClientConsumer"/> requests.
    /// </summary>
    public class RoundRobinClientRequestConsumerActor : RoundRobinRouter<IClientConsumer>, IClientConsumer
    {
        public RoundRobinClientRequestConsumerActor(RouterSpecification<IClientConsumer> specification) : base(specification)
        {
        }

        public void Consume(IConsumerByteBuffer buffer)
        {
            throw new System.NotImplementedException();
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            throw new System.NotImplementedException();
        }

        public ICompletes<Response> RequestWith(Request request, ICompletes<Response> completes)
        {
            throw new System.NotImplementedException();
        }
    }
}
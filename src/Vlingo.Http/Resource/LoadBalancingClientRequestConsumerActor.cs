// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Wire.Message;

namespace Vlingo.Http.Resource
{
    /// <summary>
    /// Load-balancing router of <code>IClientConsumer</code> requests.
    /// </summary>
    public class LoadBalancingClientRequestConsumerActor : SmallestMailboxRouter<IClientConsumer>, IClientConsumer
    {
        private const string ErrorMessage = "LoadBalancingClientRequestConsumerActor: Should not be reached. Message: ";

        public LoadBalancingClientRequestConsumerActor(
            Client.Configuration configuration,
            RouterSpecification<IClientConsumer> specification) 
            : base(specification)
        {
        }

        public void Consume(IConsumerByteBuffer buffer)
        {
            var message = $"{ErrorMessage} Consume()";
            Logger.Error(message, new NotSupportedException(message));
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            var message = $"{ErrorMessage} IntervalSignal()";
            Logger.Error(message, new NotSupportedException(message));
        }

        public ICompletes<Response> RequestWith(Request request, ICompletes<Response> completes)
        {
            DispatchCommand((a, b, c) => a.RequestWith(b, c), request, completes);
            return completes;
        }
    }
}

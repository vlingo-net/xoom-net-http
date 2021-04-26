// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Wire.Message;

namespace Vlingo.Xoom.Http.Resource
{
    /// <summary>
    /// Round-robin router of <see cref="IClientConsumer"/> requests.
    /// </summary>
    public class RoundRobinClientRequestConsumerActor : RoundRobinRouter<IClientConsumer>, IClientConsumer
    {
        private const string ErrorMessage = "RoundRobinClientRequestConsumerActor: Should not be reached. Message: ";
        
        /// <summary>
        /// Constructs my default state.
        /// </summary>
        /// <param name="configuration">The <see cref="Configuration"/></param>
        /// <param name="specification">The <see cref="RouterSpecification{T}"/></param>
        /// <exception cref="Exception">When the router cannot be initialized</exception>
        public RoundRobinClientRequestConsumerActor(Client.Configuration configuration, RouterSpecification<IClientConsumer> specification) : base(specification)
        {
        }

        public void Consume(IConsumerByteBuffer buffer)
        {
            const string message = ErrorMessage + "Consume()";
            Logger.Error(message, new InvalidOperationException(message));
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            const string message = ErrorMessage + "IntervalSignal()";
            Logger.Error(message, new InvalidOperationException(message));
        }

        public ICompletes<Response> RequestWith(Request request, ICompletes<Response> completes)
        {
            DispatchCommand((consumer, r, c) => consumer.RequestWith(r, c), request, completes);
            return completes;
        }
    }
}
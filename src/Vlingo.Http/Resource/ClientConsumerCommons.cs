// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Wire.Channel;
using Vlingo.Wire.Fdx.Bidirectional;
using Vlingo.Xoom.Actors;

namespace Vlingo.Http.Resource
{
    public static class ClientConsumerCommons
    {
        public static IClientRequestResponseChannel ClientChannel(
            Client.Configuration configuration,
            IResponseChannelConsumer consumer,
            ILogger logger)
        {
            if (configuration.IsSecure)
            {
                return new SecureClientRequestResponseChannel(
                  configuration.AddressOfHost,
                  consumer,
                  configuration.ReadBufferPoolSize,
                  configuration.ReadBufferSize,
                  logger);
            }

            return new BasicClientRequestResponseChannel(
                configuration.AddressOfHost,
                consumer,
                configuration.ReadBufferPoolSize,
                configuration.ReadBufferSize,
                logger);
        }
    }
}

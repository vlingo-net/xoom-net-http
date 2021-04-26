// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.IO;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Wire.Channel;
using Vlingo.Xoom.Wire.Fdx.Bidirectional;

namespace Vlingo.Http.Resource
{
    /// <summary>
    /// The client that is a request sender and that checks for
    /// responses and consumes them.
    /// </summary>
    public interface IClientConsumer : IResponseChannelConsumer, IScheduled<object>, IStoppable
    {
        ICompletes<Response> RequestWith(Request request, ICompletes<Response> completes);
    }

    internal sealed class State
    {
        public MemoryStream Buffer { get; }
        public IClientRequestResponseChannel Channel { get; }
        public Client.Configuration Configuration { get; }
        public ResponseParser? Parser { get; internal set; }
        public ICancellable Probe { get; }

        public State(
            Client.Configuration configuration,
            IClientRequestResponseChannel channel,
            ResponseParser? parser,
            ICancellable probe,
            MemoryStream buffer)
        {
            Configuration = configuration;
            Channel = channel;
            Parser = parser;
            Probe = probe;
            Buffer = buffer;
        }
    }
}

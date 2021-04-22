// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using Vlingo.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Wire.Channel;
using Vlingo.Wire.Fdx.Bidirectional;
using Vlingo.Wire.Message;

namespace Vlingo.Http.Resource
{
    public sealed class RequestSenderProbeActor : Actor, IRequestSender, IScheduled<object>
    {
        private readonly MemoryStream _buffer;
        private readonly IClientRequestResponseChannel _channel;
        private readonly ICancellable _cancellable;

        public RequestSenderProbeActor(Client.Configuration configuration, IResponseChannelConsumer consumer, string testId)
        {
            _channel = ClientConsumerCommons.ClientChannel(configuration, consumer, Logger);
            _cancellable = Stage.Scheduler.Schedule(
                SelfAs<IScheduled<object?>>(), 
                null, 
                TimeSpan.FromMilliseconds(1), 
                TimeSpan.FromMilliseconds(configuration.ProbeInterval));
            _buffer = new MemoryStream(configuration.WriteBufferSize);
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
            => _channel.ProbeChannel();

        public void SendRequest(Request request)
        {
            _buffer.Clear();
            var array = Converters.TextToBytes(request.ToString());
            _buffer.Write(array, 0, array.Length);
            _buffer.Flip();
            _channel.RequestWith(_buffer.ToArray());
        }

        public override void Stop()
        {
            _cancellable.Cancel();
            _channel.Close();
            base.Stop();
        }
    }
}

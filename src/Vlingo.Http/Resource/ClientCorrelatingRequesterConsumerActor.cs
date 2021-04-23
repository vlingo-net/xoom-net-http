// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.UUID;
using Vlingo.Wire.Channel;
using Vlingo.Wire.Message;
using Vlingo.Xoom.Actors;

namespace Vlingo.Http.Resource
{
    public class ClientCorrelatingRequesterConsumerActor : Actor, IClientConsumer
    {
        private readonly IDictionary<string, ICompletesEventually> _completables;
        private readonly State _state;
        private readonly RandomBasedGenerator _randomUuidGenerator;

        public ClientCorrelatingRequesterConsumerActor(Client.Configuration configuration)
        {
            _state = new State(
                configuration,
                ClientConsumerCommons.ClientChannel(configuration, SelfAs<IResponseChannelConsumer>(), Logger),
                null,
                Stage.Scheduler.Schedule(
                    SelfAs<IScheduled<object?>>(),
                    null,
                    TimeSpan.FromMilliseconds(1),
                    TimeSpan.FromMilliseconds(configuration.ProbeInterval)),
                new MemoryStream(configuration.WriteBufferSize));

            _completables = new Dictionary<string, ICompletesEventually>();
            _randomUuidGenerator = new RandomBasedGenerator();
        }

        public void Consume(IConsumerByteBuffer buffer)
        {
            try
            {
                if (_state.Parser == null)
                {
                    _state.Parser = ResponseParser.ParserFor(buffer);
                }
                else
                {
                    _state.Parser.ParseNext(buffer);
                }

                while (_state.Parser.HasFullResponse())
                {
                    var response = _state.Parser.FullResponse();
                    var correlationId = response.Headers.HeaderOf(ResponseHeader.XCorrelationID);
                    if (correlationId == null)
                    {
                        Logger.Warn("Client Consumer: Cannot complete response because no correlation id.");
                        _state.Configuration.ConsumerOfUnknownResponses.Consume(response);
                    }
                    else
                    {
                        if (_completables.TryGetValue(correlationId?.Value!, out var completes))
                        {
                            if (!_state.Configuration.KeepAlive)
                            {
                                _completables.Remove(correlationId?.Value!);
                            }
                        }

                        if (completes == null)
                        {
                            _state.Configuration.Stage.World.DefaultLogger.Warn(
                                $"Client Consumer: Cannot complete response because mismatched correlation id: {correlationId?.Value}");
                            _state.Configuration.ConsumerOfUnknownResponses.Consume(response);
                        }
                        else
                        {
                            completes.With(response);
                        }
                    }
                }
            }
            finally
            {
                buffer.Release();
            }
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
            => _state.Channel.ProbeChannel();

        public ICompletes<Response> RequestWith(Request request, ICompletes<Response> completes)
        {
            var correlationId = request.Headers.HeaderOf(RequestHeader.XCorrelationID);

            Request readyRequest;

            if (correlationId == null)
            {
                correlationId = RequestHeader.Of(RequestHeader.XCorrelationID, _randomUuidGenerator.GenerateGuid().ToString());
                readyRequest = request.And(correlationId);
            }
            else
            {
                readyRequest = request;
            }

            _completables[correlationId!.Value!] = Stage.World.CompletesFor(completes);

            _state.Buffer.Clear();
            _state.Buffer.Seek(0, SeekOrigin.Begin);
            var array = Converters.TextToBytes(readyRequest.ToString());
            _state.Buffer.Write(array, 0, array.Length);
            _state.Buffer.Flip();
            _state.Channel.RequestWith(_state.Buffer.GetBuffer());

            return completes;
        }

        public override void Stop()
        {
            _state.Channel.Close();
            _state.Probe.Cancel();
        }
    }
}

// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Wire.Channel;
using Vlingo.Wire.Message;

namespace Vlingo.Http.Resource
{
    public class ClientConsumerWorkerActor : Actor, IClientConsumer
    {
        private const string EmptyTestId = "";
        private static readonly AtomicInteger TestIdGenerator = new AtomicInteger(0);

        private readonly string _testId;
        private readonly IRequestSender _requestSender;

        private ICompletesEventually? _completesEventually;
        private ResponseParser? _parser;

        public ClientConsumerWorkerActor(Client.Configuration configuration)
        {
            _testId = configuration.HasTestInfo
                ? TestIdGenerator.IncrementAndGet().ToString()
                : EmptyTestId;

            _requestSender = StartRequestSender(configuration);
            _parser = null;
        }

        public void Consume(IConsumerByteBuffer buffer)
        {
            if (_parser == null)
            {
                _parser = ResponseParser.ParserFor(buffer);
            }
            else
            {
                _parser.ParseNext(buffer);
            }
            buffer.Release();

            // don't disperse stowed messages unless a full response has arrived
            if (_parser.HasFullResponse())
            {
                var response = _parser.FullResponse();

                if (_testId != EmptyTestId)
                {
                    response.Headers.Add(ResponseHeader.Of(Client.ClientIdCustomHeader, _testId));
                }

                _completesEventually?.With(response);
                _completesEventually = null;
                DisperseStowedMessages();
            }

            if (!_parser.IsMissingContent)
            {
                _parser = null;
            }
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
        }

        public ICompletes<Response> RequestWith(Request request, ICompletes<Response> completes)
        {
            _completesEventually = Stage.World.CompletesFor(completes);

            if (_testId != EmptyTestId)
            {
                request.Headers.Add(RequestHeader.Of(Client.ClientIdCustomHeader, _testId));
                request.Headers.Add(RequestHeader.Of(RequestHeader.XCorrelationID, _testId));
            }

            _requestSender.SendRequest(request);

            StowMessages(typeof(IResponseChannelConsumer));

            return completes;
        }

        public override void Stop()
        {
            _requestSender.Stop();
            base.Stop();
        }

        private IRequestSender StartRequestSender(Client.Configuration configuration)
        {
            var self = SelfAs<IResponseChannelConsumer>();
            
            var requestSender = ChildActorFor<IRequestSender>(() => new RequestSenderProbeActor(configuration, self, _testId));

            return requestSender;
        }
    }
}

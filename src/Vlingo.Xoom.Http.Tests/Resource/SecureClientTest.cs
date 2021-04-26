// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Wire.Channel;
using Vlingo.Xoom.Wire.Message;
using Vlingo.Xoom.Wire.Nodes;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class SecureClientTest : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private string _responseContent;
        private readonly World _world;

        [Fact]
        public void TestThatSecureClientReceivesResponse()
        {
            var responseConsumer = new TestResponseConsumer(_output);
            var access = responseConsumer.AfterCompleting(1);
            var unknown = new UnknownResponseConsumer(access, _output);

            var config =
                Client.Configuration.Secure(
                    _world.Stage,
                    Address.From(Host.Of("google.com"), 443, AddressType.None),
                    unknown,
                    false,
                    10,
                    65535,
                    10,
                    65535);

            config.TestInfo(true);

            var client =
                Client.Using(
                    config,
                    Client.ClientConsumerType.RoundRobin,
                    5);

            var request =
                Request
                    .Has(Method.Get)
                    .And(new Uri("/search?q=vlingo", UriKind.Relative))
                    .And(RequestHeader.WithHost("google.com"))
                    .And(RequestHeader.WithConnection("close"))
                    .And(RequestHeader.WithContentType("text/html"));

            var response = client.RequestWith(request);

            response.AndThenConsume(res => {
                _responseContent = res.Entity.Content;
                access.WriteUsing("response", res);
            });


            Assert.Equal(1, access.ReadFrom<int>("responseCount"));

            var accessResponse = access.ReadFrom<Response>("response");
            
            Assert.Equal(_responseContent, accessResponse.Entity.Content);
        }
        
        [Fact]
        public void TestThatSecureClientReceivesResponseFromRequestProbeActor()
        {
            var clientConsumer = new TestSecureResponseChannelConsumer();
            var access = clientConsumer.AfterCompleting(1);
            var unknown = new UnknownResponseConsumer(access, _output);

            var config =
                Client.Configuration.Secure(
                    _world.Stage,
                    Address.From(Host.Of("webhook.site"), 443, AddressType.None),
                    unknown,
                    false,
                    10,
                    65535,
                    10,
                    65535);

            config.TestInfo(true);

            var requestSender = _world.Stage.ActorFor<IRequestSender>(() => new RequestSenderProbeActor(config, clientConsumer, "1"));

            var get = "GET /4f0931bb-1c2f-4786-a703-a8b86419c03d HTTP/1.1\nHost: webhook.site\nConnection: close\n\n";
            var buffer = BasicConsumerByteBuffer.Allocate(1, 1000);
            buffer.Put(Encoding.UTF8.GetBytes(get));
            buffer.Flip();
            
            requestSender.SendRequest(Request.From(buffer));

            Assert.Equal(1, access.ReadFrom<int>("consumeCount"));

            Assert.Contains("HTTP/1.1 404 Not Found", clientConsumer.GetResponses().First());
        }

        public SecureClientTest(ITestOutputHelper output)
        {
            _output = output;
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.StartWithDefaults("secure-client");
        }

        public void Dispose()
        {
            _world?.Terminate();
        }
    }

    public class TestSecureResponseChannelConsumer : IResponseChannelConsumer
    {
        private AccessSafely _access;
        private readonly List<string> _responses = new List<string>();

        public AtomicInteger ConsumeCount { get; } = new AtomicInteger(0);
        
        public void Consume(IConsumerByteBuffer buffer)
        {
            var responsePart = buffer.ToArray().BytesToText(0, (int)buffer.Limit());
            buffer.Release();
            _access.WriteUsing("responses", responsePart);
        }
        
        public IEnumerable<string> GetResponses() => _access.ReadFrom<IEnumerable<string>>("responses");
        public AccessSafely AfterCompleting(int times)
        {
            _access = AccessSafely.AfterCompleting(times);

            _access.WritingWith<string>("responses", (response) => {
                _responses.Add(response);
                ConsumeCount.IncrementAndGet();
            });

            _access.ReadingWith<IEnumerable<string>>("responses", () => _responses);
            _access.ReadingWith("consumeCount", () => ConsumeCount.Get());

            return _access;
        }
    }
}
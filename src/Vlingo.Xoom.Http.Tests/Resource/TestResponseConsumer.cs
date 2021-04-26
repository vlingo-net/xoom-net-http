// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Resource;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class TestResponseConsumer
    {
        private readonly ITestOutputHelper _logger;
        private readonly Dictionary<string, int> _clientCounts = new Dictionary<string, int>();
        private AccessSafely _access;

        public TestResponseConsumer(ITestOutputHelper logger)
        {
            _logger = logger;
            _access = AfterCompleting(0);
        }

        public AccessSafely AfterCompleting(int happenings)
        {
            _access = AccessSafely.AfterCompleting(happenings);

            _access.WritingWith<Response>("response", response => {
                var testId = response.HeaderValueOr(Client.ClientIdCustomHeader, "");

                _logger.WriteLine("{0:MM/dd/yyyy hh:mm:ss.fff} ID: {1}", DateTimeOffset.Now, testId);

                if (string.IsNullOrEmpty(testId))
                {
                    _logger.WriteLine("Expected header missing: {0}", Client.ClientIdCustomHeader);
                }

                if (_clientCounts.ContainsKey(testId))
                {
                    _clientCounts[testId] += 1;
                }
                else
                {
                    _clientCounts.Add(testId, 1);
                }

                ResponseHolder.Set(response);

                ResponseCount.IncrementAndGet();
            });
            _access.ReadingWith("response", () => ResponseHolder.Get());
            _access.ReadingWith("responseCount", () => ResponseCount.Get());
            _access.ReadingWith("responseClientCounts", () => _clientCounts);

            _access.WritingWith<int>("unknownResponseCount", increment => UnknownResponseCount.IncrementAndGet());
            _access.ReadingWith("unknownResponseCount", () => UnknownResponseCount.Get());

            _access.ReadingWith("totalAllResponseCount", () => ResponseCount.Get() + UnknownResponseCount.Get());

            return _access;
        }
        
        public AtomicReference<Response> ResponseHolder { get; } = new AtomicReference<Response>();
        
        public AtomicInteger ResponseCount { get; } = new AtomicInteger(0);
        
        public AtomicInteger UnknownResponseCount { get; } = new AtomicInteger(0);
    }
    
    public sealed class KnownResponseConsumer : IResponseConsumer
    {
        private readonly AccessSafely _access;

        public KnownResponseConsumer(AccessSafely access) => _access = access;

        public void Consume(Response response) => _access.WriteUsing("response", response);
    }
    
    public sealed class UnknownResponseConsumer : IResponseConsumer
    {
        private readonly AccessSafely _access;
        private readonly ITestOutputHelper _logger;

        public UnknownResponseConsumer(AccessSafely access, ITestOutputHelper logger)
        {
            _access = access;
            _logger = logger;
        }

        public void Consume(Response response)
        {
            _logger.WriteLine("UNKNOWN RESPONSE:\n{0}", response);
            _access.WriteUsing("unknownResponseCount", 1);
        }
    }
}
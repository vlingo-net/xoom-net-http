// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Vlingo.Http.Resource;
using Xunit.Abstractions;

namespace Vlingo.Http.Tests.Resource
{
    public class TestResponseConsumer
    {
        private readonly ITestOutputHelper _logger;
        private readonly Dictionary<string, int> clientCounts = new Dictionary<string, int>();
        private AccessSafely access;

        public TestResponseConsumer(ITestOutputHelper logger)
        {
            _logger = logger;
            access = AfterCompleting(0);
        }

        public AccessSafely AfterCompleting(int happenings) {
            access = AccessSafely.AfterCompleting(happenings);

            access.WritingWith<Response>("response", response => {
                var testId = response.HeaderValueOr(Client.ClientIdCustomHeader, "");

                _logger.WriteLine("ID: {0}", testId);

                if (string.IsNullOrEmpty(testId))
                {
                    _logger.WriteLine("Expected header missing: {0}", Client.ClientIdCustomHeader);
                    //throw new IllegalStateException("Expected header missing: " + Client.ClientIdCustomHeader);
                }

                int existingCount = 0;
                if (clientCounts.ContainsKey(testId))
                {
                    existingCount = clientCounts[testId];
                }

                ResponseHolder.Set(response);

                clientCounts.Add(testId, existingCount + 1);

                ResponseCount.IncrementAndGet();
            });
            access.ReadingWith("response", () => ResponseHolder.Get());
            access.ReadingWith("responseCount", () => ResponseCount.Get());
            access.ReadingWith("responseClientCounts", () => clientCounts);

            access.WritingWith<int>("unknownResponseCount", increment => UnknownResponseCount.IncrementAndGet());
            access.ReadingWith("unknownResponseCount", () => UnknownResponseCount.Get());

            access.ReadingWith("totalAllResponseCount", () => ResponseCount.Get() + UnknownResponseCount.Get());

            return access;
        }
        
        public AtomicReference<Response> ResponseHolder { get; } = new AtomicReference<Response>();
        
        public AtomicInteger ResponseCount { get; } = new AtomicInteger(0);
        
        public AtomicInteger UnknownResponseCount { get; } = new AtomicInteger(0);
    }
}
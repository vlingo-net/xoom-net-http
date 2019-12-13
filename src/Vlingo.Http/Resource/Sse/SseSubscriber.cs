// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Http.Resource.Sse
{
    public class SseSubscriber
    {
        public SseClient? Client { get; }
        public string? CorrelationId { get; }
        public string? CurrentEventId { get; set; }
        public string? StreamName { get; }

        public SseSubscriber(string? streamName, SseClient client, string? correlationId, string? lastEventId)
        {
            StreamName = streamName;
            Client = client;
            CorrelationId = correlationId;
            CurrentEventId = lastEventId;
        }

        public SseSubscriber(string streamName, SseClient client)
            : this(streamName, client, string.Empty, string.Empty)
        {
        }

        public void Close() => Client.Close();

        public bool IsCompatibleWith(string streamName) => string.Equals(StreamName, streamName);

        public bool HasCorrelationId => !string.IsNullOrEmpty(CorrelationId);

        public bool HasCurrentEventId => !string.IsNullOrEmpty(CurrentEventId);

        public string Id => Client.Id;
    }
}

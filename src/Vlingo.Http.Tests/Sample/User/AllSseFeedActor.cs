// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Http.Resource.Sse;

namespace Vlingo.Http.Tests.Sample.User
{
    public class AllSseFeedActor : Actor, ISseFeed
    {
        private int _retryThreshold = 3000;

        private SseEvent.Builder _builder;
        private int _currentStreamId;
        private int _defaultId;
        private int _feedPayload;
        private string _streamName;

        public AllSseFeedActor(string streamName, int feedPayload, string feedDefaultId)
        {
            _streamName = streamName;
            _feedPayload = feedPayload;
            _currentStreamId = 1;
            _defaultId = DefaultId(feedDefaultId, _currentStreamId);
            _builder = SseEvent.Builder.Instance;
            Logger.Info($"SseFeed started for stream: {streamName}");
        }
        
        public void To(ICollection<SseSubscriber> subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                var fresh = !subscriber.HasCurrentEventId;
                var retry = fresh ? _retryThreshold : SseEvent.NoRetry;
                var startId = fresh ? _defaultId : int.Parse(subscriber.CurrentEventId);
                var endId = startId + _feedPayload - 1;
                var (sseEvents, id) = ReadSubStream(startId, endId, retry);
                subscriber.Client.Send(sseEvents);
                subscriber.CurrentEventId = id.ToString();
            }
        }
        
        private int DefaultId(string feedDefaultId, int defaultDefaultId)
        {
            var maybeDefaultId = int.Parse(feedDefaultId);
            return maybeDefaultId <= 0 ? defaultDefaultId : maybeDefaultId;
        }
        
        private (IEnumerable<SseEvent>, int) ReadSubStream(int startId, int endId, int retry)
        {
            var substream = new List<SseEvent>();
            int type = 0;
            int id = startId;
            for ( ; id <= endId; ++id)
            {
                substream.Add(_builder.Clear().WithEvent("mimeType-" + ('A' + type)).WithId(id).WithData("data-" + id).WithRetry(retry).ToEvent());
                type = type > 26 ? 0 : type + 1;
            }

            return (substream, id + 1);
        }
    }
}
// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Actors;
using Vlingo.Common;

namespace Vlingo.Http.Resource.SSE
{
    public class SseStreamResource : ResourceHandler
    {
        private static readonly IDictionary<string, ISsePublisher> publishers = new ConcurrentDictionary<string, ISsePublisher>();

        private readonly World _world;

        public override Resource<T> Routes<T>() => throw new NotSupportedException("Undefined resource; must override.");

        public SseStreamResource(World world)
        {
            _world = world;
        }

        public void SubscribeToStream(string streamName, Type feedClass, int feedPayload, int feedInterval, string feedDefaultId)
        {
            var clientContext = Context.ClientContext;
            clientContext.WhenClosing(UnsubscribeRequest());

            var correlationId = Context.Request.HeaderValueOr(RequestHeader.XCorrelationID, string.Empty);

            var subscriber = new SseSubscriber(
                    streamName,
                    new SseClient(clientContext),
                    correlationId,
                    Context.Request.HeaderValueOr(RequestHeader.LastEventID, string.Empty));

            PublisherFor(streamName, feedClass, feedPayload, feedInterval, feedDefaultId).Subscribe(subscriber);

            Completes.With(Response.Of(Response.ResponseStatus.Ok, ResponseHeader.WithHeaders(ResponseHeader.WithCorrelationId(correlationId))));
        }

        public void UnsubscribeFromStream(string streamName, string id)
        {
            if (publishers.TryGetValue(streamName, out var publisher))
            {
                if (publisher != null)
                {
                    publisher.Unsubscribe(new SseSubscriber(streamName, new SseClient(Context.ClientContext)));
                }
            }

            Completes.With(Response.Of(Response.ResponseStatus.Ok));
        }

        private ISsePublisher PublisherFor(string streamName, Type feedClass, int feedPayload, int feedInterval, string feedDefaultId)
        {
            if (publishers.TryGetValue(streamName, out var publisher))
            {
                if (publisher == null)
                {
                    publisher = _world.ActorFor<ISsePublisher>(
                        Definition.Has<SsePublisherActor>(
                            Definition.Parameters(streamName, feedClass, feedPayload, feedInterval, feedDefaultId)));

                    if (publishers.TryGetValue(streamName, out var presentPublisher) && presentPublisher != null)
                    {
                        publisher.Stop();
                        publisher = presentPublisher;
                    }
                    else
                    {
                        publishers.Add(streamName, publisher);
                    }
                }
            }
            return publisher;
        }

        private Request UnsubscribeRequest()
        {
            try
            {
                var unsubscribePath = Context.Request.Uri.AbsolutePath + "/" + Context.ClientContext.Id;
                return Request.Has(Method.DELETE).And(new Uri(unsubscribePath));
            }
            catch
            {
                return null;
            }
        }

        private class SsePublisherActor : Actor, ISsePublisher, IScheduled<object>, IStoppable
        {
            private readonly Common.ICancellable _cancellable;
            private readonly ISseFeed _feed;
            private readonly string _streamName;
            private readonly IDictionary<string, SseSubscriber> _subscribers;

            public SsePublisherActor(string streamName, Type feedClass, int feedPayload, int feedInterval, string feedDefaultId)
            {
                _streamName = streamName;
                _feed = Stage.ActorFor<ISseFeed>(Definition.Has(feedClass, Definition.Parameters(streamName, feedPayload, feedDefaultId)));
                _subscribers = new Dictionary<string, SseSubscriber>();
                _cancellable = Stage.Scheduler.Schedule(
                    SelfAs<IScheduled<object>>(),
                    null,
                    TimeSpan.FromMilliseconds(10),
                    TimeSpan.FromMilliseconds(feedInterval));

                Logger.Log($"SsePublisher started for: {_streamName}");
            }

            public void Subscribe(SseSubscriber subscriber)
                => _subscribers.Add(subscriber.Id, subscriber);

            public void Unsubscribe(SseSubscriber subscriber)
            {
                subscriber.Close();
                _subscribers.Remove(subscriber.Id);
            }

            public void IntervalSignal(IScheduled<object> scheduled, object data)
                => _feed.To(_subscribers.Values);

            public override void Stop()
            {
                _cancellable.Cancel();
                UnsubscribeAll();
                base.Stop();
            }

            private void UnsubscribeAll()
            {
                foreach (var subscriber in _subscribers.Values.ToList())
                {
                    Unsubscribe(subscriber);
                }
            }
        }
    }
}

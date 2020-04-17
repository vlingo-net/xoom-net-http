// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Vlingo.Wire.Channel;
using Vlingo.Wire.Message;

namespace Vlingo.Http.Tests.Resource.Sse
{
    public class MockResponseSenderChannel : IResponseSenderChannel
    {
        public AtomicInteger AbandonCount { get; }
        public AtomicReference<Response> EventsResponse { get; }
        public AtomicInteger RespondWithCount { get; }
        public AtomicReference<Response> Response { get; }
        
        private AccessSafely _abandonSafely = AccessSafely.AfterCompleting(0);
        private AccessSafely _respondWithSafely;

        private bool _receivedStatus;

        public MockResponseSenderChannel()
        {
            AbandonCount = new AtomicInteger(0);
            EventsResponse = new AtomicReference<Response>();
            RespondWithCount = new AtomicInteger(0);
            Response = new AtomicReference<Response>();
            _respondWithSafely = ExpectRespondWith(0);
            _receivedStatus = false;
        }
        
        public void Abandon(RequestResponseContext context)
        {
            var count = AbandonCount.IncrementAndGet();
            _abandonSafely.WriteUsing("count", count);
        }

        public void ExplicitClose(RequestResponseContext context, bool option)
        {
        }

        public void RespondWith(RequestResponseContext context, IConsumerByteBuffer buffer)
        {
            var parser = _receivedStatus ?
                ResponseParser.ParserForBodyOnly(buffer.ToArray()) :
                ResponseParser.ParserFor(buffer.ToArray());

            if (!_receivedStatus)
            {
                Response.Set(parser.FullResponse());
            }
            else
            {
                _respondWithSafely.WriteUsing("events", parser.FullResponse());
            }
            
            _receivedStatus = true;
        }

        /// <summary>
        /// Answer with an AccessSafely which
        /// writes the abandon call count using "count" every time abandon(...) is called, and
        /// reads the abandon call count using "count".
        /// </summary>
        /// <param name="n">Number of times abandon must be called before readFrom will return</param>
        /// <returns>Access Safely</returns>
        public AccessSafely ExpectAbandon(int n)
        {
            _abandonSafely = AccessSafely.AfterCompleting(n)
                .WritingWith<int>("count", x => { })
                .ReadingWith("count", () => AbandonCount.Get());
            return _abandonSafely;
        }

        /// <summary>
        /// Answer with an AccessSafely which
        /// writes the respondWith call count using "count" every time respondWith(...) is called, and
        /// reads the respondWith call count using "count".
        /// </summary>
        /// <param name="n">Number of times respondWith must be called before readFrom will return.</param>
        /// <returns>Access safely instance</returns>
        public AccessSafely ExpectRespondWith(int n)
        {
            _respondWithSafely = AccessSafely.AfterCompleting(n)
                .WritingWith<Response>("events", response => { RespondWithCount.IncrementAndGet(); EventsResponse.Set(response); } )
                .ReadingWith("count", () => RespondWithCount.Get())
                .ReadingWith("eventsResponse", () => EventsResponse.Get());
            return _respondWithSafely;
        }
    }
}
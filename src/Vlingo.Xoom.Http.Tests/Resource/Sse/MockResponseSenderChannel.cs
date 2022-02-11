// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Wire.Channel;
using Vlingo.Xoom.Wire.Message;

namespace Vlingo.Xoom.Http.Tests.Resource.Sse;

public class MockResponseSenderChannel : IResponseSenderChannel
{
    public AtomicInteger AbandonCount { get; }
    public AtomicReference<Response> EventsResponse { get; }
    public AtomicInteger RespondWithCount { get; }
    public AtomicReference<Response> Response { get; }
        
    private AccessSafely _abandonSafely;
    private AccessSafely _respondWithSafely;

    private bool _receivedStatus;

    public MockResponseSenderChannel()
    {
        AbandonCount = new AtomicInteger(0);
        EventsResponse = new AtomicReference<Response>();
        RespondWithCount = new AtomicInteger(0);
        Response = new AtomicReference<Response>();
        _abandonSafely = ExpectAbandon(0);
        _respondWithSafely = ExpectRespondWith(0);
        _receivedStatus = false;
    }
        
    public void Abandon(RequestResponseContext context)
    {
        var count = AbandonCount.IncrementAndGet();
        _abandonSafely.WriteUsing("count", count);
    }

    public void RespondWith(RequestResponseContext context, IConsumerByteBuffer buffer) =>
        RespondWith(context, buffer, false);

    public void RespondWith(RequestResponseContext context, IConsumerByteBuffer buffer, bool closeFollowing)
    {
        var parser = _receivedStatus ?
            ResponseParser.ParserForBodyOnly(buffer) :
            ResponseParser.ParserFor(buffer);

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

    public void RespondWith(RequestResponseContext context, object response, bool closeFollowing)
    {
        var textResponse = response.ToString();

        var buffer =
            new BasicConsumerByteBuffer(0, textResponse!.Length + 1024)
                .Put(Converters.TextToBytes(textResponse)).Flip();

        var parser = _receivedStatus ?
            ResponseParser.ParserForBodyOnly(buffer) :
            ResponseParser.ParserFor(buffer);

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
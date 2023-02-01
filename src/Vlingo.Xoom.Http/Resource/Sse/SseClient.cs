// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vlingo.Xoom.Wire.Channel;
using Vlingo.Xoom.Wire.Message;

namespace Vlingo.Xoom.Http.Resource.Sse;

public class SseClient
{
    private static readonly Headers<ResponseHeader> Headers;

    static SseClient()
    {
        var cacheControl = ResponseHeader.Of(ResponseHeader.CacheControl, "no-cache");
        var connection = ResponseHeader.Of(ResponseHeader.Connection, "keep-alive");
        var contentType = ResponseHeader.Of(ResponseHeader.ContentType, "text/event-stream;charset=utf-8");
        Headers = Http.Headers.Empty<ResponseHeader>().And(connection).And(contentType).And(cacheControl);
    }

    private readonly StringBuilder _builder;
    private readonly RequestResponseContext? _context;
    private readonly int _maxMessageSize;

    public SseClient(RequestResponseContext? context)
    {
        _context = context;
        _builder = new StringBuilder();
        _maxMessageSize = Configuration.Instance != null ? Configuration.Instance.Sizing.MaxMessageSize : 65535;
            
        SendInitialResponse();
    }

    public void Close() => _context?.Abandon();

    public string? Id => _context?.Id;

    public void Send(SseEvent @event) => Send(@event.Sendable());

    public void Send(params SseEvent[] events) => Send(events.ToList());

    public void Send(IEnumerable<SseEvent> events)
    {
        var entity = Flatten(events);
        Send(entity);
    }

    private void Send(string entity)
    {
        var buffer = BasicConsumerByteBuffer.Allocate(1, _maxMessageSize);
        _context?.RespondWith(buffer.Put(Encoding.UTF8.GetBytes(entity)).Flip());
    }
        
    private void SendInitialResponse()
    {
        try
        {
            var response = Response.Of(ResponseStatus.Ok, Headers.Copy());
            var buffer = BasicConsumerByteBuffer.Allocate(1, _maxMessageSize);
            _context?.RespondWith(response.Into(buffer));
        }
        catch
        {
            // it's possible that I am being used for an unsubscribe
            // where the client has already disconnected and this
            // attempt will fail; ignore it and return.
        }
    }

    private string Flatten(IEnumerable<SseEvent> events)
    {
        _builder.Clear();

        foreach (var @event in events)
        {
            _builder.Append(@event.Sendable());
        }

        return _builder.ToString();
    }
}
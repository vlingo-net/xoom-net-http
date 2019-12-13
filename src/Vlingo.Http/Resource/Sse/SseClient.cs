// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vlingo.Wire.Channel;
using Vlingo.Wire.Message;

namespace Vlingo.Http.Resource.Sse
{
    public class SseClient
    {
        private static readonly ResponseHeader CacheControl;
        private static readonly ResponseHeader Connection;
        private static readonly ResponseHeader ContentType;
        private static readonly Headers<ResponseHeader> Headers;

        static SseClient()
        {
            CacheControl = ResponseHeader.Of(ResponseHeader.CacheControl, "no-cache");
            Connection = ResponseHeader.Of(ResponseHeader.Connection, "keep-alive");
            ContentType = ResponseHeader.Of(ResponseHeader.ContentType, "text/event-stream;charset=utf-8");
            Headers = Http.Headers.Empty<ResponseHeader>().And(Connection).And(ContentType).And(CacheControl);
        }

        private readonly StringBuilder _builder;
        private readonly RequestResponseContext<object>? _context;
        private readonly int _maxMessageSize;

        public SseClient(RequestResponseContext<object>? context)
        {
            _context = context;
            _builder = new StringBuilder();
            _maxMessageSize = Configuration.Instance.Sizing.MaxMessageSize;
            
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
            var response = Response.Of(Response.ResponseStatus.Ok, Headers.Copy());
            var buffer = BasicConsumerByteBuffer.Allocate(1, _maxMessageSize);
            _context?.RespondWith(response.Into(buffer));
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
}

// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Http.Resource.Sse
{
    public class MessageEvent
    {
        public const int NoRetry = -1;

        public readonly string _comment;
        public readonly string _data;
        public readonly string _event;
        public readonly string _id;
        public readonly int _retry;

        private MessageEvent(string id, string @event, string data, int retry, string comment)
        {
            _id = id;
            _event = @event;
            _data = data;
            _retry = retry;
            _comment = comment;
        }

        public static List<MessageEvent> From(Response response)
        {
            var header = response.HeaderOf(ResponseHeader.ContentType);
            if (header == null || !header.Value.Equals("text/event-stream"))
            {
                return new List<MessageEvent>(0);
            }

            var events = new List<MessageEvent>(2);

            var rawContent = response.Entity.Content.Split('\n');
            var startIndex = 0;
            var currentIndex = 0;
            var isEvent = false;

            for (; currentIndex < rawContent.Length; ++currentIndex)
            {
                if (rawContent[currentIndex].Length > 0)
                {
                    if (!isEvent)
                    {
                        isEvent = true;
                        startIndex = currentIndex;
                    }
                }
                else
                {
                    if (isEvent)
                    {
                        isEvent = false;
                        events.Add(EventFrom(rawContent, startIndex, currentIndex - 1));
                        startIndex = 0;
                    }
                }
            }

            if (isEvent)
            {
                events.Add(EventFrom(rawContent, startIndex, currentIndex - 1));
            }

            return events;
        }

        private static MessageEvent EventFrom(string[] rawContent, int startIndex, int endIndex)
        {
            string comment = null;
            string data = null;
            string @event = null;
            string id = null;
            var retry = NoRetry;

            for (var currentIndex = startIndex; currentIndex <= endIndex; ++currentIndex)
            {
                var colon = rawContent[currentIndex].IndexOf(':');
                if (colon > 0)
                {
                    var field = rawContent[currentIndex].Substring(0, colon).Trim();
                    var value = rawContent[currentIndex].Length > (colon + 1) ? rawContent[currentIndex].Substring(colon + 1).Trim() : string.Empty;
                    switch (field)
                    {
                        case "data":
                            data = data == null ? value : data + "\n" + value;
                            break;
                        case "event":
                            @event = value;
                            break;
                        case "id":
                            id = value;
                            break;
                        case "retry":
                            if (int.TryParse(value, out var x))
                            {
                                retry = x;
                            }
                            break;
                    }
                }
                else if (colon == 0)
                {
                    if (rawContent[currentIndex].Length > 0)
                    {
                        comment = rawContent[currentIndex].Substring(1).Trim();
                    }
                }
                else
                {
                    switch (rawContent[currentIndex].Trim())
                    {
                        case "data":
                            break;   // non-data
                        case "event":
                            break;   // non-event
                        case "id":
                            id = ""; // end of stream
                            break;
                        case "retry":
                            retry = NoRetry;
                            break;
                    }
                }
            }

            return new MessageEvent(id, @event, data, retry, comment);
        }

        public bool EndOfStream
            => _id != null && string.Equals(_id, string.Empty);

        public bool HasCommend => !string.IsNullOrEmpty(_comment);

        public bool HasData => !string.IsNullOrEmpty(_data);

        public bool HasEvent => !string.IsNullOrEmpty(_event);

        public bool HasId => !string.IsNullOrEmpty(_id);

        public bool HasRetry => _retry > 0;
    }
}

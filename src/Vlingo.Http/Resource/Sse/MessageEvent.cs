// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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

        public string? Comment { get; }
        public string? Data { get; }
        public string? Event { get; }
        public string? Id { get; }
        public int Retry { get; }

        private MessageEvent(string? id, string? @event, string? data, int retry, string? comment)
        {
            Id = id;
            Event = @event;
            Data = data;
            Retry = retry;
            Comment = comment;
        }

        public static List<MessageEvent> From(Response response)
        {
            var header = response.HeaderOf(ResponseHeader.ContentType);
            if (header == null || header.Value != null && !header.Value.Equals("text/event-stream"))
            {
                // should be confirmed why this is done like that on Java side.
                ;
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
            string? comment = null;
            string? data = null;
            string? @event = null;
            string? id = null;
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
            => Id != null && string.Equals(Id, string.Empty);

        public bool HasCommend => !string.IsNullOrEmpty(Comment);

        public bool HasData => !string.IsNullOrEmpty(Data);

        public bool HasEvent => !string.IsNullOrEmpty(Event);

        public bool HasId => !string.IsNullOrEmpty(Id);

        public bool HasRetry => Retry > 0;
    }
}

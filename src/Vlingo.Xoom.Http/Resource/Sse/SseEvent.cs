// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Text;

namespace Vlingo.Xoom.Http.Resource.Sse;

public class SseEvent
{
    public const int NoRetry = -1;

    public string? Comment { get; }
    public string? Data { get; }
    public string? Event { get; }
    public string? Id { get; }
    public int Retry { get; }

    public SseEvent(string? id, string? @event, string? data, int retry, string? comment)
    {
        Id = id;
        Event = @event;
        Data = data;
        Retry = retry;
        Comment = comment;
    }

    public SseEvent(string id, string @event, string data)
        : this(id, @event, data, NoRetry, null)
    {
    }

    public SseEvent(string id, string @event)
        : this(id, @event, null, NoRetry, null)
    {
    }

    public bool EndOfStream => Id != null && string.Equals(Id, string.Empty);

    public bool HasId => !string.IsNullOrEmpty(Id);

    public string Sendable() => ToString();

    public override string ToString()
    {
        var builder = new StringBuilder();

        if (Comment != null)
        {
            builder.Append(": ").Append(Flatten(Comment)).Append('\n');
        }
        if (Id != null)
        {
            if (Id.Length > 0)
            {
                builder.Append("id: ").Append(Flatten(Id)).Append('\n');
            }
            else
            {
                builder.Append("id").Append('\n'); // end of stream
            }
        }
        if (Event != null)
        {
            builder.Append("event: ").Append(Flatten(Event)).Append('\n');
        }
        if (Data != null)
        {
            foreach (var value in Data.Split('\n'))
            {
                builder.Append("data: ").Append(value).Append('\n');
            }
        }
        if (Retry >= 0)
        {
            builder.Append("retry: ").Append(Retry).Append('\n');
        }

        return builder.Append('\n').ToString();
    }

    private string Flatten(string text)
        => text.Replace("\n", "");

    public class Builder
    {
        private string? _comment;
        private string? _data;
        private string? _event;
        private string? _id;
        private int _retry = NoRetry;

        private Builder()
        {
        }

        public static Builder Instance => new Builder();

        public Builder Clear()
        {
            _comment = null;
            _data = null;
            _event = null;
            _id = null;
            _retry = NoRetry;
            return this;
        }

        public Builder WithComment(string comment)
        {
            _comment = comment;
            return this;
        }

        public Builder WithData(string data)
        {
            _data = data;
            return this;
        }

        public Builder WithEndOfStream()
        {
            _id = string.Empty;
            return this;
        }

        public Builder WithEvent(string @event)
        {
            _event = @event;
            return this;
        }

        public Builder WithId(string id)
        {
            _id = id;
            return this;
        }

        public Builder WithId(int id)
        {
            _id = id.ToString();
            return this;
        }

        public Builder WithId(long id)
        {
            _id = id.ToString();
            return this;
        }

        public Builder WithRetry(int retry)
        {
            _retry = retry;
            return this;
        }

        public SseEvent ToEvent() => new SseEvent(_id, _event, _data, _retry, _comment);
    }
}
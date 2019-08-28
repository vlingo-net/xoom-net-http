// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Text;

namespace Vlingo.Http.Resource.Sse
{
    public class SseEvent
    {
        public const int NoRetry = -1;

        public readonly string _comment;
        public readonly string _data;
        public readonly string _event;
        public readonly string _id;
        public readonly int _retry;

        public SseEvent(string id, string @event, string data, int retry, string comment)
        {
            _id = id;
            _event = @event;
            _data = data;
            _retry = retry;
            _comment = comment;
        }

        public SseEvent(string id, string @event, string data)
            : this(id, @event, data, NoRetry, null)
        {
        }

        public SseEvent(string id, string @event)
            : this(id, @event, null, NoRetry, null)
        {
        }

        public bool EndOfStream => _id != null && string.Equals(_id, string.Empty);

        public bool HasId => string.IsNullOrEmpty(_id);

        public string Sendable() => ToString();

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (_comment != null)
            {
                builder.Append(": ").Append(Flatten(_comment)).Append('\n');
            }
            if (_id != null)
            {
                if (_id.Length > 0)
                {
                    builder.Append("id: ").Append(Flatten(_id)).Append('\n');
                }
                else
                {
                    builder.Append("id").Append('\n'); // end of stream
                }
            }
            if (_event != null)
            {
                builder.Append("event: ").Append(Flatten(_event)).Append('\n');
            }
            if (_data != null)
            {
                foreach (var value in _data.Split('\n'))
                {
                    builder.Append("data: ").Append(value).Append('\n');
                }
            }
            if (_retry >= 0)
            {
                builder.Append("retry: ").Append(_retry).Append('\n');
            }

            return builder.Append('\n').ToString();
        }

        private string Flatten(string text)
            => text.Replace("\n", "");

        public class Builder
        {
            private string _comment;
            private string _data;
            private string _event;
            private string _id;
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
}

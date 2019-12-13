// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Http
{
    public class Headers<T> : IList<T> where T : Header
    {
        private readonly List<T> _list;

        public T? HeaderOf(string name)
        {
            foreach(var header in _list)
            {
                if(string.Equals(name, header.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return header;
                }
            }

            return null;
        }

        public Headers<T> And(Headers<T> headers)
        {
            _list.AddRange(headers);
            return this;
        }

        public Headers<T> And(T header)
        {
            _list.Add(header);
            return this;
        }

        public Headers<T> And(string name, string value)
        {
            var header = (T)Activator.CreateInstance(typeof(T), name, value);
            return And(header);
        }

        public Headers<T> Copy()
        {
            var headers = new Headers<T>(_list.Count);
            var array = new T[_list.Count];
            _list.CopyTo(array);

            foreach(var header in array)
            {
                headers.And(header);
            }

            return headers;
        }

        internal Headers(int capacity)
        {
            _list = new List<T>(capacity);
        }

        public T this[int index] { get => _list[index]; set => throw new NotSupportedException(); }

        public int Count => _list.Count;

        public bool IsReadOnly => true;

        public void Add(T item) => _list.Add(item);

        public void Clear() => _list.Clear();

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        public int IndexOf(T item) => _list.IndexOf(item);

        public void Insert(int index, T item) => throw new NotSupportedException();

        public bool Remove(T item) => throw new NotSupportedException();

        public void RemoveAt(int index) => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        public override string ToString() => string.Join("\n", _list.Select(item => item.ToString()));
    }

    public static class Headers
    {
        public static Headers<T> Empty<T>() where T : Header
            => new Headers<T>(0);

        public static Headers<T> Of<T>(params T[] requestHeaders) where T : Header
        {
            var headers = new Headers<T>(requestHeaders.Length);
            foreach (var requestHeader in requestHeaders)
            {
                headers.Add(requestHeader);
            }

            return headers;
        }
    }
}

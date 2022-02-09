// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Xoom.Http.Resource
{
    public class Actions
    {
        private int _currentId;
        private readonly IList<Action> _actions;

        public static Actions CanBe(string method, string uri, string to)
            => new Actions(method, uri, to, null);

        public static Actions CanBe(string method, string uri, string to, string mapper)
            => new Actions(method, uri, to, mapper);

        public Actions Also(string method, string uri, string to)
        {
            _actions.Add(new Action(_currentId++, method, uri, to, null));
            return this;
        }

        public Actions Also(string method, string uri, string to, string mapper)
        {
            _actions.Add(new Action(_currentId++, method, uri, to, mapper));
            return this;
        }

        public IList<Action> ThatsAll() => new ArraySegment<Action>(_actions.ToArray());

        private Actions(string method, string uri, string to, string? mapper)
        {
            _actions = new List<Action>
            {
                new Action(_currentId++, method, uri, to, mapper)
            };
        }
    }
}

// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Xoom.Http
{
    public class QueryParameters
    {
        private readonly IDictionary<string, IList<string>> _allParameters;

        public QueryParameters(string? query) => _allParameters = ParseQuery(query);

        public ICollection<string> Names => _allParameters.Keys;

        public IReadOnlyList<string>? ValuesOf(string name)
        {
            if (!_allParameters.ContainsKey(name))
            {
                return null;
            }

            return new ArraySegment<string>(_allParameters[name].ToArray());
        }

        public bool ContainsKey(string name) => _allParameters.ContainsKey(name);

        private static IDictionary<string, IList<string>> ParseQuery(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new Dictionary<string, IList<string>>(0);
            }

            try
            {
                var parameters = query?.Replace("?", string.Empty) .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                var queryParameters = new Dictionary<string, IList<string>>(parameters!.Length);

                foreach (var parameter in parameters)
                {
                    var equalSign = parameter.IndexOf('=');
                    var name = equalSign > 0
                        ? Uri.UnescapeDataString(parameter.Substring(0, equalSign))
                        : parameter;
                    var value = equalSign > 0 && parameter.Length > equalSign + 1
                        ? Uri.UnescapeDataString(parameter.Substring(equalSign + 1))
                        : null;

                    if (!queryParameters.ContainsKey(name))
                    {
                        queryParameters[name] = new List<string>();
                    }
                    queryParameters[name].Add(value!);
                }

                return queryParameters;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Query parameters invalid: {query}", ex);
            }
        }
    }
}

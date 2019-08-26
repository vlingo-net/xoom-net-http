// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vlingo.Http.Resource
{
    public sealed class HttpProperties
    {
        private const string PropertiesFile = "vlingo-http.properties";

        private static Func<HttpProperties> _factory = () => Open();

        private static Lazy<HttpProperties> SingleInstance => new Lazy<HttpProperties>(_factory, true);
        
        private readonly IDictionary<string, string> _dictionary;

        public static HttpProperties Instance => SingleInstance.Value;
        
        public static HttpProperties OpenForTest(Dictionary<string, string> properties) => new HttpProperties(properties);

        public static HttpProperties Open()
        {
            var props = new HttpProperties(new Dictionary<string, string>());
            props.Load(new FileInfo(PropertiesFile));
            return props;
        }

        private HttpProperties(Dictionary<string, string> properties)
        {
            _dictionary = properties;
        }

        private string GetProperty(string key) => GetProperty(key, null);

        private string GetProperty(string key, string defaultValue)
        {
            if(_dictionary.TryGetValue(key, out string value))
            {
                return value;
            }

            return defaultValue;
        }
        
        private void SetProperty(string key, string value)
        {
            _dictionary[key] = value;
        }
        
        private void Load(FileInfo configFile)
        {
            foreach(var line in File.ReadAllLines(configFile.FullName))
            {
                if(string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                {
                    continue;
                }

                var items = line.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                var key = items[0].Trim();
                var val = string.Join("=", items.Skip(1)).Trim();
                SetProperty(key, val);
            }
        }

        private string Key(string nodeName, string key)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                return key;
            }

            return $"node.{nodeName}.{key}";
        }
    }
}

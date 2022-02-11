// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http.Resource;

public sealed class HttpProperties : ConfigurationProperties
{
    private static IDictionary<string, string> _properties = new Dictionary<string, string>();
        
    private static Func<HttpProperties> Factory = () =>
    {
        var props = new HttpProperties(_properties);
        props.Load(new FileInfo("vlingo-http.json"));
        return props;
    };

    private static Lazy<HttpProperties> SingleInstance = new Lazy<HttpProperties>(Factory, true);

    public static HttpProperties Instance
    {
        get
        {
            if (_properties.Any())
            {
                SingleInstance.Value.UpdateCustomProperties(_properties);
                _properties.Clear();
            }
                
            return SingleInstance.Value;
        }
    }
        
    public void SetCustomProperties(IDictionary<string, string> properties)
    {
        _properties = properties;
        UpdateCustomProperties(_properties);
    }

    private HttpProperties(IDictionary<string, string> properties)
    {
        _properties = properties;
    }
        
    private void UpdateCustomProperties(IDictionary<string, string> properties)
    {
        foreach (var property in properties)
        {
            SetProperty(property.Key, property.Value);
        }
    }
}
// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;

namespace Vlingo.Http.Resource
{
    public static class HttpProperties
    {
        private const string PropertiesFile = "vlingo-http.properties";

        private static Actors.Properties _properties;

        static HttpProperties()
        {
            _properties = LoadProperties();
        }

        static Actors.Properties LoadProperties()
        {
            var props = new Actors.Properties();
            try
            {
                props.Load(new FileInfo(PropertiesFile));
            }
            catch (Exception)
            {
                throw new ApplicationException("Must provide properties file on classpath: /" + PropertiesFile);
            }

            return props;
        }
    }
}

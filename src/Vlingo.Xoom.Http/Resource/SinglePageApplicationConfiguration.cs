// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Http.Resource
{
    public class SinglePageApplicationConfiguration
    {
        private readonly string _rootPath;
        private readonly string _contextPath;

        private SinglePageApplicationConfiguration() : this("/frontend", "/app")
        {
        }

        private SinglePageApplicationConfiguration(string rootPath, string contextPath)
        {
            _rootPath = rootPath;
            _contextPath = contextPath;
        }

        public string RootPath => _rootPath;

        public string ContextPath => _contextPath;

        public static SinglePageApplicationConfiguration Define() => new SinglePageApplicationConfiguration();

        public static SinglePageApplicationConfiguration DefineWith(string rootPath, string contextPath) =>
            new SinglePageApplicationConfiguration(rootPath, contextPath);
    }
}
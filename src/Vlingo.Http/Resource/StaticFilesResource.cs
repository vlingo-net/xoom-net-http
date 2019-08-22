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
    public class StaticFilesResource : ResourceHandler
    {
        private string _rootPath;

        public StaticFilesResource()
        {
        }

        public void ServeFile(string contentFile, string root, string validSubPaths)
        {
            if (_rootPath == null)
            {
                var slash = root.EndsWith("/") ? "" : "/";
                _rootPath = root + slash;
            }

            var contentPath = _rootPath + Context.Request.Uri;

            try
            {
                var fileContent = ReadFile(contentPath);
                Completes.With(Response.Of(Response.ResponseStatus.Ok, Body.From(fileContent, Body.Encoding.UTF8).Content));
            }
            catch (IOException)
            {
                Completes.With(Response.Of(Response.ResponseStatus.InternalServerError));
            }
            catch (ArgumentException)
            {
                Completes.With(Response.Of(Response.ResponseStatus.NotFound));
            }
        }

        private byte[] ReadFile(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            throw new ArgumentException("File not found.");
        }
    }
}

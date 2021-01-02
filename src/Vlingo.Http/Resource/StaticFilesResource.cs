// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Text;

namespace Vlingo.Http.Resource
{
    public class StaticFilesResource : ResourceHandler
    {
        private string? _rootPath;

        public void ServeFile(string contentFile, string root, string validSubPaths)
        {
            if (_rootPath == null)
            {
                var initialSlash = root.StartsWith("/") ? "" : "/";
                _rootPath = initialSlash + (root.EndsWith("/") ? root.Substring(0, root.Length - 1) : root);
            }

            var uri = string.IsNullOrEmpty(contentFile) ? "/index.html" : Context?.Request?.Uri?.AbsolutePath;
            var contentPath = ContentFilePath(_rootPath + uri);

            try
            {
                var fileContent = ReadFile(contentPath);
                Completes?.With(Response.Of(Response.ResponseStatus.Ok, Body.From(fileContent, Body.Encoding.UTF8).Content));
            }
            catch (IOException)
            {
                Completes?.With(Response.Of(Response.ResponseStatus.InternalServerError));
            }
            catch (ArgumentException)
            {
                Completes?.With(Response.Of(Response.ResponseStatus.NotFound));
            }
        }
        
        private string ContentFilePath(string path)
        {
            var fileSystemPath = $@"{path}";
            var maybeContent = File.GetAttributes(fileSystemPath);

            if (maybeContent.HasFlag(FileAttributes.Directory))
            {
                var builder = new StringBuilder(fileSystemPath);

                if (!path.EndsWith("/"))
                {
                    builder.Append("/");
                }

                builder.Append("index.html");

                return builder.ToString();
            }

            return path;
        }

        private byte[] ReadFile(string? path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            throw new ArgumentException("File not found.");
        }
    }
}

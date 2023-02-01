// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.StaticFiles;

namespace Vlingo.Xoom.Http.Resource;

/// <summary>
/// Serves static file resources. Note that the current limit of 2GB file sizes.
/// </summary>
public class StaticFilesResource : ResourceHandler
{
    private string? _rootPath;
    private Assembly? _assembly;

    /// <summary>
    /// Completes with <code>Ok</code> and the file content or <code>NotFound</code>.
    /// </summary>
    /// <param name="contentFile">The name of the content file to be served</param>
    /// <param name="root">The root path of the static content</param>
    /// <param name="validSubPaths">The indicating the valid file paths under the root</param>
    public void ServeFile(string contentFile, string root, string validSubPaths)
    {
        if (string.IsNullOrWhiteSpace(_rootPath))
        {
            var slash = root.EndsWith("/") ? "" : "/";
            _rootPath = root + slash;
        }

        var uri = string.IsNullOrEmpty(contentFile) ? "/index.html" : Context?.Request?.Uri?.AbsolutePath;
        var contentPath = ContentFilePath(_rootPath + uri);

        try
        {
            // try to read from disk first
            if (FileExists(contentFile))
            {
                var fileContent = ReadFile(contentPath);
                Completes?.With(Response.Of(ResponseStatus.Ok, Body.From(fileContent, Body.Encoding.UTF8).Content));
            }
            else // than from embedded resource
            {
                _rootPath = root.EndsWith("/") ? root.Substring(0, root.Length - 1) : root;
                uri = string.IsNullOrEmpty(contentFile) ? "/index.html" :  Context?.Request?.Uri?.AbsolutePath;
                    
                _assembly ??= EmbeddedResourceLoader.LoadFromPath(root);
                    
                var response = new List<string>
                    {
                        $"{_rootPath}{uri}",
                        $"{WithIndexHtmlAppended(_rootPath + uri)}"
                    }
                    .Select(EmbeddedResourceLoader.CleanPath)
                    .Where(IsValidFilename)
                    .Take(1)
                    .Select(FileResponse)
                    .DefaultIfEmpty(NotFound());
                    
                Completes?.With(response.First());
            }
        }
        catch (IOException)
        {
            Completes?.With(Response.Of(ResponseStatus.InternalServerError));
        }
        catch (ArgumentException)
        {
            Completes?.With(Response.Of(ResponseStatus.NotFound));
        }
    }

    public bool IsValidFilename(string path)
    {
        var containsABadCharacter = new Regex($"[{Regex.Escape(new string(Path.GetInvalidPathChars()))}]");
        if (containsABadCharacter.IsMatch(path))    
        {
            return false;
        }

        using var contentStream = _assembly?.GetManifestResourceStream(path);
        if (contentStream != null && contentStream.Length > 0)
        {
            return true;
        }

        return false;
    }
        
    private string WithIndexHtmlAppended(string path)
    {
        var builder = new StringBuilder(path);

        if (!path.EndsWith("/"))
        {
            builder.Append("/");
        }

        builder.Append("index.html");

        return builder.ToString();
    }
        
    private byte[] ReadManifestFile(string path)
    {
        using var contentStream = _assembly?.GetManifestResourceStream(path);
        if (contentStream != null && contentStream.Length > 0)
        {
            var content = new byte[contentStream.Length];
            contentStream.Read(content, 0, content.Length);
            return content;
        }

        throw new FileNotFoundException($"File '{path}' not found.");
    }
        
    private string GuessContentType(string path)
    {
        new FileExtensionContentTypeProvider().TryGetContentType(path, out string contentType);
        return contentType ?? "application/octet-stream";
    }
        
    private Response FileResponse(string path)
    {
        try
        {
            var fileContent = ReadManifestFile(path);
            return Response.Of(
                ResponseStatus.Ok,
                Headers.Of(
                    ResponseHeader.Of(RequestHeader.ContentType, GuessContentType(path)),
                    ResponseHeader.Of(ResponseHeader.ContentLength, fileContent.Length)),
                Body.From(fileContent, Body.Encoding.UTF8).Content);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception e)
        {
            return InternalServerError(e);
        }
    }

    private Response InternalServerError(Exception e)
    {
        Logger?.Error($"Internal server error because: {e.Message}", e);
        return Response.Of(ResponseStatus.InternalServerError);
    }

    private Response NotFound() => Response.Of(ResponseStatus.NotFound);

    private string ContentFilePath(string path)
    {
        var fileSystemPath = Path.GetFullPath($@"{path.Replace("%20", " ")}");

        try
        {
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
                
        }
        catch
        {
            // just move on
        }
            
        return path;
    }

    private bool FileExists(string? path) => File.Exists(path);

    private byte[] ReadFile(string? path)
    {
        if (FileExists(path))
        {
            return File.ReadAllBytes(path!);
        }
        throw new ArgumentException("File not found.");
    }
}
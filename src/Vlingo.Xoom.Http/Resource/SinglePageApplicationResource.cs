// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http.Resource;

public class SinglePageApplicationResource : ResourceHandler
{
    private readonly string _contextPath;
    private readonly string _indexPagePath;
    private readonly FileExtensionContentTypeProvider _mimeMap = new FileExtensionContentTypeProvider();
    private readonly string _rootPath;

    public SinglePageApplicationResource() : this("/frontend", "/app")
    {
    }

    public SinglePageApplicationResource(string rootPath) : this(rootPath, "/app")
    {
    }

    public SinglePageApplicationResource(string rootPath, string contextPath)
    {
        _rootPath = rootPath;
        _indexPagePath = $"{rootPath}/index.html";
        _contextPath = contextPath;
    }

    public override Resource Routes()
    {
        RequestHandler0.Handler0 serve0 = () => Serve();
        RequestHandler1<string>.Handler1 serve1 = p1 => Serve(p1);
        RequestHandler2<string, string>.Handler2 serve2 = (p1, p2) => Serve(p1, p2);
        RequestHandler3<string, string, string>.Handler3 serve3 = (p1, p2, p3) => Serve(p1, p2, p3);
        RequestHandler4<string, string, string, string>.Handler4 serve4 = (p1, p2, p3, p4) => Serve(p1, p2, p3, p4);
        RequestHandler5<string, string, string, string, string>.Handler5 serve5 = (p1, p2, p3, p4, p5) => Serve(p1, p2, p3, p4, p5);
        RequestHandler6<string, string, string, string, string, string>.Handler6 serve6 = (p1, p2, p3, p4, p5, p6) => Serve(p1, p2, p3, p4, p5, p6);
        RequestHandler7<string, string, string, string, string, string, string>.Handler7 serve7 = (p1, p2, p3, p4, p5, p6, p7) => Serve(p1, p2, p3, p4, p5, p6, p7);
        RequestHandler8<string, string, string, string, string, string, string, string>.Handler8 serve8 = (p1, p2, p3, p4, p5, p6, p7, p8) => Serve(p1, p2, p3, p4, p5, p6, p7, p8);

        return ResourceBuilder.Resource("ui", 10,
            ResourceBuilder.Get("/")
                .Handle(RedirectToApp),
            ResourceBuilder.Get($"{_contextPath}/")
                .Handle(serve0),
            ResourceBuilder.Get($"{_contextPath}/{{file}}")
                .Param<string>()
                .Handle(serve1),
            ResourceBuilder.Get($"{_contextPath}/{{path1}}/{{file}}")
                .Param<string>()
                .Param<string>()
                .Handle(serve2),
            ResourceBuilder.Get($"{_contextPath}/{{path1}}/{{path2}}/{{file}}")
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Handle(serve3),
            ResourceBuilder.Get($"{_contextPath}/{{path1}}/{{path2}}/{{path3}}/{{file}}")
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Handle(serve4),
            ResourceBuilder.Get($"{_contextPath}/{{path1}}/{{path2}}/{{path3}}/{{path4}}/{{file}}")
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Handle(serve5),
            ResourceBuilder.Get($"{_contextPath}/{{path1}}/{{path2}}/{{path3}}/{{path4}}/{{path5}}/{{file}}")
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Handle(serve6),
            ResourceBuilder.Get($"{_contextPath}/{{path1}}/{{path2}}/{{path3}}/{{path4}}/{{path5}}/{{path6}}/{{file}}")
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Handle(serve7),
            ResourceBuilder.Get($"{_contextPath}/{{path1}}/{{path2}}/{{path3}}/{{path4}}/{{path5}}/{{path6}}/{{path7}}/{{file}}")
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Param<string>()
                .Handle(serve8)
        );
    }

    private ICompletes<Response> RedirectToApp()
    {
        return Common.Completes.WithSuccess(
            Response.Of(
                ResponseStatus.MovedPermanently,
                Headers.Of(ResponseHeader.Of(RequestHeader.ContentLength, "0"),
                    ResponseHeader.Of("Location", $"{_contextPath}/"))
            )
        );
    }

    private ICompletes<Response> Serve(params string[] pathSegments)
    {
        var path = EmbeddedResourceLoader.CleanPath($"{_rootPath}/{string.Join("/", pathSegments)}");
        if (path.IndexOf("/static", StringComparison.Ordinal) != -1)
        {
            path = _rootPath + path.Substring(path.IndexOf("/static", StringComparison.Ordinal));
        }
                
        var assembly = EmbeddedResourceLoader.LoadFromPath(path);
        var contentStream = assembly.GetManifestResourceStream(path);
        string? contentType = null;
        if (contentStream == null || path.Equals(_rootPath))
        {
            path = EmbeddedResourceLoader.CleanPath(_indexPagePath);
            contentStream = EmbeddedResourceLoader.LoadFromPath(path).GetManifestResourceStream(path);
            contentType = "text/html";
        }

        if (contentStream == null)
        {
            return Common.Completes.WithFailure(Response.Of(ResponseStatus.NotFound));
        }

        if (contentType == null)
        {
            contentType = GuessContentType(path);
        }

        try
        {
            byte[] content = Read(contentStream); // TODO: implement caching
            return Common.Completes.WithSuccess(
                Response.Of(
                    ResponseStatus.Ok,
                    Headers.Of(ResponseHeader.Of(ResponseHeader.ContentType, contentType),
                        ResponseHeader.Of(ResponseHeader.ContentLength, content.Length)),
                    Body.BytesToUTF8(content)
                )
            );
        }
        catch (Exception e)
        {
            Logger?.Error("Failed to read UI Resource", e);
            return Common.Completes.WithFailure(Response.Of(ResponseStatus.InternalServerError));
        }
    }

    private string GuessContentType(string path)
    {
        _mimeMap.TryGetContentType(path, out string contentType);
        return contentType ?? "application/octet-stream";
    }

    private static byte[] Read(Stream? stream)
    {
        if (stream != null && stream.Length > 0)
        {
            var content = new byte[stream.Length];
            stream.Read(content, 0, content.Length);
            return content;
        }

        return new byte[0];
    }
}
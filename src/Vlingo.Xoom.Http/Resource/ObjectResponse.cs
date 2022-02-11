// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text;
using Vlingo.Xoom.Http.Media;

namespace Vlingo.Xoom.Http.Resource;

public interface IObjectResponse
{
    Response ResponseFrom(Request request, MediaTypeMapper mapper);
}

public class ObjectResponse<T> : IObjectResponse
{
    private static readonly ContentMediaType DefaultMediaType = ContentMediaType.Json;

    private readonly Version _version;
    private readonly ResponseStatus _status;
    private readonly Headers<ResponseHeader> _headers;
    private readonly T _entity;

    private ObjectResponse(Version version,
        ResponseStatus status,
        Headers<ResponseHeader> headers,
        T entity)
    {
        _version = version;
        _status = status;
        _headers = headers;
        _entity = entity;
    }

    public static IObjectResponse Of<TR>(
        Version version,
        ResponseStatus status,
        Headers<ResponseHeader> headers,
        TR entity)
        => new ObjectResponse<TR>(version, status, headers, entity);

    public static IObjectResponse Of<TR>(
        ResponseStatus status,
        Headers<ResponseHeader> headers,
        TR entity)
        => new ObjectResponse<TR>(Version.Http1_1, status, headers, entity);

    public static IObjectResponse Of<TR>(
        ResponseStatus status,
        TR entity)
        => new ObjectResponse<TR>(Version.Http1_1, status, Headers.Empty<ResponseHeader>(), entity);

    public Response ResponseFrom(Request request, MediaTypeMapper mapper)
    {
        var acceptedMediaTypes = request.HeaderValueOr(RequestHeader.Accept, DefaultMediaType.ToString());
        var responseMediaTypeSelector = new ResponseMediaTypeSelector(acceptedMediaTypes);
        var responseContentMediaType = responseMediaTypeSelector.SelectType(mapper.MappedMediaTypes);
        var bodyContent = mapper.From(_entity, responseContentMediaType);
        var body = Body.From(bodyContent);
        _headers.Add(ResponseHeader.Of(ResponseHeader.ContentType, responseContentMediaType.ToString()));

        return Response.Of(_version, _status, _headers, body);
    }

    public override string ToString() => Into(new StringBuilder().Append);

    private string Into<TR>(Func<string, TR> appender)
    {
        // TODO: currently supports only HTTP/1.1
        appender(Version.HTTP_1_1);
        appender(" ");
        appender(_status.ToString());
        appender("\n");

        AppendAllHeadersTo(appender);
        appender("\n");
        return appender(_entity?.ToString() ?? string.Empty)?.ToString() ?? string.Empty;
    }

    private void AppendAllHeadersTo<TR>(Func<string, TR> appender)
    {
        foreach (ResponseHeader header in _headers)
        {
            appender(header.Name);
            appender(": ");
            appender(header.Value ?? string.Empty);
            appender("\n");
        }
    }
}
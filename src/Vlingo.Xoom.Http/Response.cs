// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Vlingo.Xoom.Wire.Message;

namespace Vlingo.Xoom.Http;

public class Response
{
    public static Response Of(ResponseStatus statusCode)
        => new Response(Version.Http1_1, statusCode, Http.Headers.Empty<ResponseHeader>(), Body.Empty);

    public static Response Of(Version version, ResponseStatus statusCode)
        => new Response(version, statusCode, Http.Headers.Empty<ResponseHeader>(), Body.Empty);

    public static Response Of(ResponseStatus statusCode, string entity)
        => new Response(Version.Http1_1, statusCode, Http.Headers.Empty<ResponseHeader>(), Body.From(entity));
        
    public static Response Of(ResponseStatus statusCode, byte[] entity)
        => new Response(Version.Http1_1, statusCode, Http.Headers.Empty<ResponseHeader>(), Body.From(entity));

    public static Response Of(Version version, ResponseStatus statusCode, string entity)
        => new Response(version, statusCode, Http.Headers.Empty<ResponseHeader>(), Body.From(entity));

    public static Response Of(Version version, ResponseStatus statusCode, byte[] entity)
        => new Response(version, statusCode, Http.Headers.Empty<ResponseHeader>(), Body.From(entity));

    public static Response Of(ResponseStatus statusCode, Headers<ResponseHeader> headers)
        => new Response(Version.Http1_1, statusCode, headers, Body.Empty);

    public static Response Of(Version version, ResponseStatus statusCode, Headers<ResponseHeader> headers)
        => new Response(version, statusCode, headers, Body.Empty);

    public static Response Of(ResponseStatus statusCode, Headers<ResponseHeader> headers, string entity)
        => new Response(Version.Http1_1, statusCode, headers, Body.From(entity));

    public static Response Of(ResponseStatus statusCode, Headers<ResponseHeader> headers, byte[] entity)
        => new Response(Version.Http1_1, statusCode, headers, Body.From(entity));

    public static Response Of(Version version, ResponseStatus statusCode, Headers<ResponseHeader> headers, string entity)
        => new Response(version, statusCode, headers, Body.From(entity));
        
    public static Response Of(Version version, ResponseStatus statusCode, Headers<ResponseHeader> headers, byte[] entity)
        => new Response(version, statusCode, headers, Body.From(entity));
        
    public static Response Of(ResponseStatus statusCode, Body body)
        => new Response(Version.Http1_1, statusCode, Http.Headers.Empty<ResponseHeader>(), body);

    public static Response Of(ResponseStatus statusCode, Headers<ResponseHeader> headers, Body entity)
        => new Response(Version.Http1_1, statusCode, headers, entity);

    public static Response Of(Version? version, ResponseStatus statusCode, Headers<ResponseHeader> headers, Body? entity)
        => new Response(version, statusCode, headers, entity);

    public ResponseStatus Status { get; }
    public string StatusCode { get; }
    public Headers<ResponseHeader> Headers { get; }
    public Body Entity { get; }
    public Version? Version { get; }

    protected Response(Version? version, ResponseStatus status, Headers<ResponseHeader> headers, Body? entity)
    {
        Version = version;
        Status = status;
        var statusDescription = status.GetDescription();
        StatusCode = statusDescription.Substring(0, statusDescription.IndexOf(' '));
        Entity = EntityFrom(headers, entity!);
        Headers = AddMissingContentLengthHeader(headers);
    }

    public Header? HeaderOf(string name) => Headers.HeaderOf(name);

    public bool HeaderMatches(string name, string value)
    {
        var header = HeaderOf(name);
        return header?.MatchesValueOf(value) ?? false;
    }
        
    public string HeaderValueOr(string headerName, string defaultValue)
        => Headers.HeaderOf(headerName)?.Value ?? defaultValue;

    public Response Include(Header header)
    {
        if (header != null && HeaderOf(header.Name) == null)
        {
            Headers.And(ResponseHeader.Of(header.Name, header.Value));
        }
        return this;
    }
        
    public Response IncludeAll(IEnumerable<ResponseHeader> headers)
    {
        foreach (var header in headers)
        {
            Include(header);
        }
            
        return this;
    }

    public IConsumerByteBuffer Into(IConsumerByteBuffer consumerByteBuffer)
        => consumerByteBuffer.Put(Converters.TextToBytes(ToString())).Flip();
        
    public int Size
    {
        get
        {
            int headersSize = 0;
            foreach (var header in Headers)
            {
                // name + ": " + value + "\n"
                headersSize += header.Name.Length + 2 + header.Value!.Length + 1;
            }
            // HTTP/1.1 + 1 + status code + "\n" + headers + "\n" + entity + just-in-case
            return 9 + StatusCode.Length + 1 + headersSize + 1 + Entity.Content.Length + 5;
        }
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        // TODO: currently supports only HTTP/1.1

        builder.Append(Version.Http1_1).Append(" ").Append(Status.GetDescription()).Append("\n");
        builder = AppendAllHeadersTo(builder);
        builder.Append("\n").Append(Entity);

        return builder.ToString();
    }

    private Headers<ResponseHeader> AddMissingContentLengthHeader(Headers<ResponseHeader> headers)
    {
        if (!Entity.IsComplex)
        {
            var header = headers.HeaderOf(ResponseHeader.ContentLength);
            if (header == null && !((int)Status).ToString().StartsWith("1") && Status != ResponseStatus.NoContent && Status != ResponseStatus.NotModified)
            {
                headers.Add(ResponseHeader.Of(ResponseHeader.ContentLength, Converters.EncodedLength(Entity.Content)));
            }
        }

        return headers;
    }

    private StringBuilder AppendAllHeadersTo(StringBuilder builder)
    {
        foreach (var header in Headers)
        {
            builder.Append(header.Name).Append(": ").Append(header.Value).Append("\n");
        }
        return builder;
    }
        
    private Body EntityFrom(Headers<ResponseHeader> headers, Body entity)
    {
        var header = headers.HeaderOf(ResponseHeader.TransferEncoding);

        if (header != null && header.Value!.Equals("chunked"))
        {
            if (entity.IsComplex && !entity.HasContent)
            {
                return Body.BeginChunked();
            }
        }

        return entity;
    }
}

public enum ResponseStatus
{
    // 1xx Informational responses
    [Description("100 Continue")]
    Continue = 100,
    [Description("101 Switching Protocols")]
    SwitchingProtocols = 101,
    [Description("102 Processing")]
    Processing = 102,
    [Description("103 Early Hints")]
    EarlyHints = 103,

    // 2xx Success
    [Description("200 OK")]
    Ok = 200,
    [Description("201 Created")]
    Created = 201,
    [Description("202 Accepted")]
    Accepted = 202,
    [Description("203 Non-Authoritative Information")]
    NonAuthoritativeInformation = 203,
    [Description("204 No Content")]
    NoContent = 204,
    [Description("205 Reset Content")]
    ResetContent = 205,
    [Description("206 Partial Content")]
    PartialContent = 206,
    [Description("207 Multi-Status")]
    MultiStatus = 207,
    [Description("208 Already Reported")]
    AlreadyReported = 208,
    [Description("226 IM Used")]
    IMUsed = 226,

    // 3xx Redirection
    [Description("300 Multiple Choices")]
    MultipleChoices = 300,
    [Description("301 Moved Permanently")]
    MovedPermanently = 301,
    [Description("302 Found")]
    Found = 302,
    [Description("303 See Other")]
    SeeOther = 303,
    [Description("304 Not Modified")]
    NotModified = 304,
    [Description("305 Use Proxy")]
    UseProxy = 305,
    [Description("306 Switch Proxy")]
    SwitchProxy = 306,
    [Description("307 Temporary Redirect")]
    TemporaryRedirect = 307,
    [Description("308 Permanent Redirect")]
    PermanentRedirect = 308,

    // 4xx Client errors
    [Description("400 Bad Request")]
    BadRequest = 400,
    [Description("401 Unauthorized")]
    Unauthorized = 401,
    [Description("402 Payment Required")]
    PaymentRequired = 402,
    [Description("403 Forbidden")]
    Forbidden = 403,
    [Description("404 Not Found")]
    NotFound = 404,
    [Description("405 Method Not Allowed")]
    MethodNotAllowed = 405,
    [Description("406 Not Acceptable")]
    NotAcceptable = 406,
    [Description("407 Proxy Authentication Required")]
    ProxyAuthenticationRequired = 407,
    [Description("408 Request Timeout")]
    RequestTimeout = 408,
    [Description("409 Conflict")]
    Conflict = 409,
    [Description("410 Gone")]
    Gone = 410,
    [Description("411 Length Required")]
    LengthRequired = 411,
    [Description("412 Precondition Failed")]
    PreconditionFailed = 412,
    [Description("413 Payload Too Large")]
    PayloadTooLarge = 413,
    [Description("414 URI Too Long")]
    URITooLong = 414,
    [Description("415 Unsupported Media Type")]
    UnsupportedMediaType = 415,
    [Description("416 Range Not Satisfiable")]
    RangeNotSatisfiable = 416,
    [Description("417 Expectation Failed")]
    ExpectationFailed = 417,
    [Description("418 I'm a teapot")]
    Imateapot = 418,
    [Description("421 Misdirected Request")]
    MisdirectedRequest = 421,
    [Description("422 Unprocessable Entity")]
    UnprocessableEntity = 422,
    [Description("423 Locked")]
    Locked = 423,
    [Description("424 Failed Dependency")]
    FailedDependency = 424,
    [Description("426 Upgrade Required")]
    UpgradeRequired = 426,
    [Description("428 Precondition Required")]
    PreconditionRequired = 428,
    [Description("429 Too Many Requests")]
    TooManyRequests = 429,
    [Description("431 Request Header Fields Too Large")]
    RequestHeaderFieldsTooLarge = 431,
    [Description("451 Unavailable For Legal Reasons")]
    UnavailableForLegalReasons = 451,

    // 5xx Server errors
    [Description("500 Internal Server Error")]
    InternalServerError = 500,
    [Description("501 Not Implemented")]
    NotImplemented = 501,
    [Description("502 Bad Gateway")]
    BadGateway = 502,
    [Description("503 Service Unavailable")]
    ServiceUnavailable = 503,
    [Description("504 Gateway Timeout")]
    GatewayTimeout = 504,
    [Description("505 HTTP Version Not Supported")]
    HTTPVersionNotSupported = 505,
    [Description("506 Variant Also Negotiates")]
    VariantAlsoNegotiates = 506,
    [Description("507 Insufficient Storage")]
    InsufficientStorage = 507,
    [Description("508 Loop Detected")]
    LoopDetected = 508,
    [Description("510 Not Extended")]
    NotExtended = 510,
    [Description("511 Network Authentication Required")]
    NetworkAuthenticationRequired = 511
}

public static class ResponseEnumExtension
{
    public static string GetDescription(this ResponseStatus status)
    {
        var type = typeof(ResponseStatus);
        var memberInfo = type.GetMember(status.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attrs != null && attrs.Length > 0)
            {
                //Pull out the description value
                return ((DescriptionAttribute)attrs[0]).Description;
            }
        }
        //If we have no description attribute, just return the ToString of the enum
        return status.ToString();
    }

    internal static ResponseStatus ConvertToResponseStatus(this string statusDescription)
    {
        if (int.TryParse(statusDescription.Substring(0, statusDescription.IndexOf(' ')).Trim(), out var statusCode))
        {
            if (Enum.IsDefined(typeof(ResponseStatus), statusCode))
            {
                return (ResponseStatus)statusCode;
            }
        }

        throw new ArgumentException($"Status {statusDescription} is not valid.");
    }
}
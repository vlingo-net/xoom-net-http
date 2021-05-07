// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Http.Media;

namespace Vlingo.Xoom.Http.Resource
{
    internal interface IParameterResolver
    {
        ParameterResolver.Type Type { get; }
        Type ParamClass { get; }
        object Apply(Request request, Action.MappedParameters mappedParameters);
    }

    internal class ParameterResolver<T> : IParameterResolver
    {
        public ParameterResolver.Type Type { get; }
        public Type ParamClass { get; }
        private readonly Func<Request, Action.MappedParameters, T> _resolver;

        private ParameterResolver(ParameterResolver.Type type, Func<Request, Action.MappedParameters, T> resolver)
        {
            Type = type;
            ParamClass = typeof(T);
            _resolver = resolver;
        }

        object IParameterResolver.Apply(Request request, Action.MappedParameters mappedParameters)
            => _resolver.Invoke(request, mappedParameters)!;

        public T Apply(Request? request, Action.MappedParameters mappedParameters)
            => _resolver.Invoke(request!, mappedParameters);

        internal static ParameterResolver<T> Create(ParameterResolver.Type type, Func<Request, Action.MappedParameters, T> resolver)
            => new ParameterResolver<T>(type, resolver);
    }

    internal static class ParameterResolver
    {
        public static ParameterResolver<T> Path<T>(int position)
            => ParameterResolver<T>.Create(
                Type.Path,
                (request, mappedParameters) =>
                {
                    var value = mappedParameters.Mapped[position].Value;
                    if (value is T)
                    {
                        return (T)value!;
                    }

                    throw new ArgumentException(
                        $"Value {value} is of mimeType {mappedParameters.Mapped[position].Type} instead of {typeof(T).Name}");
                });

        public static ParameterResolver<T> Body<T>()
            => Body<T>(DefaultMediaTypeMapper.Instance);

        public static ParameterResolver<T> Body<T>(IMapper mapper)
            => ParameterResolver<T>.Create(
                Type.Body,
                (request, mappedParameters) => (T)mapper.From(request?.Body?.ToString(), typeof(T))!);

        public static ParameterResolver<T> Body<T>(MediaTypeMapper mediaTypeMapper)
        {
            return typeof(T).IsAssignableFrom(typeof(RequestData)) ? 
                (ParameterResolver<T>)(object) ParameterResolver<RequestData>.Create(Type.Body, (request, mappedParameters) =>
                {
                    // This is a fall-back when content-type not provided for backwards compat for curl/cmd line users
                    var bodyMediaType = BodyMediaTypeOrFallback(request);
                    var contentEncodingHeader = request.Headers.HeaderOfOrDefault(ResponseHeader.ContentEncoding, RequestHeader.WithContentEncoding());
                    var contentEncoding = ContentEncoding.ParseFromHeader(contentEncodingHeader?.Value);
                    return new RequestData(request.Body!, ContentMediaType.ParseFromDescriptor(bodyMediaType), contentEncoding);
                }) :
            ParameterResolver<T>.Create(Type.Body, (request, mappedParameters) =>
                {
                    var bodyMediaType = BodyMediaTypeOrFallback(request);
                    return mediaTypeMapper.From<T>(request.Body?.ToString(), ContentMediaType.ParseFromDescriptor(bodyMediaType));
                });
        }
        
        private static string BodyMediaTypeOrFallback(Request request)
        {
            var assumedBodyContentType = ContentMediaType.Json.ToString();
            return request.HeaderValueOr(RequestHeader.ContentType, assumedBodyContentType);
        }

        public static ParameterResolver<Header> Header(string headerName)
            => ParameterResolver<Header>.Create(
                Type.Header,
                (request, mappedParameters) => request.HeaderOf(headerName)!);

        public static ParameterResolver<T> Query<T>(string name)
            => Query(name, default(T)!);

        public static ParameterResolver<T> Query<T>(string name, T defaultValue)
            => ParameterResolver<T>.Create(
                Type.Query,
                (request, mappedParameters) =>
                {
                    string? value;
                    try
                    {
                        value = request.QueryParameters.ValuesOf(name)![0];
                    }
                    catch (ArgumentException)
                    {
                        return defaultValue;
                    }
                    catch (NullReferenceException)
                    {
                        return defaultValue;
                    }

                    if (typeof(T) == typeof(int))
                    {
                        return (T)(object)int.Parse(value);
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        return (T)(object)value!;
                    }
                    else if (typeof(T) == typeof(float))
                    {
                        return (T)(object)float.Parse(value);
                    }
                    else if (typeof(T) == typeof(long))
                    {
                        return (T)(object)long.Parse(value);
                    }
                    else if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)bool.Parse(value);
                    }
                    else if (typeof(T) == typeof(short))
                    {
                        return (T)(object)short.Parse(value);
                    }
                    else if (typeof(T) == typeof(byte))
                    {
                        return (T)(object)byte.Parse(value);
                    }
                    throw new ArgumentException("unknown mimeType " + typeof(T).Name);
                });

        internal enum Type
        {
            Path,
            Body,
            Header,
            Query
        }
    }
}

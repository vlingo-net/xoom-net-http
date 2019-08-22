// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Http.Media;

namespace Vlingo.Http.Resource
{
    public interface IObjectResponse
    {
        Response ResponseFrom(Request request, MediaTypeMapper mapper);
    }

    public class ObjectResponse<T> : IObjectResponse
    {
        private static readonly ContentMediaType DEFAULT_MEDIA_TYPE = ContentMediaType.Json;

        private readonly Version _version;
        private readonly Response.ResponseStatus _status;
        private readonly Headers<ResponseHeader> _headers;
        private readonly T _entity;
        private readonly Type _class;

        private ObjectResponse(
            Version version,
            Response.ResponseStatus status,
            Headers<ResponseHeader> headers,
            T entity,
            Type @class)
        {
            _version = version;
            _status = status;
            _headers = headers;
            _entity = entity;
            _class = @class;
        }

        public static ObjectResponse<TR> Of<TR>(
            Version version,
            Response.ResponseStatus status,
            Headers<ResponseHeader> headers,
            TR entity,
            Type @class)
            => new ObjectResponse<TR>(version, status, headers, entity, @class);

        public static ObjectResponse<TR> Of<TR>(
            Response.ResponseStatus status,
            Headers<ResponseHeader> headers,
            TR entity,
            Type @class)
            => new ObjectResponse<TR>(Version.Http1_1, status, headers, entity, @class);

        public static ObjectResponse<TR> Of<TR>(
            Response.ResponseStatus status,
            TR entity,
            Type @class)
            => new ObjectResponse<TR>(Version.Http1_1, status, Headers.Empty<ResponseHeader>(), entity, @class);

        public Response ResponseFrom(Request request, MediaTypeMapper mapper)
        {
            var acceptedMediaTypes = request.HeaderValueOr(RequestHeader.Accept, DEFAULT_MEDIA_TYPE.ToString());
            var responseMediaTypeSelector = new ResponseMediaTypeSelector(acceptedMediaTypes);
            var responseContentMediaType = responseMediaTypeSelector.SelectType(mapper.MappedMediaTypes);
            var bodyContent = mapper.From(_entity, responseContentMediaType, _class);
            var body = Body.From(bodyContent);
            _headers.Add(ResponseHeader.Of(ResponseHeader.ContentType, responseContentMediaType.ToString()));

            return Response.Of(_version, _status, _headers, body);
        }
    }
}

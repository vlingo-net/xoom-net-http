// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Http.Media;

namespace Vlingo.Http.Resource
{
    public class MediaTypeMapper
    {
        private readonly IDictionary<ContentMediaType, IMapper> _mappersByContentType;

        public MediaTypeMapper(IDictionary<ContentMediaType, IMapper> mappersByContentType)
        {
            _mappersByContentType = mappersByContentType;
        }

        public T From<T>(string? data, ContentMediaType contentMediaType)
        {
            var baseType = contentMediaType.ToBaseType();
            if (_mappersByContentType.ContainsKey(baseType))
            {
                return (T)_mappersByContentType[baseType].From(data, typeof(T))!;
            }
            throw new MediaTypeNotSupportedException(contentMediaType.ToString());
        }

        public string From<T>(T data, ContentMediaType contentMediaType)
        {
            var baseType = contentMediaType.ToBaseType();
            if (_mappersByContentType.ContainsKey(baseType))
            {
                return _mappersByContentType[baseType].From(data)!;
            }
            throw new MediaTypeNotSupportedException(contentMediaType.ToString());
        }

        public ContentMediaType[] MappedMediaTypes => _mappersByContentType.Keys.ToArray();

        public class Builder
        {
            private IDictionary<ContentMediaType, IMapper> _mappersByContentType;

            public Builder()
            {
                _mappersByContentType = new Dictionary<ContentMediaType, IMapper>();
            }

            public Builder AddMapperFor(ContentMediaType contentMediaType, IMapper mapper)
            {
                if (_mappersByContentType.ContainsKey(contentMediaType))
                {
                    throw new InvalidOperationException("Content mimeType already added");
                }

                _mappersByContentType[contentMediaType] = mapper;

                return this;
            }

            public MediaTypeMapper Build() => new MediaTypeMapper(_mappersByContentType);
        }
    }
}

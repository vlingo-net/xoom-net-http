// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Http.Media;

namespace Vlingo.Xoom.Http.Resource
{
    public static class DefaultMediaTypeMapper
    {
        private static MediaTypeMapper BuildInstance()
            => new MediaTypeMapper.Builder()
                    .AddMapperFor(ContentMediaType.Json, DefaultJsonMapper.Instance)
                    .Build();

        public static MediaTypeMapper Instance { get; } = BuildInstance();
    }
}

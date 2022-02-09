// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http.Resource
{
    public abstract class ResourceHandler
    {
        private Context? _context;
        private Stage? _stage;

        public virtual Resource Routes() => throw new NotSupportedException("Undefined resource; must override.");

        protected ResourceHandler() { }

        protected virtual ICompletesEventually? Completes => Context?.Completes;

        public virtual Context? Context
        {
            get => _context;
            set => _context = value;
        }
        
        protected ContentType ContentType => ContentType.Of("text/plain", "us-ascii");
        
        /// <summary>
        /// Answer a <see cref="Response"/> with the <see cref="ResponseStatus"/> and <paramref name="entity"/>
        /// with a <code>Content-Type</code> header per my <code>ContentType</code>, which may be overridden.
        /// </summary>
        /// <param name="status">The status of the response</param>
        /// <param name="entity">The string entity of the response</param>
        /// <returns><see cref="Response"/></returns>
        protected Response EntityResponseOf(ResponseStatus status, string entity) =>
            EntityResponseOf(status, Headers.Empty<ResponseHeader>(), entity);

        /// <summary>
        /// Answer a <see cref="Response"/> with the <see cref="ResponseStatus"/> and <paramref name="entity"/>
        /// with a <code>Content-Type</code> header per my <code>ContentType</code>, which may be overridden.
        /// </summary>
        /// <param name="status">The status of the response</param>
        /// <param name="headers">The <see cref="Headers{ResponseHeader}"/> to which the <code>Content-Type</code> header is appended</param>
        /// <param name="entity">The string entity of the response</param>
        /// <returns><see cref="Response"/></returns>
        protected Response EntityResponseOf(ResponseStatus status, Headers<ResponseHeader> headers, string entity) =>
            Response.Of(status, headers.And(ContentType.ToResponseHeader()), entity);

        protected internal virtual ILogger? Logger => _stage?.World.DefaultLogger;

        public virtual Scheduler? Scheduler => _stage?.Scheduler;

        public virtual Stage? Stage { get => _stage; set => _stage = value; }
    }
}

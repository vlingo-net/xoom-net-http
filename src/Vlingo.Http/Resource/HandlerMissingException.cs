// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.Serialization;

namespace Vlingo.Http.Resource
{
    public class HandlerMissingException : Exception
    {
        public HandlerMissingException()
        {
        }

        public HandlerMissingException(string message) : base(message)
        {
        }

        public HandlerMissingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HandlerMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

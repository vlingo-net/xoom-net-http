// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vlingo.Http.Media
{
    public abstract class MediaTypeDescriptor
    {
        internal const char PARAMETER_SEPARATOR = ';';
        internal const char MIME_SUBTYPE_SEPARATOR = '/';
        internal const char PARAMETER_ASSIGNMENT = '=';

        protected internal string MimeType { get; }
        protected internal string MimeSubType { get; }
        protected IDictionary<string, string> Parameters { get; }

        protected MediaTypeDescriptor(string mimeType, string mimeSubType, IDictionary<string, string> parameters)
        {
            MimeType = mimeType;
            MimeSubType = mimeSubType;
            Parameters = parameters;
        }

        protected MediaTypeDescriptor(string mimeType, string mimeSubType)
            : this(mimeType, mimeSubType, new Dictionary<string, string>())
        {
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(MimeType)
                .Append(MIME_SUBTYPE_SEPARATOR)
                .Append(MimeSubType);

            foreach (var parameterName in Parameters.Keys)
            {
                sb.Append(PARAMETER_SEPARATOR)
                  .Append(parameterName)
                  .Append(PARAMETER_ASSIGNMENT)
                  .Append(Parameters[parameterName]);
            }
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var mtd = (MediaTypeDescriptor)obj;

            if (!string.Equals(MimeType, mtd.MimeType) || !string.Equals(MimeSubType, mtd.MimeSubType))
            {
                return false;
            }

            if (Parameters == null && mtd.Parameters == null)
            {
                return true;
            }
            else if (Parameters == null || Parameters == null)
            {
                return false;
            }

            return Parameters.Intersect(mtd.Parameters).Count() == Parameters.Union(mtd.Parameters).Count();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (MimeType?.GetHashCode() ?? 0);
                hash = hash * 23 + (MimeSubType?.GetHashCode() ?? 0);
                hash = hash * 23 + (Parameters?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public class Builder<T>
        {
            protected string _mimeType;
            protected string _mimeSubType;
            protected IDictionary<string, string> _parameters;
            protected readonly Func<string, string, IDictionary<string,string>, T> _supplier;

            public Builder(Func<string, string, IDictionary<string, string>, T> supplier)
            {
                _supplier = supplier;
                _parameters = new Dictionary<string, string>();
                _mimeType = string.Empty;
                _mimeSubType = string.Empty;
            }

            public Builder<T> WithMimeType(string mimeType)
            {
                _mimeType = mimeType;
                return this;
            }

            public Builder<T> WithMimeSubType(string mimeSubType)
            {
                _mimeSubType = mimeSubType;
                return this;
            }

            public Builder<T> WithParameter(string paramName, string paramValue)
            {
                _parameters[paramName] = paramValue;
                return this;
            }

            public T Build() => _supplier.Invoke(_mimeType, _mimeSubType, _parameters);
        }
    }
}

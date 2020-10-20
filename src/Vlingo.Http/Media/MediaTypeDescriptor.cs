// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
        internal const char ParameterSeparator = ';';
        internal const char MimeSubtypeSeparator = '/';
        internal const char ParameterAssignment = '=';

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
                .Append(MimeSubtypeSeparator)
                .Append(MimeSubType);

            foreach (var parameterName in Parameters.Keys)
            {
                sb.Append(ParameterSeparator)
                  .Append(parameterName)
                  .Append(ParameterAssignment)
                  .Append(Parameters[parameterName]);
            }
            return sb.ToString();
        }

        public override bool Equals(object? obj)
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
            var hash = MimeType.GetHashCode() * 31 + MimeSubType.GetHashCode();
            foreach (var parameter in Parameters)
            {
                hash += parameter.GetHashCode();
            }

            return hash;
        }

        public class Builder<T>
        {
            protected string MimeType;
            protected string MimeSubType;
            protected readonly IDictionary<string, string> Parameters;
            protected readonly Func<string, string, IDictionary<string,string>, T> Supplier;

            public Builder(Func<string, string, IDictionary<string, string>, T> supplier)
            {
                Supplier = supplier;
                Parameters = new Dictionary<string, string>();
                MimeType = string.Empty;
                MimeSubType = string.Empty;
            }

            public Builder<T> WithMimeType(string mimeType)
            {
                MimeType = mimeType;
                return this;
            }

            public Builder<T> WithMimeSubType(string mimeSubType)
            {
                MimeSubType = mimeSubType;
                return this;
            }

            public Builder<T> WithParameter(string paramName, string paramValue)
            {
                Parameters[paramName] = paramValue;
                return this;
            }

            public T Build() => Supplier.Invoke(MimeType, MimeSubType, Parameters);
        }
    }
}

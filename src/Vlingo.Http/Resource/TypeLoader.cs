// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;

namespace Vlingo.Http.Resource
{
    public static class TypeLoader
    {
        public static Type Load(string className)
        {
            var classType = Type.GetType(className);
            if (classType == null)
            {
                // tires to load type with assembly name
                var classNameParts = className.Split('.');
                for (var i = 0; i < classNameParts.Length; i++)
                {
                    var potentialAssemblyName = string.Join("." ,classNameParts.Take(i + 1));
                    var fullyQualifiedTypeName = $"{className}, {potentialAssemblyName}";
                    classType = Type.GetType(fullyQualifiedTypeName);
                    if (classType != null)
                    {
                        break;
                    }
                }
            }

            if (classType == null)
            {
                throw new InvalidOperationException($"Cannot load class for: {className}");
            }
                
            return classType;
        }
    }
}
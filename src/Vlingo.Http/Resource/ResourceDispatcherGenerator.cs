// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.IO;
using Vlingo.Common.Compiler;

namespace Vlingo.Http.Resource
{
    public class ResourceDispatcherGenerator
    {
        internal sealed class Result
        {
            internal Result(
                string fullyQualifiedClassName,
                string className,
                string source,
                FileInfo sourceFile)
            {
                FullyQualifiedClassName = fullyQualifiedClassName;
                ClassName = className;
                Source = source;
                SourceFile = sourceFile;
            }

            public string FullyQualifiedClassName { get; }
            public string ClassName { get; }
            public string Source { get; }
            public FileInfo SourceFile { get; }
        }

        public DynaType Type { get; set; }


        public static ResourceDispatcherGenerator ForMain(IList<Action> actions, bool b)
        {
            throw new System.NotImplementedException();
        }

        public static ResourceDispatcherGenerator ForTest(IList<Action> actions, bool b)
        {
            throw new System.NotImplementedException();
        }

        internal Result GenerateFor(string fullName)
        {
            throw new System.NotImplementedException();
        }
    }
}
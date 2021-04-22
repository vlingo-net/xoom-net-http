// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Http.Resource;
using Vlingo.Http.Tests.Sample.User;
using Configuration = Vlingo.Http.Resource.Configuration;

namespace Vlingo.Http.Tests.Resource
{
    public class ServerBootstrap
    {
        private static readonly Random Random = new Random();
        private static readonly AtomicInteger PortToUse = new AtomicInteger(Random.Next(32_768, 60_999));
        
        public static ServerBootstrap Instance { get; private set; }
        
        public World World { get; }
        
        public static void Main(string[] args)
        {
            Instance = new ServerBootstrap();
        }
        
        public IServer Server { get; }
        
        private ServerBootstrap()
        {
            World = World.Start("vlingo-http-server");

            var userResource = new UserResourceFluent(World);
            var profileResource = new ProfileResourceFluent(World);
            var r1 = userResource.Routes();
            var r2 = profileResource.Routes();
            var resources = Resources.Are(r1, r2);

            Server =
                Server.StartWith(
                    World.Stage,
                    resources,
                    Filters.None(),
                    PortToUse.IncrementAndGet(),
                    Configuration.SizingConf.DefineWith(4, 10, 100, 10240),
                    Configuration.TimingConf.DefineWith(3, 1, 100),
                    "arrayQueueMailbox",
                    "arrayQueueMailbox");
            
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                if (Instance != null)
                {
                    Instance.Server.Stop();

                    Console.WriteLine("\n");
                    Console.WriteLine("==============================");
                    Console.WriteLine("Stopping vlingo/http Server...");
                    Console.WriteLine("==============================");
                }
            };
        }
    }
}
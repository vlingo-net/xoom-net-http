// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http.Resource
{
    public interface IServer : IStoppable
    {
        IServer StartWith(Stage stage);

        /// <summary>
        /// Answer a new <see cref="IServer"/> with the given configuration and characteristics.
        /// </summary>
        /// <param name="stage">The <see cref="Stage"/> in which the Server lives</param>
        /// <param name="properties">The <see cref="HttpProperties"/> with properties named per vlingo-http.json</param>
        /// <returns><see cref="IServer"/></returns>
        IServer StartWith(Stage stage, HttpProperties properties);

        /// <summary>
        /// Answer a new <see cref="IServer"/> with the given configuration and characteristics.
        /// </summary>
        /// <param name="stage">The <see cref="Stage"/> in which the <see cref="IServer"/> lives</param>
        /// <param name="resources">The <see cref="Resource"/> with URI descriptions that the Server understands</param>
        /// <param name="port">The int socket port the <see cref="IServer"/> will run on</param>
        /// <param name="sizing">The <see cref="Configuration.SizingConf"/> such as pool and buffer sizes</param>
        /// <param name="timing">The <see cref="Configuration.TimingConf"/> such as probe interval and missing content timeout</param>
        /// <returns><see cref="IServer"/></returns>
        IServer StartWith(Stage stage, Resources resources, int port, Configuration.SizingConf sizing, Configuration.TimingConf timing);

        /// <summary>
        /// Answer a new <see cref="IServer"/> with the given configuration and characteristics.
        /// </summary>
        /// <param name="stage">The <see cref="Stage"/> in which the <see cref="IServer"/> lives</param>
        /// <param name="resources">The <see cref="Resource"/> with URI descriptions that the Server understands</param>
        /// <param name="filters">The <see cref="Filters"/> used to process requests before dispatching to a resource</param>
        /// <param name="port">The int socket port the <see cref="IServer"/> will run on</param>
        /// <param name="sizing">The <see cref="Configuration.SizingConf"/> such as pool and buffer sizes</param>
        /// <param name="timing">The <see cref="Configuration.TimingConf"/> such as probe interval and missing content timeout</param>
        /// <returns><see cref="IServer"/></returns>
        IServer StartWith(Stage stage, Resources resources, Filters filters, int port, Configuration.SizingConf sizing, Configuration.TimingConf timing);

        /// <summary>
        /// Answer a new <see cref="IServer"/> with the given configuration and characteristics.
        /// </summary>
        /// <param name="stage">The <see cref="Stage"/> in which the <see cref="IServer"/> lives</param>
        /// <param name="resources">The <see cref="Resource"/> with URI descriptions that the Server understands</param>
        /// <param name="filters">The <see cref="Filters"/> used to process requests before dispatching to a resource</param>
        /// <param name="port">The int socket port the <see cref="IServer"/> will run on</param>
        /// <param name="sizing">The <see cref="Configuration.SizingConf"/> such as pool and buffer sizes</param>
        /// <param name="timing">The <see cref="Configuration.TimingConf"/> such as probe interval and missing content timeout</param>
        /// <param name="severMailboxTypeName">The string name of the mailbox to used by the <see cref="IServer"/></param>
        /// <param name="channelMailboxTypeName">The string name of the mailbox to use by the socket channel</param>
        /// <returns><see cref="IServer"/></returns>
        IServer StartWith(Stage stage, Resources resources, Filters filters, int port, Configuration.SizingConf sizing, Configuration.TimingConf timing, string severMailboxTypeName, string channelMailboxTypeName);
        
        ICompletes<bool> ShutDown();
        
        ICompletes<bool> StartUp();
    }

    public static class ServerFactory
    {
        public static IServer StartWith(Stage stage) => StartWith(stage, HttpProperties.Instance);

        public static IServer StartWith(Stage stage, HttpProperties properties)
        {
            var configuration = Configuration.DefineWith(properties);

            var resources = Loader.LoadResources(properties, stage.World.DefaultLogger);

            return StartWith(
                stage,
                resources,
                configuration.Port,
                configuration.Sizing,
                configuration.Timing);
        }

        public static IServer StartWith(Stage stage, Resources resources, int port, Configuration.SizingConf sizing, Configuration.TimingConf timing)
            => StartWith(stage, resources, Filters.None(), port, sizing, timing);
        
        public static IServer StartWith(Stage stage, Resources resources, Filters filters, int port, Configuration.SizingConf sizing, Configuration.TimingConf timing)
            => StartWith(stage, resources, filters, port, sizing, timing, "queueMailbox", "queueMailbox");

        public static IServer StartWith(Stage stage, Resources resources, Filters filters, int port, Configuration.SizingConf sizing, Configuration.TimingConf timing, string severMailboxTypeName, string channelMailboxTypeName)
        {
            var server = stage.ActorFor<IServer>(
                () => new ServerActor(resources, filters, port, sizing, timing, channelMailboxTypeName),
                severMailboxTypeName, ServerActor.ServerName, stage.World.AddressFactory.WithHighId(),
                stage.World.DefaultLogger);

            server.StartUp();

            return server;
        }
        
        public static IServer StartWithAgent(
            Stage stage,
            Resources resources,
            int port,
            int dispatcherPoolSize) =>
            StartWithAgent(stage, resources, Filters.None(), port, dispatcherPoolSize);

        public static IServer StartWithAgent(
            Stage stage,
            Resources resources,
            Filters filters,
            int port,
            int dispatcherPoolSize) =>
            StartWithAgent(stage, resources, filters, port, new Configuration.SizingConf(1, dispatcherPoolSize, 100, 10240), "queueMailbox");

        public static IServer StartWithAgent(Stage stage, Resources resources, Filters filters, int port, Configuration.SizingConf sizing, string severMailboxTypeName)
        {
            var server = stage.ActorFor<IServer>(
                () => new ServerActor(resources, filters, port, sizing, new Configuration.TimingConf(2, 2, 100), severMailboxTypeName),
                severMailboxTypeName, ServerActor.ServerName, stage.World.AddressFactory.WithHighId(),
                stage.World.DefaultLogger);

            server.StartUp();

            return server;
        }
    }
}
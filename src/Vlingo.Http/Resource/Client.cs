// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Wire.Node;

namespace Vlingo.Http.Resource
{
    /// <summary>
    /// Defines an HTTP client for sending <code>Request</code>s and receiving <code>Request</code>s.
    /// </summary>
    public class Client
    {
        public const string ClientIdCustomHeader = "X-VLINGO-CLIENT-ID";

        public enum ClientConsumerType { Correlating, LoadBalancing, RoundRobin };

        private readonly Configuration _configuration;
        private readonly IClientConsumer _consumer;

        /// <summary>
        /// Answers a new <code>Client</code> from the <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The <code>Configuration</code>.</param>
        /// <param name="type">The <code>ClientConsumerType</code>.</param>
        /// <param name="poolSize">The size of the pool of workers.</param>
        /// <returns></returns>
        public static Client Using(Configuration configuration, ClientConsumerType type, int poolSize)
            => new Client(configuration, type, poolSize);

        /// <summary>
        /// Answers a new <code>Client</code> from the <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The <code>Configuration</code>.</param>
        /// <returns></returns>
        public static Client Using(Configuration configuration)
            => new Client(configuration);


        /// <summary>
        /// Constructs my default state from the <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The <code>Configuration</code>.</param>
        /// <param name="type">The <code>ClientConsumerType</code>.</param>
        /// <param name="poolSize">The size of the pool of workers.</param>
        public Client(Configuration configuration, ClientConsumerType type, int poolSize)
        {
            _configuration = configuration;

            Type clientConsumerType;
            IEnumerable<object> parameters;

            switch (type)
            {
                case ClientConsumerType.Correlating:
                    clientConsumerType = typeof(ClientCorrelatingRequesterConsumerActor);
                    parameters = Definition.Parameters(configuration);
                    break;
                case ClientConsumerType.RoundRobin:
                    clientConsumerType = typeof(RoundRobinClientRequestConsumerActor);
                    var rrDefinition = Definition.Has<ClientConsumerWorkerActor>(Definition.Parameters(configuration));
                    var rrSpec = new RouterSpecification<IClientConsumer>(poolSize, rrDefinition);
                    parameters = Definition.Parameters(configuration, rrSpec);
                    break;
                case ClientConsumerType.LoadBalancing:
                    clientConsumerType = typeof(LoadBalancingClientRequestConsumerActor);
                    var lbDefinition = Definition.Has<ClientConsumerWorkerActor>(Definition.Parameters(configuration));
                    var lbSpec = new RouterSpecification<IClientConsumer>(poolSize, lbDefinition);
                    parameters = Definition.Parameters(configuration, lbSpec);
                    break;
                default:
                    throw new ArgumentException($"ClientConsumerType is not mapped: {type}");
            }

            _consumer = configuration.Stage.ActorFor<IClientConsumer>(Definition.Has(clientConsumerType, parameters));
        }

        public Client(Configuration configuration)
            : this(configuration, ClientConsumerType.Correlating, 0)
        {
        }

        public void Close() => _consumer.Stop();

        public ICompletes<Response> RequestWith(Request request)
        {
            var completes = _configuration.KeepAlive
                ? Completes.RepeatableUsing<Response>(_configuration.Stage.Scheduler)
                : Completes.Using<Response>(_configuration.Stage.Scheduler);
            _consumer.RequestWith(request, completes);
            return completes;
        }


        /// <summary>
        /// Configuration used to create a <code>Client</code>.
        /// </summary>
        public class Configuration
        {
            public Address AddressOfHost { get; }
            public IResponseConsumer ConsumerOfUnknownResponses { get; }
            public bool KeepAlive { get; }
            public long ProbeInterval { get; }
            public int ReadBufferSize { get; }
            public int ReadBufferPoolSize { get; }
            public int WriteBufferSize { get; }
            public Stage Stage { get; }
            public bool IsSecure { get; }

            private object? _testInfo;

            /// <summary>
            /// Answer the <code>Configuration</code> with defaults except for the <paramref name="addressOfHost"/>.
            /// </summary>
            /// <param name="stage">The Stage to host the Client.</param>
            /// <param name="consumerOfUnknownResponses">The ResponseConsumer of responses that cannot be associated with a given consumer.</param>
            /// <returns></returns>
            public static Configuration DefaultedExceptFor(
                Stage stage,
                IResponseConsumer consumerOfUnknownResponses)
                => DefaultedExceptFor(
                    stage,
                    Address.From(Host.Of("localhost"), 8080, AddressType.None),
                    consumerOfUnknownResponses);

            /// <summary>
            /// Answer the <code>Configuration</code> with defaults except for the <paramref name="addressOfHost"/> and <paramref name="consumerOfUnknownResponses"/>.
            /// </summary>
            /// <param name="stage">The Stage to host the Client.</param>
            /// <param name="addressOfHost">The Address of the host server.</param>
            /// <param name="consumerOfUnknownResponses">The ResponseConsumer of responses that cannot be associated with a given consumer.</param>
            /// <returns></returns>
            public static Configuration DefaultedExceptFor(
                Stage stage,
                Address addressOfHost,
                IResponseConsumer consumerOfUnknownResponses)
                => Has(
                    stage,
                    addressOfHost,
                    consumerOfUnknownResponses,
                    false,
                    10,
                    10240,
                    10,
                    10240);

            /// <summary>
            /// Answer the <code>Configuration</code> with defaults except for the 
            /// <paramref name="addressOfHost"/>, <paramref name="consumerOfUnknownResponses"/>, 
            /// <paramref name="writeBufferSize"/>, 
            /// and <paramref name="readBufferSize"/>.
            /// </summary>
            /// <param name="stage">The Stage to host the Client.</param>
            /// <param name="addressOfHost">The Address of the host server.</param>
            /// <param name="consumerOfUnknownResponses">The ResponseConsumer of responses that cannot be associated with a given consumer.</param>
            /// <param name="writeBufferSize">The int size of the write buffer.</param>
            /// <param name="readBufferSize">The int size of the read buffer.</param>
            /// <returns></returns>
            public static Configuration DefaultedExceptFor(
                Stage stage,
                Address addressOfHost,
                IResponseConsumer consumerOfUnknownResponses,
                int writeBufferSize,
                int readBufferSize)
                => Has(
                    stage,
                    addressOfHost,
                    consumerOfUnknownResponses,
                    false,
                    10,
                    writeBufferSize,
                    10,
                    readBufferSize);

            /// <summary>
            /// Answer the <code>Configuration</code> with for keep-alive mode with defaults 
            /// except for the <paramref name="addressOfHost"/> and <paramref name="consumerOfUnknownResponses"/>.
            /// </summary>
            /// <param name="stage">The Stage to host the Client.</param>
            /// <param name="addressOfHost">The Address of the host server.</param>
            /// <param name="consumerOfUnknownResponses">The ResponseConsumer of responses that cannot be associated with a given consumer.</param>
            /// <returns></returns>
            public static Configuration DefaultedKeepAliveExceptFor(
                Stage stage,
                Address addressOfHost,
                IResponseConsumer consumerOfUnknownResponses)
                => Has(
                    stage,
                    addressOfHost,
                    consumerOfUnknownResponses,
                    true,
                    10,
                    10240,
                    10,
                    10240);

            /// <summary>
            /// Answer the <code>Configuration</code> with the given options.
            /// </summary>
            /// <param name="stage">The Stage to host the Client.</param>
            /// <param name="addressOfHost">The Address of the host server.</param>
            /// <param name="consumerOfUnknownResponses">The ResponseConsumer of responses that cannot be associated with a given consumer.</param>
            /// <param name="keepAlive">The boolean indicating whether or not the connection is kept alive over multiple requests-responses.</param>
            /// <param name="probeInterval">The long number of milliseconds between each consumer channel probe.</param>
            /// <param name="writeBufferSize">The int size of the buffer used for writes/sends.</param>
            /// <param name="readBufferPoolSize">The int number of read buffers in the pool.</param>
            /// <param name="readBufferSize">The int size of the buffer used for reads/receives.</param>
            /// <returns></returns>
            public static Configuration Has(
                Stage stage,
                Address addressOfHost,
                IResponseConsumer consumerOfUnknownResponses,
                bool keepAlive,
                long probeInterval,
                int writeBufferSize,
                int readBufferPoolSize,
                int readBufferSize)
                => new Configuration(
                    stage,
                    addressOfHost,
                    consumerOfUnknownResponses,
                    keepAlive,
                    probeInterval,
                    writeBufferSize,
                    readBufferPoolSize,
                    readBufferSize,
                    false);

            /// <summary>
            /// Answer the <code>Configuration</code> with the given options and that requires a secure channel.
            /// </summary>
            /// <param name="stage">The Stage to host the Client.</param>
            /// <param name="addressOfHost">The Address of the host server.</param>
            /// <param name="consumerOfUnknownResponses">The ResponseConsumer of responses that cannot be associated with a given consumer.</param>
            /// <param name="keepAlive">The boolean indicating whether or not the connection is kept alive over multiple requests-responses.</param>
            /// <param name="probeInterval">The long number of milliseconds between each consumer channel probe.</param>
            /// <param name="writeBufferSize">The int size of the buffer used for writes/sends.</param>
            /// <param name="readBufferPoolSize">The int number of read buffers in the pool.</param>
            /// <param name="readBufferSize">The int size of the buffer used for reads/receives.</param>
            /// <returns></returns>
            public static Configuration Secure(
                Stage stage,
                Address addressOfHost,
                IResponseConsumer consumerOfUnknownResponses,
                bool keepAlive,
                long probeInterval,
                int writeBufferSize,
                int readBufferPoolSize,
                int readBufferSize)
                => new Configuration(
                    stage,
                    addressOfHost,
                    consumerOfUnknownResponses,
                    keepAlive,
                    probeInterval,
                    writeBufferSize,
                    readBufferPoolSize,
                    readBufferSize,
                    true);

            /// <summary>
            /// Constructs my default state with the given options.
            /// </summary>
            /// <param name="stage">The Stage to host the Client.</param>
            /// <param name="addressOfHost">The Address of the host server.</param>
            /// <param name="consumerOfUnknownResponses">The ResponseConsumer of responses that cannot be associated with a given consumer.</param>
            /// <param name="keepAlive">The boolean indicating whether or not the connection is kept alive over multiple requests-responses.</param>
            /// <param name="probeInterval">The long number of milliseconds between each consumer channel probe.</param>
            /// <param name="writeBufferSize">The int size of the buffer used for writes/sends.</param>
            /// <param name="readBufferPoolSize">The int number of read buffers in the pool.</param>
            /// <param name="readBufferSize">The int size of the buffer used for reads/receives.</param>
            /// <param name="secure">The boolean indicating whether the connection should be secure.</param>
            public Configuration(
                Stage stage,
                Address addressOfHost,
                IResponseConsumer consumerOfUnknownResponses,
                bool keepAlive,
                long probeInterval,
                int writeBufferSize,
                int readBufferPoolSize,
                int readBufferSize,
                bool secure)
            {
                Stage = stage;
                AddressOfHost = addressOfHost;
                ConsumerOfUnknownResponses = consumerOfUnknownResponses;
                KeepAlive = keepAlive;
                ProbeInterval = probeInterval;
                WriteBufferSize = writeBufferSize;
                ReadBufferPoolSize = readBufferPoolSize;
                ReadBufferSize = readBufferSize;
                IsSecure = secure;
            }

            /// <summary>
            /// Answer whether or not I have <code>TestInfo</code>.
            /// </summary>
            public bool HasTestInfo => _testInfo != null;

            /// <summary>
            /// Answer my test info, which may be null.
            /// </summary>
            /// <typeparam name="R">The type expected by the test request/response handler.</typeparam>
            /// <returns></returns>
            public R TestInfo<R>() => (R)_testInfo!;

            /// <summary>
            /// Marks this configuration as used for testing. There may be a
            /// contract with a given client worker type to attach some test
            /// data (perhaps a custom header) that conveys useful information
            /// to the test being run.
            /// </summary>
            /// <param name="testInfo">The Object reference to test information.</param>
            public void TestInfo(object testInfo) => _testInfo = testInfo;
        }
    }
}

// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Wire.Message;
using Xunit;
using Xunit.Abstractions;
using IDispatcher = Vlingo.Xoom.Http.Resource.IDispatcher;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class DynamicResourceDispatcherTest : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IDispatcher _dispatcher;
        private readonly World _world;
        private readonly IConsumerByteBuffer _consumerByteBuffer = BasicConsumerByteBuffer.Allocate(4, 512);
        private long _previousResourceHandlerId = -1L;
        private static readonly Data TestData1 = Data.With("Test1", "The test description");
        private static readonly string DataSerialized = JsonSerialization.Serialized(TestData1);
        private readonly string _postDataMessage =
            $"POST /res HTTP/1.1\nHost: vlingo.io\nContent-Length: {DataSerialized.Length}\n\n{DataSerialized}";

        [Fact]
        public void TestThatDispatchesThroughPool()
        {
            for (var count = 0; count < 3; ++count)
            {
                var request = Request.From(ToConsumerByteBuffer(_postDataMessage));
                var completes = new MockCompletesEventuallyResponse();

                var outcomes = completes.ExpectWithTimes(1);
                _dispatcher.DispatchFor(new Context(request, completes));
                var responseCount = outcomes.ReadFrom<int>("completed");
                var response = outcomes.ReadFrom<Response>("response");
                Assert.Equal(1, responseCount);

                var responseData = JsonSerialization.Deserialized<Data>(response.Entity.Content);

                Assert.Equal(TestData1, responseData);

                _output.WriteLine("previousResourceHandlerId={0} resourceHandlerId={1}", _previousResourceHandlerId,
                    responseData.ResourceHandlerId);

                Assert.NotEqual(_previousResourceHandlerId, responseData.ResourceHandlerId);

                _previousResourceHandlerId = responseData.ResourceHandlerId;
            }
        }

        public DynamicResourceDispatcherTest(ITestOutputHelper output)
        {
            _output = output;
            var converter = new Converter(output);
            Console.SetOut(converter);

            _world = World.Start("test-dynamic-resource-dispatcher");

            var fluentResource = new FluentTestResource(_world);

            var resource = fluentResource.Routes();

            resource.AllocateHandlerPool(_world.Stage);

            var resources = Resources.Are(resource);

            _dispatcher = new TestDispatcher(resources, _world.DefaultLogger);
        }

        public void Dispose() => _world.Terminate();

        private IConsumerByteBuffer ToConsumerByteBuffer(string requestContent)
        {
            _consumerByteBuffer.Clear();
            _consumerByteBuffer.Put(Converters.TextToBytes(requestContent));
            _consumerByteBuffer.Flip();
            return _consumerByteBuffer;
        }
    }
}
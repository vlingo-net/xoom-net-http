// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using Vlingo.Actors;
using Vlingo.Http.Resource;
using Vlingo.Http.Resource.Serialization;
using Vlingo.Wire.Channel;
using Vlingo.Wire.Message;
using Xunit;
using Xunit.Abstractions;
using IDispatcher = Vlingo.Http.Resource.IDispatcher;

namespace Vlingo.Http.Tests.Resource
{
    public class DynamicResourceDispatcherTest : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IDispatcher _dispatcher;
        private readonly World _world;
        private readonly MemoryStream _buffer = new MemoryStream(512);
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
                var request = Request.From(ToStream(_postDataMessage).ToArray());
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

        private MemoryStream ToStream(string requestContent)
        {
            _buffer.Clear();
            _buffer.Write(Converters.TextToBytes(requestContent));
            _buffer.Flip();
            return _buffer;
        }
    }
}
// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Text;
using System.Threading;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Wire.Channel;
using Vlingo.Xoom.Wire.Fdx.Bidirectional;
using Vlingo.Xoom.Wire.Message;
using Vlingo.Xoom.Wire.Nodes;
using Xunit;
using Xunit.Abstractions;
using Configuration = Vlingo.Xoom.Http.Resource.Configuration;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class SinglePageApplicationResourceTest : IDisposable
    {
        private static readonly AtomicInteger BaseServerPort = new AtomicInteger(20001);

        private readonly IConsumerByteBuffer _buffer = BasicConsumerByteBuffer.Allocate(1, 65535);
        private readonly IClientRequestResponseChannel _client;

        private const string ContentRoot = "Vlingo.Xoom.Http.Tests.Content";
        private const string ContextPath = "/app";

        private readonly Progress _progress;

        private readonly IServer _server;
        private readonly World _world;
        
        [Fact]
        public void RootDefaultStaticFile()
        {
            var resource = "/index.html";
            var content = ReadTextFile(ContentRoot + resource);
            var request = GetRequest(ContextPath + "/");
            var consumeCalls = _progress.ExpectConsumeTimes(1);

            _client.RequestWith(ToByteBuffer(request).ToArray());

            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryPeek(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void RootStaticFile()
        {
            var resource = "/index.html";
            var content = ReadTextFile(ContentRoot + resource);
            var request = GetRequest(ContextPath + resource);
            var consumeCalls = _progress.ExpectConsumeTimes(1);

            _client.RequestWith(ToByteBuffer(request).ToArray());

            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryPeek(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void DynamicPath()
        {
            var content = ReadTextFile(ContentRoot + "/index.html");
            var request = GetRequest(ContextPath + "/customers");
            var consumeCalls = _progress.ExpectConsumeTimes(1);

            _client.RequestWith(ToByteBuffer(request).ToArray());

            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryPeek(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void DynamicPathWithSubPath()
        {
            var content = ReadTextFile(ContentRoot + "/index.html");
            var request = GetRequest(ContextPath + "/customers/1");
            var consumeCalls = _progress.ExpectConsumeTimes(1);

            _client.RequestWith(ToByteBuffer(request).ToArray());

            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryPeek(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void DynamicPathWithSubPathWithUuid()
        {
            var content = ReadTextFile(ContentRoot + "/index.html");
            var request = GetRequest(ContextPath + "/specialists/ea9124b6-2b34-4906-bcec-0a2fed9625b6/specializeIn");
            var consumeCalls = _progress.ExpectConsumeTimes(1);

            _client.RequestWith(ToByteBuffer(request).ToArray());

            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryPeek(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void CssSubDirectoryStaticFile()
        {
            var resource = "/css/styles.css";
            var content = ReadTextFile(ContentRoot + resource);
            var request = GetRequest(ContextPath + resource);
            var consumeCalls = _progress.ExpectConsumeTimes(1);

            _client.RequestWith(ToByteBuffer(request).ToArray());

            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryPeek(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void JsSubDirectoryStaticFile()
        {
            var resource = "/js/vuetify.js";
            var content = ReadTextFile(ContentRoot + resource);
            var request = GetRequest(ContextPath + resource);
            var consumeCalls = _progress.ExpectConsumeTimes(1);

            _client.RequestWith(ToByteBuffer(request).ToArray());

            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryPeek(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void ViewsSubDirectoryStaticFile()
        {
            var resource = "/views/About.vue";
            var content = ReadTextFile(ContentRoot + resource);
            var request = GetRequest(ContextPath + resource);
            var consumeCalls = _progress.ExpectConsumeTimes(1);

            _client.RequestWith(ToByteBuffer(request).ToArray());

            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryPeek(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        public SinglePageApplicationResourceTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);

            _world = World.StartWithDefaults("static-file-resources");

            var serverPort = BaseServerPort.GetAndIncrement();

            var resources = Resources.Are(new SinglePageApplicationResource(ContentRoot, ContextPath).Routes());
            var sizing = Configuration.SizingConf.DefineWith(2, 10, 100, 65535);
            var timing = Configuration.TimingConf.DefineWith(2, 2, 100);
            _server = ServerFactory.StartWith(_world.Stage, resources, serverPort, sizing, timing);
            Assert.True(_server.StartUp().Await(TimeSpan.FromMilliseconds(500)));

            _progress = new Progress();
            var consumer = _world.ActorFor<IResponseChannelConsumer>(() => new TestResponseChannelConsumer(_progress));
            _client = new BasicClientRequestResponseChannel(
                Address.From(Host.Of("localhost"), serverPort, AddressType.None), consumer, 100, 10240,
                _world.DefaultLogger);
        }

        public void Dispose()
        {
            _client.Close();
            _server.ShutDown();
            Thread.Sleep(200);
            _world.Terminate();
        }

        private string GetRequest(string filePath) => 
            $"GET {string.Join("%20", filePath.Split(" "))} HTTP/1.1\nHost: vlingo.io\n\n";

        private byte[] ReadFile(string path)
        {
            path = $"{path.Replace("/", ".").Replace(" ", "_")}";
            var contentStream = typeof(StaticFilesResourceTest).Assembly.GetManifestResourceStream(path);
            if (contentStream != null && contentStream.Length > 0)
            {
                var content = new byte[contentStream.Length];
                contentStream.Read(content, 0, content.Length);
                return content;
            }

            throw new FileNotFoundException("File not found.");
        }

        private string ReadTextFile(string path) => Encoding.UTF8.GetString(ReadFile(path));

        private IConsumerByteBuffer ToByteBuffer(string requestContent)
        {
            _buffer.Clear();
            _buffer.Put(Converters.TextToBytes(requestContent));
            _buffer.Flip();
            return _buffer;
        }
    }
}
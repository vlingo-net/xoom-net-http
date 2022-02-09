// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Wire.Channel;
using Vlingo.Xoom.Wire.Fdx.Bidirectional;
using Vlingo.Xoom.Wire.Message;
using Vlingo.Xoom.Wire.Nodes;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class StaticFilesResourceTest : IDisposable
    {
        private static readonly AtomicInteger BaseServerPort = new AtomicInteger(19001);

        private readonly MemoryStream _buffer = new MemoryStream(65535);
        private readonly IClientRequestResponseChannel _client;
        private readonly string _contentRoot;
        private readonly Progress _progress;
        private readonly World _world;

        private string GetRequest(string filePath)
            => $"GET {string.Join("%20", filePath.Split(" "))} HTTP/1.1\nHost: vlingo.io\n\n";
        
        [Fact]
        public void TestThatServesRootDefaultStaticFile()
        {
            var resource = "/index.html";
            var content = ReadManifestTextFile(_contentRoot + resource);
            var request = GetRequest("/");
            
            var consumeCalls = _progress.ExpectConsumeTimes(1);
            
            _client.RequestWith(ToByteBuffer(request));
            
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }
            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryDequeue(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }
        
        [Fact]
        public void TestThatServesDefaultStaticFile()
        {
            var resource = "/views/test 2/index.html";
            var content = ReadManifestTextFile(_contentRoot + resource);
            var request = GetRequest("/views/test 2/");
            
            var consumeCalls = _progress.ExpectConsumeTimes(1);

            _client.RequestWith(ToByteBuffer(request));
            
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }
            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryDequeue(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void TestThatServesRootStaticFile()
        {
            var resource = "/index.html";
            var content = ReadManifestTextFile(_contentRoot + resource);
            var request = GetRequest(resource);
            
            var consumeCalls = _progress.ExpectConsumeTimes(1);
            
            _client.RequestWith(ToByteBuffer(request));
            
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryDequeue(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void TestThatServesCssSubDirectoryStaticFile()
        {
            var resource = "/css/styles.css";
            var content = ReadManifestTextFile(_contentRoot + resource);
            var request = GetRequest(resource);
            
            var consumeCalls = _progress.ExpectConsumeTimes(1);
            
            _client.RequestWith(ToByteBuffer(request));
            
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");

            _progress.Responses.TryDequeue(out var contentResponse);

            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void TestThatServesJsSubDirectoryStaticFile()
        {
            var resource = "/js/vuetify.js";
            var content = ReadManifestTextFile(_contentRoot + resource);
            var request = GetRequest(resource);
            
            var consumeCalls = _progress.ExpectConsumeTimes(1);
            
            _client.RequestWith(ToByteBuffer(request));
            
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");
            
            _progress.Responses.TryDequeue(out var contentResponse);
            
            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        [Fact]
        public void TestThatServesViewsSubDirectoryStaticFile()
        {
            var resource = "/views/About.vue";
            var content = ReadManifestTextFile(_contentRoot + resource);
            var request = GetRequest(resource);
            
            var consumeCalls = _progress.ExpectConsumeTimes(1);
            
            _client.RequestWith(ToByteBuffer(request));
            
            while (consumeCalls.TotalWrites < 1)
            {
                _client.ProbeChannel();
            }

            consumeCalls.ReadFrom<int>("completed");
            
            _progress.Responses.TryDequeue(out var contentResponse);
            
            Assert.Equal(1, _progress.ConsumeCount.Get());
            Assert.Equal(ResponseStatus.Ok, contentResponse.Status);
            Assert.Equal(content, contentResponse.Entity.Content);
        }

        public StaticFilesResourceTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);

            _world = World.StartWithDefaults("static-file-resources");

            var serverPort = BaseServerPort.GetAndIncrement();

            var properties = new Dictionary<string, string>();

            properties.Add("server.http.port", serverPort.ToString());
            properties.Add("server.dispatcher.pool", "10");
            properties.Add("server.buffer.pool.size", "100");
            properties.Add("server.message.buffer.size", "65535");
            properties.Add("server.probe.interval", "2");
            properties.Add("server.probe.timeout", "2");
            properties.Add("server.processor.pool.size", "10");
            properties.Add("server.request.missing.content.timeout", "100");

            properties.Add("static.files.resource.pool", "5");
            _contentRoot = $"Vlingo.Xoom.Http.Tests.Content";
            properties.Add("static.files.resource.root", _contentRoot);
            properties.Add("static.files.resource.subpaths", "[/, /css, /js, /views]");

            properties.Add("feed.producer.name.events", "/feeds/events");
            properties.Add("feed.producer.events.class", "Vlingo.Xoom.Http.Tests.Resource.Feed.EventsFeedProducerActor");
            properties.Add("feed.producer.events.payload", "20");
            properties.Add("feed.producer.events.pool", "10");

            properties.Add("sse.stream.name.all", "/eventstreams/all");
            properties.Add("sse.stream.all.feed.class", "Vlingo.Xoom.Http.Tests.Sample.User.AllSseFeedActor");
            properties.Add("sse.stream.all.feed.payload", "50");
            properties.Add("sse.stream.all.feed.interval", "1000");
            properties.Add("sse.stream.all.feed.default.id", "-1");
            properties.Add("sse.stream.all.pool", "10");

            properties.Add("resource.name.profile", "[define, query]");

            properties.Add("resource.profile.handler", "Vlingo.Xoom.Http.Tests.Sample.User.ProfileResource");
            properties.Add("resource.profile.pool", "5");
            properties.Add("resource.profile.disallowPathParametersWithSlash", "false");

            properties.Add("action.profile.define.method", "PUT");
            properties.Add("action.profile.define.uri", "/users/{userId}/profile");
            properties.Add("action.profile.define.to",
                "define(string userId, body:Vlingo.Xoom.Http.Tests.Sample.User.ProfileData profileData)");
            properties.Add("action.profile.define.mapper", "Vlingo.Xoom.Http.Tests.Sample.User.ProfileDataMapper");

            properties.Add("action.profile.query.method", "GET");
            properties.Add("action.profile.query.uri", "/users/{userId}/profile");
            properties.Add("action.profile.query.to", "query(string userId)");
            properties.Add("action.profile.query.mapper", "Vlingo.Xoom.Http.Tests.Sample.User.ProfileDataMapper");

            var httpProperties = HttpProperties.Instance;
            httpProperties.SetCustomProperties(properties);

            var server = ServerFactory.StartWith(_world.Stage, httpProperties);
            Assert.True(server.StartUp().Await(TimeSpan.FromMilliseconds(500L)));

            _progress = new Progress();
            var consumer = _world.ActorFor<IResponseChannelConsumer>(
                () => new TestResponseChannelConsumer(_progress));
            _client = new BasicClientRequestResponseChannel(
                Address.From(Host.Of("localhost"), serverPort, AddressType.None), consumer, 100, 10240,
                _world.DefaultLogger);
        }

        private string ReadTextFile(string path)
        {
            var fixedPath = Path.GetFullPath(path);
            return File.ReadAllText(fixedPath);
        }
        
        private string ReadManifestTextFile(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            path = $"{path.Replace("/", ".").Replace(" ", "_")}";
            var stream = assembly.GetManifestResourceStream(path);
            using var sreader = new StreamReader(stream);
            return sreader.ReadToEnd();
        }

        private byte[] ToByteBuffer(string requestContent)
        {
            _buffer.Clear();
            _buffer.Write(Converters.TextToBytes(requestContent));
            _buffer.Flip();
            return _buffer.ToArray();
        }

        public void Dispose()
        {
            _client.Close();
            _buffer?.Dispose();
            _world.Terminate();
        }
    }
}
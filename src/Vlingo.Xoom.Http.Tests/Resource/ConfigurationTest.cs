// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Http.Resource;
using Xunit;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class ConfigurationTest
    {
        private readonly Request _request = Request.From(Method.Get, "/".ToMatchableUri(), Version.Http1_1, Headers.Empty<RequestHeader>(), Body.Empty);
        private readonly Response _response = Response.Of(ResponseStatus.Ok, Body.Empty);
        private readonly RequestFilter _dummyFilter = new DummyRequestFilter();
            
        [Fact]
        public void TestThatConfigurationDefaults()
        {
            var configuration =
                Configuration.Define();

            Assert.NotNull(configuration);
            Assert.Equal(8080, configuration.Port);

            Assert.NotNull(configuration.Sizing);
            Assert.Equal(10, configuration.Sizing.DispatcherPoolSize);
            Assert.Equal(100, configuration.Sizing.MaxBufferPoolSize);
            Assert.Equal(65535, configuration.Sizing.MaxMessageSize);

            Assert.NotNull(configuration.Timing);
            Assert.Equal(4, configuration.Timing.ProbeInterval);
            Assert.Equal(100, configuration.Timing.RequestMissingContentTimeout);
        }

        [Fact]
        public void TestThatConfigurationConfirgures()
        {
            var configuration =
                Configuration.Define()
                    .WithPort(9000)
                    .With(Configuration.SizingConf.DefineConf()
                        .WithDispatcherPoolSize(20)
                        .WithMaxBufferPoolSize(200)
                        .WithMaxMessageSize(3333))
                    .With(Configuration.TimingConf.DefineConf()
                        .WithProbeInterval(30)
                        .WithProbeTimeout(40)
                        .WithRequestMissingContentTimeout(200))
                    .With(Filters.Are(new []{_dummyFilter}, Filters.NoResponseFilters()));

            Assert.NotNull(configuration);
            Assert.Equal(9000, configuration.Port);

            Assert.NotNull(configuration.Sizing);
            Assert.Equal(20, configuration.Sizing.DispatcherPoolSize);
            Assert.Equal(200, configuration.Sizing.MaxBufferPoolSize);
            Assert.Equal(3333, configuration.Sizing.MaxMessageSize);

            Assert.NotNull(configuration.Timing);
            Assert.Equal(30, configuration.Timing.ProbeInterval);
            Assert.Equal(40, configuration.Timing.ProbeTimeout);
            Assert.Equal(200, configuration.Timing.RequestMissingContentTimeout);
            
            Assert.NotNull(configuration.Filters);
            Assert.Equal(Method.Post, configuration.Filters.Process(_request).Method);
            Assert.Equal(ResponseStatus.Ok, configuration.Filters.Process(_response).Status);
        }
    }
    
    public class DummyRequestFilter : RequestFilter
    {
        public override void Stop()
        {
        }

        public override (Request, bool) Filter(Request request) => 
            (Request.From(Method.Post, "/".ToMatchableUri(), Version.Http1_1, Headers.Empty<RequestHeader>(), Body.Empty), false);
    }
}
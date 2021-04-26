// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Http.Resource;
using Xunit;

namespace Vlingo.Http.Tests.Resource
{
    public class ConfigurationTest
    {
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
                        .WithRequestMissingContentTimeout(200));

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
        }
    }
}
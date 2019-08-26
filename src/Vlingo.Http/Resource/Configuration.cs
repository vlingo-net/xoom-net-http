// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Http.Resource
{
    public class Configuration
    {
        public static Configuration Instance { get; private set; }

        public int Port { get; }
        public SizingConf Sizing { get; }
        public TimingConf Timing { get; }

        public static Configuration Define()
        {
            Instance = new Configuration();
            return Instance;
        }

        private Configuration()
        {
            Port = 8080;
            Sizing = SizingConf.Define();
            Timing = TimingConf.Define();
        }

        private Configuration(HttpProperties properties)
            : this()
        {
            Port = int.Parse(properties.GetProperty("server.http.port", Port.ToString()));
            var processorPoolSize = int.Parse(properties.GetProperty("server.processor.pool.size", Sizing.ProcessorPoolSize.ToString()));
            var dispatcherPoolSize = int.Parse(properties.GetProperty("server.dispatcher.pool", Sizing.DispatcherPoolSize.ToString()));
            var maxBufferPoolSize = int.Parse(properties.GetProperty("server.buffer.pool.size", Sizing.MaxBufferPoolSize.ToString()));
            var maxMessageSize = int.Parse(properties.GetProperty("server.message.buffer.size", Sizing.MaxMessageSize.ToString()));
            var probeInterval = long.Parse(properties.GetProperty("server.probe.interval", Timing.ProbeInterval.ToString()));
            var requestMissingContentTimeout = long.Parse(properties.GetProperty("server.request.missing.content.timeout", Timing.RequestMissingContentTimeout.ToString()));

            Sizing = new SizingConf(processorPoolSize, dispatcherPoolSize, maxBufferPoolSize, maxMessageSize);
            Timing = new TimingConf(probeInterval, requestMissingContentTimeout);
        }

        public class SizingConf
        {
            public int ProcessorPoolSize { get; }
            public int DispatcherPoolSize { get; }
            public int MaxBufferPoolSize { get; }
            public int MaxMessageSize { get; }

            public static SizingConf Define() => new SizingConf(10, 10, 100, 65535);

            public SizingConf(int processorPoolSize, int dispatcherPoolSize, int maxBufferPoolSize, int maxMessageSize)
            {
                ProcessorPoolSize = processorPoolSize;
                DispatcherPoolSize = dispatcherPoolSize;
                MaxBufferPoolSize = maxBufferPoolSize;
                MaxMessageSize = maxMessageSize;
            }

            public SizingConf WithProcessorPoolSize(int processorPoolSize)
                => new SizingConf(processorPoolSize, DispatcherPoolSize, MaxBufferPoolSize, MaxMessageSize);

            public SizingConf WithDispatcherPoolSize(int dispatcherPoolSize)
                => new SizingConf(ProcessorPoolSize, dispatcherPoolSize, MaxBufferPoolSize, MaxMessageSize);

            public SizingConf WithMaxBufferPoolSize(int maxBufferPoolSize)
                => new SizingConf(ProcessorPoolSize, DispatcherPoolSize, maxBufferPoolSize, MaxMessageSize);

            public SizingConf WithMaxMessageSize(int maxMessageSize)
                => new SizingConf(ProcessorPoolSize, DispatcherPoolSize, MaxBufferPoolSize, maxMessageSize);
        }

        public class TimingConf
        {
            public TimingConf(long probeInterval, long requestMissingContentTimeout)
            {
                ProbeInterval = probeInterval;
                RequestMissingContentTimeout = requestMissingContentTimeout;
            }

            public static TimingConf Define() => new TimingConf(10, 100);

            public TimingConf WithProbeInterval(long probeInterval)
                => new TimingConf(probeInterval, RequestMissingContentTimeout);

            public TimingConf WithRequestMissingContentTimeout(long requestMissingContentTimeout)
                => new TimingConf(ProbeInterval, requestMissingContentTimeout);

            public long ProbeInterval { get; }
            public long RequestMissingContentTimeout { get; }
        }
    }
}

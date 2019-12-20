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
        public static Configuration? Instance { get; private set; }

        public int Port { get; private set; }
        public SizingConf Sizing { get; private set; }
        public TimingConf Timing { get; private set; }

        public static Configuration Define()
        {
            Instance = new Configuration();
            return Instance;
        }
        
        public static Configuration DefineWith(HttpProperties properties)
        {
            Instance = new Configuration(properties);
            return Instance;
        }

        public Configuration WithPort(int port)
        {
            Port = port;
            return this;
        }

        public Configuration With(SizingConf sizing) {
            Sizing = sizing;
            return this;
        }

        public Configuration With(TimingConf timing) {
            Timing = timing;
            return this;
        }

        private Configuration()
        {
            Port = 8080;
            Sizing = SizingConf.DefineConf();
            Timing = new TimingConf(4, 2, 100);
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
            var probeTimeout = long.Parse(properties.GetProperty("server.probe.timeout", Timing.ProbeInterval.ToString()));
            var requestMissingContentTimeout = long.Parse(properties.GetProperty("server.request.missing.content.timeout", Timing.RequestMissingContentTimeout.ToString()));

            Sizing = new SizingConf(processorPoolSize, dispatcherPoolSize, maxBufferPoolSize, maxMessageSize);
            Timing = new TimingConf(probeInterval, probeTimeout, requestMissingContentTimeout);
        }

        public class SizingConf
        {
            public int ProcessorPoolSize { get; }
            public int DispatcherPoolSize { get; }
            public int MaxBufferPoolSize { get; }
            public int MaxMessageSize { get; }

            public static SizingConf DefineConf() => new SizingConf(10, 10, 100, 65535);
            
            public static SizingConf DefineWith(int processorPoolSize, int dispatcherPoolSize, int maxBufferPoolSize, int maxMessageSize)
                => new SizingConf(processorPoolSize, dispatcherPoolSize, maxBufferPoolSize, maxMessageSize);

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
            public TimingConf(long probeInterval, long probeTimeout, long requestMissingContentTimeout)
            {
                ProbeInterval = probeInterval;
                ProbeTimeout = probeTimeout;
                RequestMissingContentTimeout = requestMissingContentTimeout;
            }

            public static TimingConf DefineConf() => new TimingConf(3, 2, 100);

            public static TimingConf DefineWith(long probeInterval, long probeTimeout, long requestMissingContentTimeout)
                => new TimingConf(probeInterval, probeTimeout, requestMissingContentTimeout);

            public TimingConf WithProbeInterval(long probeInterval)
                => new TimingConf(probeInterval, ProbeTimeout, RequestMissingContentTimeout);
            
            public TimingConf WithProbeTimeout(long probeTimeout)
                => new TimingConf(ProbeInterval, probeTimeout, RequestMissingContentTimeout);

            public TimingConf WithRequestMissingContentTimeout(long requestMissingContentTimeout)
                => new TimingConf(ProbeInterval, ProbeTimeout, requestMissingContentTimeout);

            public long ProbeInterval { get; }
            
            public long ProbeTimeout { get; }
            
            public long RequestMissingContentTimeout { get; }
        }
    }
}

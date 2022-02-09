// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;
using Vlingo.Xoom.Wire.Channel;

namespace Vlingo.Xoom.Http.Tests.Resource.Sse
{
    public class MockRequestResponseContext : RequestResponseContext
    {
        public MockResponseSenderChannel Channel { get; }
        
        public AtomicReference<object> ConsumerDataRef => new AtomicReference<object>();
        public AtomicReference<object> WhenClosingData => new AtomicReference<object>();

        public MockRequestResponseContext(MockResponseSenderChannel channel)
        {
            Channel = channel;
        }

        public override TR ConsumerData<TR>() => (TR)ConsumerDataRef.Get();

        public override TR ConsumerData<TR>(TR data)
        {
            ConsumerDataRef.Set(data);
            return data;
        }

        public override void WhenClosing(object data) => WhenClosingData.Set(data);

        public override bool HasConsumerData => ConsumerDataRef.Get() != null;

        public override string Id => "1";

        public override IResponseSenderChannel Sender => Channel;
    }
}
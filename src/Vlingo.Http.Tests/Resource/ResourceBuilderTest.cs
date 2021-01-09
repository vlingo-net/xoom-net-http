// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common.Serialization;
using Vlingo.Http.Resource;
using Vlingo.Http.Tests.Sample.User;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Http.Tests.Resource
{
    public class ResourceBuilderTest : ResourceTestFixtures
    {
        [Fact]
        public void SimpleRoute()
        {
            var resource = (DynamicResource) ResourceBuilder.Resource("userResource",
                    ResourceBuilder.Get("/helloWorld").Handle(() => Common.Completes.WithSuccess(Response.Of(ResponseStatus.Ok,  JsonSerialization.Serialized("Hello World")))),
                ResourceBuilder.Post("/post/{postId}")
                .Param<string>()
                .Body<UserData>()
                .Handle((postId, userData) => Common.Completes.WithSuccess(Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized(postId)))));

            Assert.NotNull(resource);
            Assert.Equal("userResource", resource.Name);
            Assert.Equal(10, resource.HandlerPoolSize);
            Assert.Equal(2, resource.Handlers.Count);
        }
        
        [Fact]
        public void ShouldRespondToCorrectResourceHandler()
        {
            var resource = (DynamicResource) ResourceBuilder.Resource("userResource",
                    ResourceBuilder.Get("/customers/{customerId}/accounts/{accountId}")
                .Param<string>()
                .Param<string>()
                .Handle((customerId, accountId) => Common.Completes.WithSuccess(Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized("users")))),
            ResourceBuilder.Get("/customers/{customerId}/accounts/{accountId}/withdraw")
                .Param<string>()
                .Param<string>()
                .Handle((customerId, accountId) => Common.Completes.WithSuccess(Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized("user admin"))))
            );

            var matchWithdrawResource = resource.MatchWith(Method.Get, "/customers/cd1234/accounts/ac1234/withdraw".ToMatchableUri());
            var matchAccountResource = resource.MatchWith(Method.Get, "/customers/cd1234/accounts/ac1234".ToMatchableUri());

            Assert.Equal("/customers/{customerId}/accounts/{accountId}/withdraw", matchWithdrawResource.Action.Uri);
            Assert.Equal("/customers/{customerId}/accounts/{accountId}", matchAccountResource.Action.Uri);
        }
        
        public ResourceBuilderTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}
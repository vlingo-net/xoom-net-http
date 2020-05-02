// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
using Vlingo.Http.Resource;
using Vlingo.Http.Tests.Sample.User;
using Xunit;
using Xunit.Abstractions;
using Configuration = Vlingo.Http.Resource.Configuration;

namespace Vlingo.Http.Tests
{
    public class FiltersTest
    {
        [Fact]
        public void TestThatRequestFiltersProcess()
        {
            var filter1 = new RequestFilter1();
            var filter2 = new RequestFilter1();
            var filter3 = new RequestFilter1();

            var filters = Filters.Are(new []{filter1, filter2, filter3}, Filters.NoResponseFilters());

            var request1 = Request.Has(Method.Get).And("/".ToMatchableUri());

            var request2 = request1;
            for (var times = 0; times < 5; ++times)
            {
                request2 = filters.Process(request2);
            }

            Assert.Equal(request1, request2);

            Assert.Equal(5, filter1.Count);
            Assert.Equal(4, filter2.Count);
            Assert.Equal(4, filter3.Count);

            filters.Stop();

            Assert.True(filter1.Stopped);
            Assert.True(filter2.Stopped);
            Assert.True(filter3.Stopped);
        }
        
        [Fact]
        public void TestThatResponseFiltersProcess()
        {
            var filter1 = new ResponseFilter1();
            var filter2 = new ResponseFilter1();
            var filter3 = new ResponseFilter1();

            var filters = Filters.Are(Filters.NoRequestFilters(), new []{filter1, filter2, filter3});

            var response1 = Response.Of(Response.ResponseStatus.Ok);

            var response2 = response1;
            for (var times = 0; times < 5; ++times)
            {
                response2 = filters.Process(response2);
            }

            Assert.Equal(response1, response2);

            Assert.Equal(5, filter1.Count);
            Assert.Equal(4, filter2.Count);
            Assert.Equal(4, filter3.Count);

            filters.Stop();

            Assert.True(filter1.Stopped);
            Assert.True(filter2.Stopped);
            Assert.True(filter3.Stopped);
        }
        
        [Fact]
        public void TestThatServerStartsWithFilters()
        {
            var world = World.StartWithDefaults("filters");

            var resource =
                ConfigurationResource.Defining("profile", typeof(ProfileResource), 5,
                    Actions.CanBe("PUT", "/users/{userId}/profile", "define(string userId, body:Vlingo.Http.Tests.Sample.User.ProfileData profileData)", "Vlingo.Http.Tests.Sample.User.ProfileDataMapper")
                        .Also("GET", "/users/{userId}/profile", "query(string userId)", "Vlingo.Http.Tests.Sample.User.ProfileDataMapper").ThatsAll(), world.DefaultLogger);

            var server =
                ServerFactory.StartWith(
                    world.Stage,
                    Resources.Are(resource),
                    Filters.Are(new []{new RequestFilter1()}, Filters.NoResponseFilters()),
                    8081,
                    Configuration.SizingConf.DefineConf(),
                    Configuration.TimingConf.DefineConf());

            Assert.NotNull(server);
        }

        public FiltersTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
        }
    }

    internal class RequestFilter1 : RequestFilter
    {
        public int Count { get; private set; }
        public bool Stopped { get; private set; }

        public RequestFilter1()
        {
            Count = 0;
            Stopped = false;
        }

        public override void Stop() => Stopped = true;

        public override (Request, bool) Filter(Request request) => (request, ++Count < 5);
    }

    internal class ResponseFilter1 : ResponseFilter
    {
        public int Count { get; private set; }
        public bool Stopped { get; private set; }

        public ResponseFilter1()
        {
            Count = 0;
            Stopped = false;
        }

        public override void Stop() => Stopped = true;

        public override (Response, bool) Filter(Response response) => (response, ++Count < 5);
    }
}
// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Http.Tests.Sample.User;
using Xunit;
using Xunit.Abstractions;
using Configuration = Vlingo.Xoom.Http.Resource.Configuration;

namespace Vlingo.Xoom.Http.Tests
{
    public class FiltersTest
    {
        private static readonly Random Random = new Random();
        private static readonly AtomicInteger PortToUse = new AtomicInteger(Random.Next(32_768, 60_999));
        
        private static readonly string HeaderAcceptOriginAny = "*";
        private static readonly string ResponseHeaderAcceptAllHeaders = "X-Requested-With, Content-Type, Content-Length";
        private static readonly string ResponseHeaderAcceptMethodsAll = "POST,GET,PUT,PATCH,DELETE";

        private static readonly string HeaderAcceptOriginHelloWorld = "hello.world";
        private static readonly string ResponseHeaderAcceptHeadersHelloWorld = "X-Requested-With, Content-Type, Content-Length";
        private static readonly string ResponseHeaderAcceptMethodsHelloWorld = "POST,GET";

        private static readonly string HeaderAcceptOriginHelloCors = "hello.cors";
        private static readonly string ResponseHeaderAcceptHeadersHelloCors = "Content-Type, Content-Length";
        private static readonly string ResponseHeaderAcceptMethodsHelloCors = "POST,GET,PUT";

        
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

            var response1 = Response.Of(ResponseStatus.Ok);

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
        public void TestThatCorsOriginAnyAllowed()
        {
            var filter = new CORSResponseFilter();

            var headers = new List<ResponseHeader>
            {
                ResponseHeader.Of(ResponseHeader.AccessControlAllowOrigin, HeaderAcceptOriginAny),
                ResponseHeader.Of(ResponseHeader.AccessControlAllowHeaders, ResponseHeaderAcceptAllHeaders),
                ResponseHeader.Of(ResponseHeader.AccessControlAllowMethods, ResponseHeaderAcceptMethodsAll)
            };

            filter.OriginHeadersFor(HeaderAcceptOriginAny, headers);

            var request1 = Request.Has(Method.Get).And(RequestHeader.Of(RequestHeader.Origin, HeaderAcceptOriginAny));

            var response1 = Response.Of(ResponseStatus.Ok).Include(ResponseHeader.WithContentLength(0));

            var filteredResponse = filter.Filter(request1, response1);

            Assert.True(filteredResponse.Item2);
            Assert.Equal(HeaderAcceptOriginAny, filteredResponse.Item1.HeaderOf(ResponseHeader.AccessControlAllowOrigin).Value);
            Assert.Equal(ResponseHeaderAcceptAllHeaders, filteredResponse.Item1.HeaderOf(ResponseHeader.AccessControlAllowHeaders).Value);
            Assert.Equal(ResponseHeaderAcceptMethodsAll, filteredResponse.Item1.HeaderOf(ResponseHeader.AccessControlAllowMethods).Value);
          }
        
          [Fact]
          public void TestThatCorsOriginSomeAllowed()
          {
            var filter = new CORSResponseFilter();

            var headersHelloWorld = new List<ResponseHeader>
            {
                ResponseHeader.Of(ResponseHeader.AccessControlAllowOrigin, HeaderAcceptOriginHelloWorld),
                ResponseHeader.Of(ResponseHeader.AccessControlAllowHeaders, ResponseHeaderAcceptHeadersHelloWorld),
                ResponseHeader.Of(ResponseHeader.AccessControlAllowMethods, ResponseHeaderAcceptMethodsHelloWorld)
            };

            var headersHelloCors = new List<ResponseHeader>
            {
                ResponseHeader.Of(ResponseHeader.AccessControlAllowOrigin, HeaderAcceptOriginHelloCors),
                ResponseHeader.Of(ResponseHeader.AccessControlAllowHeaders, ResponseHeaderAcceptHeadersHelloCors),
                ResponseHeader.Of(ResponseHeader.AccessControlAllowMethods, ResponseHeaderAcceptMethodsHelloCors)
            };

            filter.OriginHeadersFor(HeaderAcceptOriginHelloWorld, headersHelloWorld);
            filter.OriginHeadersFor(HeaderAcceptOriginHelloCors, headersHelloCors);

            //////////////// request: hello.world

            var requestHelloWorld = Request.Has(Method.Get).And(RequestHeader.Of(RequestHeader.Origin, HeaderAcceptOriginHelloWorld));

            var responseHelloWorld = Response.Of(ResponseStatus.Ok).Include(ResponseHeader.WithContentLength(0));

            var helloWorldFilteredResponse = filter.Filter(requestHelloWorld, responseHelloWorld);

            Assert.True(helloWorldFilteredResponse.Item2);
            Assert.Equal(HeaderAcceptOriginHelloWorld, helloWorldFilteredResponse.Item1.HeaderOf(ResponseHeader.AccessControlAllowOrigin).Value);
            Assert.Equal(ResponseHeaderAcceptHeadersHelloWorld, helloWorldFilteredResponse.Item1.HeaderOf(ResponseHeader.AccessControlAllowHeaders).Value);
            Assert.Equal(ResponseHeaderAcceptMethodsHelloWorld, helloWorldFilteredResponse.Item1.HeaderOf(ResponseHeader.AccessControlAllowMethods).Value);

            //////////////// request: hello.cors

            var requestHelloCors = Request.Has(Method.Get).And(RequestHeader.Of(RequestHeader.Origin, HeaderAcceptOriginHelloCors));

            var responseHelloCors = Response.Of(ResponseStatus.Ok).Include(ResponseHeader.WithContentLength(0));

            var helloCorsFilteredResponse = filter.Filter(requestHelloCors, responseHelloCors);

            Assert.True(helloCorsFilteredResponse.Item2);
            Assert.Equal(HeaderAcceptOriginHelloCors, helloCorsFilteredResponse.Item1.HeaderOf(ResponseHeader.AccessControlAllowOrigin).Value);
            Assert.Equal(ResponseHeaderAcceptHeadersHelloCors, helloCorsFilteredResponse.Item1.HeaderOf(ResponseHeader.AccessControlAllowHeaders).Value);
            Assert.Equal(ResponseHeaderAcceptMethodsHelloCors, helloCorsFilteredResponse.Item1.HeaderOf(ResponseHeader.AccessControlAllowMethods).Value);

            //////////////// request: *

            var requestAll = Request.Has(Method.Get).And(RequestHeader.Of(RequestHeader.Origin, HeaderAcceptOriginAny));

            var responseAll = Response.Of(ResponseStatus.Ok).Include(ResponseHeader.WithContentLength(0));

            var allFilteredResponse = filter.Filter(requestAll, responseAll);

            Assert.True(allFilteredResponse.Item2);
            Assert.Null(allFilteredResponse.Item1.HeaderValueOr(ResponseHeader.AccessControlAllowOrigin, null));
            Assert.Null(allFilteredResponse.Item1.HeaderValueOr(ResponseHeader.AccessControlAllowHeaders, null));
            Assert.Null(allFilteredResponse.Item1.HeaderValueOr(ResponseHeader.AccessControlAllowMethods, null));
          }
        
        [Fact]
        public void TestThatServerStartsWithFilters()
        {
            var world = World.StartWithDefaults("filters");

            var resource =
                ConfigurationResource.Defining("profile", typeof(ProfileResource), 5,
                    Actions.CanBe("PUT", "/users/{userId}/profile", "define(string userId, body:Vlingo.Xoom.Http.Tests.Sample.User.ProfileData profileData)", "Vlingo.Xoom.Http.Tests.Sample.User.ProfileDataMapper")
                        .Also("GET", "/users/{userId}/profile", "query(string userId)", "Vlingo.Xoom.Http.Tests.Sample.User.ProfileDataMapper").ThatsAll(), world.DefaultLogger);

            var server =
                ServerFactory.StartWith(
                    world.Stage,
                    Resources.Are(resource),
                    Filters.Are(new []{new RequestFilter1()}, Filters.NoResponseFilters()),
                    PortToUse.IncrementAndGet(),
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
        public override (Response, bool) Filter(Request request, Response response) => (response, ++Count < 5);
    }
}
// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;
using Action = Vlingo.Http.Resource.Action;

namespace Vlingo.Http.Tests.Resource
{
    public class ActionTest
    {
        [Fact]
        public void TestMatchesNoParameters()
        {
            var action = new Action(0, "GET", "/users", "QueryUsers()", null);
    
            var matchResults = action.MatchWith(Method.Get, "/users".ToMatchableUri());
        
            Assert.True(matchResults.IsMatched);
            Assert.Equal(0, matchResults.ParameterCount);
            Assert.Same(action, matchResults.Action);
        }
        
        [Fact]
        public void TestMatchesOneParameterInBetween()
        {
            var action = new Action(0, "PATCH", "/users/{userId}/name", "ChangeName(string userId)", null);
    
            var matchResults = action.MatchWith(Method.Patch, "/users/1234567/name".ToMatchableUri());
        
            Assert.True(matchResults.IsMatched);
            Assert.Same(action, matchResults.Action);
            Assert.Equal(1, matchResults.ParameterCount);
            Assert.Equal("userId", matchResults.Parameters[0].Name);
            Assert.Equal("1234567", matchResults.Parameters[0].Value);
        }
        
        [Fact]
        public void TestMatchesOneParameterLastPosition()
        {
            var action = new Action(0, "GET", "/users/{userId}", "QueryUser(string userId)", null);
    
            var matchResults = action.MatchWith(Method.Get, "/users/1234567".ToMatchableUri());
        
            Assert.True(matchResults.IsMatched);
            Assert.Same(action, matchResults.Action);
            Assert.Equal(1, matchResults.ParameterCount);
            Assert.Equal("userId", matchResults.Parameters[0].Name);
            Assert.Equal("1234567", matchResults.Parameters[0].Value);
        }
        
        [Fact]
        public void TestMatchesMultipleParameters()
        {
            var action =
                new Action(
                0,
                "GET",
                "/catalogs/{catalogId}/products/{productId}/details/{detailId}",
                "queryCatalogProductDetails(String catalogId, String productId, String detailId)",
                null);
    
            var matchResults = action.MatchWith(Method.Get, "/catalogs/123/products/4567/details/890".ToMatchableUri());
        
            Assert.True(matchResults.IsMatched);
            Assert.Same(action, matchResults.Action);
            Assert.Equal(3, matchResults.ParameterCount);
            Assert.Equal("catalogId", matchResults.Parameters[0].Name);
            Assert.Equal("123", matchResults.Parameters[0].Value);
            Assert.Equal("productId", matchResults.Parameters[1].Name);
            Assert.Equal("4567", matchResults.Parameters[1].Value);
            Assert.Equal("detailId", matchResults.Parameters[2].Name);
            Assert.Equal("890", matchResults.Parameters[2].Value);
        }
        
        [Fact]
        public void TestMatchesOneParameterWithEndSlash()
        {
            var action = new Action(0, "GET", "/users/{userId}/", "QueryUser(string userId)", null);
    
            var matchResults = action.MatchWith(Method.Get, "/users/1234567/".ToMatchableUri());
        
            Assert.True(matchResults.IsMatched);
            Assert.Same(action, matchResults.Action);
            Assert.Equal(1, matchResults.ParameterCount);
            Assert.Equal("userId", matchResults.Parameters[0].Name);
            Assert.Equal("1234567", matchResults.Parameters[0].Value);
        }
        
        [Fact]
        public void TestMatchesMultipleParametersWithSimilarUri()
        {
            var shortAction =
                new Action(
                0,
                "GET",
                "/o/{oId}/u/{uId}/foo",
                "foo(string oId, string uId, string uId)",
                null);

            var longAction =
                new Action(
                    0,
                    "GET",
                    "/o/{oId}/u/{uId}/c/{cId}/foo",
                    "foo(string oId, string uId, string uId, string cId)",
                    null);

            var matchResultsShortUriToShortAction = shortAction.MatchWith(Method.Get, "/o/1/u/2/foo".ToMatchableUri());
            var matchResultsLongUriToShortAction = shortAction.MatchWith(Method.Get, "/o/1/u/2/c/3/foo".ToMatchableUri());

            var matchResultsShortUriToLongAction = longAction.MatchWith(Method.Get, "/o/1/u/2/foo".ToMatchableUri());
            var matchResultsLongUriToLongAction = longAction.MatchWith(Method.Get, "/o/1/u/2/c/3/foo".ToMatchableUri());


            Assert.True(matchResultsShortUriToShortAction.IsMatched);
            Assert.False(matchResultsLongUriToShortAction.IsMatched);
            Assert.False(matchResultsShortUriToLongAction.IsMatched);
            Assert.True(matchResultsLongUriToLongAction.IsMatched);
        }
        
        [Fact]
        public void TestMatchesMultipleParametersWithEndSlash()
        {
            var action =
                new Action(
                0,
                "GET",
                "/users/{userId}/emailAddresses/{emailAddressId}/",
                "queryUserEmailAddress(String userId, String emailAddressId)",
                null);
    
            var matchResults = action.MatchWith(Method.Get, "/users/1234567/emailAddresses/890/".ToMatchableUri());
        
            Assert.True(matchResults.IsMatched);
            Assert.Same(action, matchResults.Action);
            Assert.Equal(2, matchResults.ParameterCount);
            Assert.Equal("userId", matchResults.Parameters[0].Name);
            Assert.Equal("1234567", matchResults.Parameters[0].Value);
            Assert.Equal("emailAddressId", matchResults.Parameters[1].Name);
            Assert.Equal("890", matchResults.Parameters[1].Value);
        }
        
        [Fact]
        public void TestNoMatchMethod()
        {
            var action = new Action(0, "GET", "/users/all", "queryUsers()", null);
    
            var matchResults = action.MatchWith(Method.Post, "/users".ToMatchableUri());
        
            Assert.False(matchResults.IsMatched);
            Assert.Null(matchResults.Action);
            Assert.Equal(0, matchResults.ParameterCount);
        }
        
        [Fact]
        public void TestNoMatchNoParameters()
        {
            var action = new Action(0, "GET", "/users/all", "queryUsers()", null);
    
            var matchResults = action.MatchWith(Method.Get, "/users/one".ToMatchableUri());
        
            Assert.False(matchResults.IsMatched);
            Assert.Null(matchResults.Action);
            Assert.Equal(0, matchResults.ParameterCount);
        }
        
        [Fact]
        public void TestNoMatchGivenAdditionalElements()
        {
            var action = new Action(0, "GET", "/users/{id}", "queryUsers(String userId)", null);

            var matchResults = action.MatchWith(Method.Get, "/users/1234/extra".ToMatchableUri());

            Assert.False(matchResults.IsMatched);
            Assert.Null(matchResults.Action);
            Assert.Equal(0, matchResults.ParameterCount);
        }
        
        [Fact]
        public void TestNoMatchEmptyParam()
        {
            var action = new Action(0, "GET", "/users/{id}/data", "queryUserData(String userId)", null);

            var matchResults = action.MatchWith(Method.Get, "/users//data".ToMatchableUri());

            Assert.False(matchResults.IsMatched);
            Assert.Same(action, matchResults.Action);
            Assert.Equal(0, matchResults.ParameterCount);
        }
        
        [Fact]
        public void TestMatchEmptyParamGivenAllowsTrailingSlash()
        {
            var action = new Action(0, "GET", "/users/{id}", "queryUsers(String userId)", null);

            var matchResults = action.MatchWith(Method.Get, "/users//".ToMatchableUri());

            Assert.True(matchResults.IsMatched);
            Assert.Same(action, matchResults.Action);
            Assert.Equal(1, matchResults.ParameterCount);
        }
        
        [Fact]
        public void TestNoMatchMultipleParametersMissingSlash()
        {
            var action =
                new Action(
                0,
                "GET",
                "/users/{userId}/emailAddresses/{emailAddressId}/",
                "queryUserEmailAddress(String userId, String emailAddressId)",
                null);
    
            var matchResults = action.MatchWith(Method.Get, "/users/1234567/emailAddresses/890".ToMatchableUri());
        
            Assert.False(matchResults.IsMatched);
            Assert.Null(matchResults.Action);
            Assert.Equal(0, matchResults.ParameterCount);
        }
        
        [Fact]
        public void TestWeirdMatchMultipleParametersNoSlash()
        {
            var action =
                new Action(
                0,
                "GET",
                "/users/{userId}/emailAddresses/{emailAddressId}",
                "queryUserEmailAddress(String userId, String emailAddressId)",
                null);
    
            var matchResults = action.MatchWith(Method.Get, "/users/1234567/emailAddresses/890/".ToMatchableUri());
        
            Assert.True(matchResults.IsMatched);
            Assert.Same(action, matchResults.Action);
            Assert.Equal(2, matchResults.ParameterCount);
            Assert.NotEqual("890", matchResults.Parameters[1].Value);
            Assert.Equal("890/", matchResults.Parameters[1].Value); // TODO: may watch for last "/" or add optional configuration
        }
        
        [Fact]
        public void TestWithQueryParameters()
        {
            var action =
                new Action(
                0,
                "GET",
                "/users/{userId}",
                "queryUser(String userId)",
                null);

            var uri = "/users/1234567?one=1.1&two=2.0&three=three*&three=3.3".ToMatchableUri();
            
            var matchResults = action.MatchWith(Method.Get, uri);
            Assert.True(matchResults.IsMatched);
            Assert.Same(action, matchResults.Action);
            Assert.Equal(1, matchResults.ParameterCount);
            Assert.Equal("1234567", matchResults.Parameters[0].Value);

            var queryParameters = new QueryParameters(uri.Query);
            Assert.Equal(3, queryParameters.Names.Count);
            Assert.Equal(1, queryParameters.ValuesOf("one").Count);
            Assert.Equal("1.1", queryParameters.ValuesOf("one")[0]);
            Assert.Equal(1, queryParameters.ValuesOf("two").Count);
            Assert.Equal("2.0", queryParameters.ValuesOf("two")[0]);
            Assert.Equal(2, queryParameters.ValuesOf("three").Count);
            Assert.Equal("three*", queryParameters.ValuesOf("three")[0]);
            Assert.Equal("3.3", queryParameters.ValuesOf("three")[1]);
        }
    }
}
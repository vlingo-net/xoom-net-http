// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using Vlingo.Http.Resource;
using Vlingo.Http.Resource.Serialization;
using Vlingo.Http.Tests.Sample.User;
using Xunit;
using Xunit.Abstractions;
using Action = Vlingo.Http.Resource.Action;

namespace Vlingo.Http.Tests.Resource
{
    public class ConfigurationResourceTest : ResourceTestFixtures
    {
        [Fact]
        public void TestThatPostRegisterUserDispatches() {
            var request = Request.From(Encoding.UTF8.GetBytes(PostJohnDoeUserMessage));
            var completes = new MockCompletesEventuallyResponse();

            var withCalls = completes.ExpectWithTimes(1);
            _dispatcher.DispatchFor(new Context(request, completes));
            withCalls.ReadFrom<int>("completed");

            Assert.NotNull(completes.Response);

            Assert.Equal(Response.ResponseStatus.Created, completes.Response.Status);
            Assert.Equal(2, completes.Response.Headers.Count);
            Assert.Equal(ResponseHeader.Location, completes.Response.Headers[0].Name);
            Assert.StartsWith("/users/", completes.Response.HeaderOf(ResponseHeader.Location).Value);
            Assert.NotNull(completes.Response.Entity);

            var createdUserData = (UserData)JsonSerialization.Deserialized(completes.Response.Entity.Content, typeof(UserData));
            Assert.NotNull(createdUserData);
            Assert.Equal(JohnDoeUserData.NameData.Given, createdUserData.NameData.Given);
            Assert.Equal(JohnDoeUserData.NameData.Family, createdUserData.NameData.Family);
            Assert.Equal(JohnDoeUserData.ContactData.EmailAddress, createdUserData.ContactData.EmailAddress);
            Assert.Equal(JohnDoeUserData.ContactData.TelephoneNumber, createdUserData.ContactData.TelephoneNumber);
        }
        
        [Fact]
        public void TestThatGetUserDispatches()
        {
            var postRequest = Request.From(Encoding.UTF8.GetBytes(PostJohnDoeUserMessage));
            var postCompletes = new MockCompletesEventuallyResponse();
            var postCompletesWithCalls = postCompletes.ExpectWithTimes(1);
            _dispatcher.DispatchFor(new Context(postRequest, postCompletes));
            postCompletesWithCalls.ReadFrom<int>("completed");
            Assert.NotNull(postCompletes.Response);

            var getUserMessage = $"GET {postCompletes.Response.HeaderOf(ResponseHeader.Location).Value} HTTP/1.1\nHost: vlingo.io\n\n";
            var getRequest = Request.From(Encoding.UTF8.GetBytes(getUserMessage));
            var getCompletes = new MockCompletesEventuallyResponse();
            var getCompletesWithCalls = getCompletes.ExpectWithTimes(1);
            _dispatcher.DispatchFor(new Context(getRequest, getCompletes));
            getCompletesWithCalls.ReadFrom<int>("completed");
            Assert.NotNull(getCompletes.Response);
            Assert.Equal(Response.ResponseStatus.Ok, getCompletes.Response.Status);
            var getUserData = JsonSerialization.Deserialized<UserData>(getCompletes.Response.Entity.Content);
            Assert.NotNull(getUserData);
            Assert.Equal(JohnDoeUserData.NameData.Given, getUserData.NameData.Given);
            Assert.Equal(JohnDoeUserData.NameData.Family, getUserData.NameData.Family);
            Assert.Equal(JohnDoeUserData.ContactData.EmailAddress, getUserData.ContactData.EmailAddress);
            Assert.Equal(JohnDoeUserData.ContactData.TelephoneNumber, getUserData.ContactData.TelephoneNumber);
        }
        
        [Fact]
        public void TestThatGetAllUsersDispatches()
        {
            var postRequest1 = Request.From(Encoding.UTF8.GetBytes(PostJohnDoeUserMessage));
            var postCompletes1 = new MockCompletesEventuallyResponse();

            var postCompletes1WithCalls = postCompletes1.ExpectWithTimes(1);
            _dispatcher.DispatchFor(new Context(postRequest1, postCompletes1));
            postCompletes1WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(postCompletes1.Response);
            var postRequest2 = Request.From(Encoding.UTF8.GetBytes(PostJaneDoeUserMessage));
            var postCompletes2 = new MockCompletesEventuallyResponse();

            var postCompletes2WithCalls = postCompletes2.ExpectWithTimes(1);
            _dispatcher.DispatchFor(new Context(postRequest2, postCompletes2));
            postCompletes2WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(postCompletes2.Response);

            var getUserMessage = "GET /users HTTP/1.1\nHost: vlingo.io\n\n";
            var getRequest = Request.From(Encoding.UTF8.GetBytes(getUserMessage));
            var getCompletes = new MockCompletesEventuallyResponse();

            var getCompletesWithCalls = getCompletes.ExpectWithTimes(1);
            _dispatcher.DispatchFor(new Context(getRequest, getCompletes));
            getCompletesWithCalls.ReadFrom<int>("completed");

            Assert.NotNull(getCompletes.Response);
            Assert.Equal(Response.ResponseStatus.Ok, getCompletes.Response.Status);
            var getUserData = JsonSerialization.DeserializedList<UserData>(getCompletes.Response.Entity.Content);
            Assert.NotNull(getUserData);

            var johnUserData = UserData.UserAt(postCompletes1.Response.HeaderOf(ResponseHeader.Location).Value, getUserData);

            Assert.Equal(JohnDoeUserData.NameData.Given, johnUserData.NameData.Given);
            Assert.Equal(JohnDoeUserData.NameData.Family, johnUserData.NameData.Family);
            Assert.Equal(JohnDoeUserData.ContactData.EmailAddress, johnUserData.ContactData.EmailAddress);
            Assert.Equal(JohnDoeUserData.ContactData.TelephoneNumber, johnUserData.ContactData.TelephoneNumber);

            var janeUserData = UserData.UserAt(postCompletes2.Response.HeaderOf(ResponseHeader.Location).Value, getUserData);

            Assert.Equal(JaneDoeUserData.NameData.Given, janeUserData.NameData.Given);
            Assert.Equal(JaneDoeUserData.NameData.Family, janeUserData.NameData.Family);
            Assert.Equal(JaneDoeUserData.ContactData.EmailAddress, janeUserData.ContactData.EmailAddress);
            Assert.Equal(JaneDoeUserData.ContactData.TelephoneNumber, janeUserData.ContactData.TelephoneNumber);
        }
        
        [Fact]
        public void TestThatPatchNameWorks()
        {
            var postRequest1 = Request.From(Encoding.UTF8.GetBytes(PostJohnDoeUserMessage));
            var postCompletes1 = new MockCompletesEventuallyResponse();
            var postCompletes1WithCalls = postCompletes1.ExpectWithTimes(1);
            _dispatcher.DispatchFor(new Context(postRequest1, postCompletes1));
            postCompletes1WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(postCompletes1.Response);

            var postRequest2 = Request.From(Encoding.UTF8.GetBytes(PostJaneDoeUserMessage));
            var postCompletes2 = new MockCompletesEventuallyResponse();

            var postCompletes2WithCalls = postCompletes2.ExpectWithTimes(1);
            _dispatcher.DispatchFor(new Context(postRequest2, postCompletes2));
            postCompletes2WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(postCompletes2.Response);

            // John Doe and Jane Doe marry and change their family name to, of course, Doe-Doe
            var johnNameData = NameData.From("John", "Doe-Doe");
            var johnNameSerialized = JsonSerialization.Serialized(johnNameData);
            var patchJohnDoeUserMessage =
                $"PATCH {postCompletes1.Response.HeaderOf(ResponseHeader.Location).Value}" +
                $"/name HTTP/1.1\nHost: vlingo.io\nContent-Length: {johnNameSerialized.Length}" +
                $"\n\n{johnNameSerialized}";

            var patchRequest1 = Request.From(Encoding.UTF8.GetBytes(patchJohnDoeUserMessage));
            var patchCompletes1 = new MockCompletesEventuallyResponse();

            var patchCompletes1WithCalls = patchCompletes1.ExpectWithTimes(1);
            _dispatcher.DispatchFor(new Context(patchRequest1, patchCompletes1));
            patchCompletes1WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(patchCompletes1.Response);
            Assert.Equal(Response.ResponseStatus.Ok, patchCompletes1.Response.Status);
            var getJohnDoeDoeUserData = JsonSerialization.Deserialized<UserData>(patchCompletes1.Response.Entity.Content);
            Assert.Equal(johnNameData.Given, getJohnDoeDoeUserData.NameData.Given);
            Assert.Equal(johnNameData.Family, getJohnDoeDoeUserData.NameData.Family);
            Assert.Equal(JohnDoeUserData.ContactData.EmailAddress, getJohnDoeDoeUserData.ContactData.EmailAddress);
            Assert.Equal(JohnDoeUserData.ContactData.TelephoneNumber, getJohnDoeDoeUserData.ContactData.TelephoneNumber);

            var janeNameData = NameData.From("Jane", "Doe-Doe");
            var janeNameSerialized = JsonSerialization.Serialized(janeNameData);
            var patchJaneDoeUserMessage =
                $"PATCH {postCompletes2.Response.HeaderOf(ResponseHeader.Location).Value}" +
                $"/name HTTP/1.1\nHost: vlingo.io\nContent-Length: {janeNameSerialized.Length}" +
                $"\n\n{janeNameSerialized}";

            var patchRequest2 = Request.From(Encoding.UTF8.GetBytes(patchJaneDoeUserMessage));
            var patchCompletes2 = new MockCompletesEventuallyResponse();

            var patchCompletes2WithCalls = patchCompletes2.ExpectWithTimes(1);
            _dispatcher.DispatchFor(new Context(patchRequest2, patchCompletes2));
            patchCompletes2WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(patchCompletes2.Response);
            Assert.Equal(Response.ResponseStatus.Ok, patchCompletes2.Response.Status);
            var getJaneDoeDoeUserData = JsonSerialization.Deserialized<UserData>(patchCompletes2.Response.Entity.Content);
            Assert.Equal(JaneDoeUserData.NameData.Given, getJaneDoeDoeUserData.NameData.Given);
            Assert.Equal(JaneDoeUserData.NameData.Family, getJaneDoeDoeUserData.NameData.Family);
            Assert.Equal(JaneDoeUserData.ContactData.EmailAddress, getJaneDoeDoeUserData.ContactData.EmailAddress);
            Assert.Equal(JaneDoeUserData.ContactData.TelephoneNumber, getJaneDoeDoeUserData.ContactData.TelephoneNumber);
        }
        
        [Fact]
        public void TestThatAllWellOrderedActionHaveMatches()
        {
            var actionGetUsersMatch = _resource.MatchWith(Method.GET, new Uri("/users").ToMatchableUri());
            Assert.True(actionGetUsersMatch.IsMatched);
            Assert.Equal(_actionGetUsers, actionGetUsersMatch.Action);

            var actionGetUserMatch = _resource.MatchWith(Method.GET, new Uri("/users/1234567").ToMatchableUri());
            Assert.True(actionGetUserMatch.IsMatched);
            Assert.Equal(_actionGetUser, actionGetUserMatch.Action);

            var actionPatchUserNameMatch = _resource.MatchWith(Method.PATCH, new Uri("/users/1234567/name").ToMatchableUri());
            Assert.True(actionPatchUserNameMatch.IsMatched);
            Assert.Equal(_actionPatchUserName, actionPatchUserNameMatch.Action);

            var actionPostUserMatch = _resource.MatchWith(Method.POST, new Uri("/users").ToMatchableUri());
            Assert.True(actionPostUserMatch.IsMatched);
            Assert.Equal(_actionPostUser, actionPostUserMatch.Action);
        }
        
        [Fact]
        public void TestThatAllPoorlyOrderedActionHaveMatches()
        {
            _actionPostUser = new Action(0, "POST", "/users", "register(body:Vlingo.Http.Tests.Sample.User.UserData userData)", null, true);
            _actionPatchUserContact = new Action(1, "PATCH", "/users/{userId}/contact", "changeContact(string userId, body:Vlingo.Http.Tests.Sample.User.ContactData contactData)", null, true);
            _actionPatchUserName = new Action(2, "PATCH", "/users/{userId}/name", "changeName(string userId, body:Vlingo.Http.Tests.Sample.User.NameData nameData)", null, true);
            _actionGetUsers = new Action(3, "GET", "/users", "queryUsers()", null, true);
            _actionGetUser = new Action(4, "GET", "/users/{userId}", "queryUser(string userId)", null, true);
            var actionGetUserEmailAddress = new Action(5, "GET", "/users/{userId}/emailAddresses/{emailAddressId}", "queryUserEmailAddress(string userId, string emailAddressId)", null, true);

            //=============================================================
            // this test assures that the optional feature used in the
            // Action.MatchResults constructor is enabled and short circuits
            // a match if any parameters contain a "/", which would normally
            // mean that the Action that appeared to match didn't have
            // enough Matchable PathSegments. Look for the following in
            // Action.MatchResult(): disallowPathParametersWithSlash
            // see the above testThatAllWellOrderedActionHaveMatches() for
            // a better ordering of actions and one that does not use the
            // disallowPathParametersWithSlash option.
            // See also: vlingo-http.properties
            //   userResource.NAME.disallowPathParametersWithSlash = true/false
            //=============================================================

            var actions = new List<Action>
                                {
                                    _actionPostUser,
                                    _actionPatchUserContact,
                                    _actionPatchUserName,
                                    _actionGetUsers, // order is problematic unless parameter matching short circuit used
                                    _actionGetUser,
                                    actionGetUserEmailAddress
                                };

            var resource = ConfigurationResource<UserResource>.NewResourceFor("user", _resourceHandlerType, 5, actions);

            var actionGetUsersMatch = resource.MatchWith(Method.GET, new Uri("/users").ToMatchableUri());
            Assert.True(actionGetUsersMatch.IsMatched);
            Assert.Equal(_actionGetUsers, actionGetUsersMatch.Action);

            var actionGetUserMatch = resource.MatchWith(Method.GET, new Uri("/users/1234567").ToMatchableUri());
            Assert.True(actionGetUserMatch.IsMatched);
            Assert.Equal(_actionGetUser, actionGetUserMatch.Action);

            var actionGetUserEmailAddressMatch = resource.MatchWith(Method.GET, new Uri("/users/1234567/emailAddresses/890").ToMatchableUri());
            Assert.True(actionGetUserEmailAddressMatch.IsMatched);
            Assert.Equal(actionGetUserEmailAddress, actionGetUserEmailAddressMatch.Action);

            var actionPatchUserNameMatch = resource.MatchWith(Method.PATCH, new Uri("/users/1234567/name").ToMatchableUri());
            Assert.True(actionPatchUserNameMatch.IsMatched);
            Assert.Equal(_actionPatchUserName, actionPatchUserNameMatch.Action);

            var actionPostUserMatch = resource.MatchWith(Method.POST, new Uri("/users").ToMatchableUri());
            Assert.True(actionPostUserMatch.IsMatched);
            Assert.Equal(_actionPostUser, actionPostUserMatch.Action);
        }
        
        public ConfigurationResourceTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}
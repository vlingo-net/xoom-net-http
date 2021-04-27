// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Vlingo.Xoom.Actors.Plugin.Logging.Console;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Http.Tests.Sample.User;
using Vlingo.Xoom.Http.Tests.Sample.User.Serialization;
using Vlingo.Xoom.Wire.Message;
using Xunit;
using Xunit.Abstractions;
using Action = Vlingo.Xoom.Http.Resource.Action;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class ConfigurationResourceTest : ResourceTestFixtures
    {
        private readonly ITestOutputHelper _output;
        private readonly JsonSerializerSettings _settings;

        [Fact]
        public void TestThatPostRegisterUserDispatches()
        {
            var request = Request.From(ConsumerByteBuffer(PostJohnDoeUserMessage));
            var completes = new MockCompletesEventuallyResponse();

            var withCalls = completes.ExpectWithTimes(1);
            Dispatcher.DispatchFor(new Context(request, completes));
            withCalls.ReadFrom<int>("completed");

            Assert.NotNull(completes.Response);

            Assert.Equal(ResponseStatus.Created, completes.Response.Get().Status);
            Assert.Equal(2, completes.Response.Get().Headers.Count);
            Assert.Equal(ResponseHeader.Location, completes.Response.Get().Headers[0].Name);
            Assert.StartsWith("/users/", completes.Response.Get().HeaderOf(ResponseHeader.Location).Value);
            Assert.NotNull(completes.Response.Get().Entity);

            var createdUserData = (UserData)JsonSerialization.Deserialized(completes.Response.Get().Entity.Content, typeof(UserData), _settings);
            Assert.NotNull(createdUserData);
            Assert.Equal(JohnDoeUserData.NameData.Given, createdUserData.NameData.Given);
            Assert.Equal(JohnDoeUserData.NameData.Family, createdUserData.NameData.Family);
            Assert.Equal(JohnDoeUserData.ContactData.EmailAddress, createdUserData.ContactData.EmailAddress);
            Assert.Equal(JohnDoeUserData.ContactData.TelephoneNumber, createdUserData.ContactData.TelephoneNumber);
        }

        [Fact]
        public void TestThatGetUserDispatches()
        {
            var postRequest = Request.From(ConsumerByteBuffer(PostJohnDoeUserMessage));
            var postCompletes = new MockCompletesEventuallyResponse();
            var postCompletesWithCalls = postCompletes.ExpectWithTimes(1);
            Dispatcher.DispatchFor(new Context(postRequest, postCompletes));
            postCompletesWithCalls.ReadFrom<int>("completed");
            Assert.NotNull(postCompletes.Response);

            var getUserMessage = $"GET {postCompletes.Response.Get().HeaderOf(ResponseHeader.Location).Value} HTTP/1.1\nHost: vlingo.io\n\n";
            var getRequest = Request.From(ConsumerByteBuffer(getUserMessage));
            var getCompletes = new MockCompletesEventuallyResponse();
            var getCompletesWithCalls = getCompletes.ExpectWithTimes(1);
            Dispatcher.DispatchFor(new Context(getRequest, getCompletes));
            getCompletesWithCalls.ReadFrom<int>("completed");
            Assert.NotNull(getCompletes.Response);
            Assert.Equal(ResponseStatus.Ok, getCompletes.Response.Get().Status);
            var getUserData = JsonSerialization.Deserialized<UserData>(getCompletes.Response.Get().Entity.Content, _settings);
            Assert.NotNull(getUserData);
            Assert.Equal(JohnDoeUserData.NameData.Given, getUserData.NameData.Given);
            Assert.Equal(JohnDoeUserData.NameData.Family, getUserData.NameData.Family);
            Assert.Equal(JohnDoeUserData.ContactData.EmailAddress, getUserData.ContactData.EmailAddress);
            Assert.Equal(JohnDoeUserData.ContactData.TelephoneNumber, getUserData.ContactData.TelephoneNumber);
        }
        
        [Fact]
        public void TestThatGetAllUsersDispatches()
        {
            var postRequest1 = Request.From(ConsumerByteBuffer(PostJohnDoeUserMessage));
            var postCompletes1 = new MockCompletesEventuallyResponse();

            var postCompletes1WithCalls = postCompletes1.ExpectWithTimes(1);
            Dispatcher.DispatchFor(new Context(postRequest1, postCompletes1));
            postCompletes1WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(postCompletes1.Response);
            var postRequest2 = Request.From(ConsumerByteBuffer(PostJaneDoeUserMessage));
            var postCompletes2 = new MockCompletesEventuallyResponse();

            var postCompletes2WithCalls = postCompletes2.ExpectWithTimes(1);
            Dispatcher.DispatchFor(new Context(postRequest2, postCompletes2));
            postCompletes2WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(postCompletes2.Response);

            var getUserMessage = "GET /users HTTP/1.1\nHost: vlingo.io\n\n";
            var getRequest = Request.From(ConsumerByteBuffer(getUserMessage));
            var getCompletes = new MockCompletesEventuallyResponse();

            var getCompletesWithCalls = getCompletes.ExpectWithTimes(1);
            Dispatcher.DispatchFor(new Context(getRequest, getCompletes));
            getCompletesWithCalls.ReadFrom<int>("completed");

            Assert.NotNull(getCompletes.Response);
            Assert.Equal(ResponseStatus.Ok, getCompletes.Response.Get().Status);
            var getUserData = JsonSerialization.DeserializedList<UserData>(getCompletes.Response.Get().Entity.Content, _settings);
            Assert.NotNull(getUserData);

            var johnUserData = UserData.UserAt(postCompletes1.Response.Get().HeaderOf(ResponseHeader.Location).Value, getUserData);

            Assert.Equal(JohnDoeUserData.NameData.Given, johnUserData.NameData.Given);
            Assert.Equal(JohnDoeUserData.NameData.Family, johnUserData.NameData.Family);
            Assert.Equal(JohnDoeUserData.ContactData.EmailAddress, johnUserData.ContactData.EmailAddress);
            Assert.Equal(JohnDoeUserData.ContactData.TelephoneNumber, johnUserData.ContactData.TelephoneNumber);

            var janeUserData = UserData.UserAt(postCompletes2.Response.Get().HeaderOf(ResponseHeader.Location).Value, getUserData);

            Assert.Equal(JaneDoeUserData.NameData.Given, janeUserData.NameData.Given);
            Assert.Equal(JaneDoeUserData.NameData.Family, janeUserData.NameData.Family);
            Assert.Equal(JaneDoeUserData.ContactData.EmailAddress, janeUserData.ContactData.EmailAddress);
            Assert.Equal(JaneDoeUserData.ContactData.TelephoneNumber, janeUserData.ContactData.TelephoneNumber);
        }
        
        [Fact]
        public void TestThatPatchNameWorks()
        {
            _output.WriteLine("TestThatPatchNameWorks()");
            var postRequest1 = Request.From(ConsumerByteBuffer(PostJohnDoeUserMessage));
            var postCompletes1 = new MockCompletesEventuallyResponse();
            var postCompletes1WithCalls = postCompletes1.ExpectWithTimes(1);
            Dispatcher.DispatchFor(new Context(postRequest1, postCompletes1));
            postCompletes1WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(postCompletes1.Response);
            _output.WriteLine("1");

            var postRequest2 = Request.From(ConsumerByteBuffer(PostJaneDoeUserMessage));
            var postCompletes2 = new MockCompletesEventuallyResponse();

            var postCompletes2WithCalls = postCompletes2.ExpectWithTimes(1);
            Dispatcher.DispatchFor(new Context(postRequest2, postCompletes2));
            postCompletes2WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(postCompletes2.Response);
            _output.WriteLine("2");

            // John Doe and Jane Doe marry and change their family name to, of course, Doe-Doe
            var johnNameData = NameData.From("John", "Doe-Doe");
            var johnNameSerialized = JsonSerialization.Serialized(johnNameData);
            var patchJohnDoeUserMessage =
                $"PATCH {postCompletes1.Response.Get().HeaderOf(ResponseHeader.Location).Value}" +
                $"/name HTTP/1.1\nHost: vlingo.io\nContent-Length: {johnNameSerialized.Length}" +
                $"\n\n{johnNameSerialized}";

            _output.WriteLine($"2.0: {patchJohnDoeUserMessage}");
            var patchRequest1 = Request.From(ConsumerByteBuffer(patchJohnDoeUserMessage));
            var patchCompletes1 = new MockCompletesEventuallyResponse();

            var patchCompletes1WithCalls = patchCompletes1.ExpectWithTimes(1);
            Dispatcher.DispatchFor(new Context(patchRequest1, patchCompletes1));
            patchCompletes1WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(patchCompletes1.Response);
            Assert.Equal(ResponseStatus.Ok, patchCompletes1.Response.Get().Status);
            var getJohnDoeDoeUserData = JsonSerialization.Deserialized<UserData>(patchCompletes1.Response.Get().Entity.Content, _settings);
            Assert.Equal(johnNameData.Given, getJohnDoeDoeUserData.NameData.Given);
            Assert.Equal(johnNameData.Family, getJohnDoeDoeUserData.NameData.Family);
            Assert.Equal(JohnDoeUserData.ContactData.EmailAddress, getJohnDoeDoeUserData.ContactData.EmailAddress);
            Assert.Equal(JohnDoeUserData.ContactData.TelephoneNumber, getJohnDoeDoeUserData.ContactData.TelephoneNumber);

            var janeNameData = NameData.From("Jane", "Doe-Doe");
            var janeNameSerialized = JsonSerialization.Serialized(janeNameData);
            var patchJaneDoeUserMessage =
                $"PATCH {postCompletes2.Response.Get().HeaderOf(ResponseHeader.Location).Value}" +
                $"/name HTTP/1.1\nHost: vlingo.io\nContent-Length: {janeNameSerialized.Length}" +
                $"\n\n{janeNameSerialized}";

            var patchRequest2 = Request.From(ConsumerByteBuffer(patchJaneDoeUserMessage));
            var patchCompletes2 = new MockCompletesEventuallyResponse();

            var patchCompletes2WithCalls = patchCompletes2.ExpectWithTimes(1);
            Dispatcher.DispatchFor(new Context(patchRequest2, patchCompletes2));
            patchCompletes2WithCalls.ReadFrom<int>("completed");

            Assert.NotNull(patchCompletes2.Response);
            Assert.Equal(ResponseStatus.Ok, patchCompletes2.Response.Get().Status);
            var getJaneDoeDoeUserData = JsonSerialization.Deserialized<UserData>(patchCompletes2.Response.Get().Entity.Content);
            Assert.Equal(janeNameData.Given, getJaneDoeDoeUserData.NameData.Given);
            Assert.Equal(janeNameData.Family, getJaneDoeDoeUserData.NameData.Family);
            Assert.Equal(JaneDoeUserData.ContactData.EmailAddress, getJaneDoeDoeUserData.ContactData.EmailAddress);
            Assert.Equal(JaneDoeUserData.ContactData.TelephoneNumber, getJaneDoeDoeUserData.ContactData.TelephoneNumber);
        }
        
        [Fact]
        public void TestThatAllWellOrderedActionHaveMatches()
        {
            var actionGetUsersMatch = Resource.MatchWith(Method.Get, "/users".ToMatchableUri());
            Assert.True(actionGetUsersMatch.IsMatched);
            Assert.Equal(ActionGetUsers, actionGetUsersMatch.Action);

            var actionGetUserMatch = Resource.MatchWith(Method.Get, "/users/1234567".ToMatchableUri());
            Assert.True(actionGetUserMatch.IsMatched);
            Assert.Equal(ActionGetUser, actionGetUserMatch.Action);

            var actionPatchUserNameMatch = Resource.MatchWith(Method.Patch, "/users/1234567/name".ToMatchableUri());
            Assert.True(actionPatchUserNameMatch.IsMatched);
            Assert.Equal(ActionPatchUserName, actionPatchUserNameMatch.Action);

            var actionPostUserMatch = Resource.MatchWith(Method.Post, "/users".ToMatchableUri());
            Assert.True(actionPostUserMatch.IsMatched);
            Assert.Equal(ActionPostUser, actionPostUserMatch.Action);
        }
        
        [Fact]
        public void TestThatAllPoorlyOrderedActionHaveMatches()
        {
            ActionPostUser = new Action(0, "POST", "/users", "register(body:Vlingo.Xoom.Http.Tests.Sample.User.UserData userData)", null);
            ActionPatchUserContact = new Action(1, "PATCH", "/users/{userId}/contact", "changeContact(string userId, body:Vlingo.Xoom.Http.Tests.Sample.User.ContactData contactData)", null);
            ActionPatchUserName = new Action(2, "PATCH", "/users/{userId}/name", "changeName(string userId, body:Vlingo.Xoom.Http.Tests.Sample.User.NameData nameData)", null);
            ActionGetUsers = new Action(3, "GET", "/users", "queryUsers()", null);
            ActionGetUser = new Action(4, "GET", "/users/{userId}", "queryUser(string userId)", null);
            var actionGetUserEmailAddress = new Action(5, "GET", "/users/{userId}/emailAddresses/{emailAddressId}", "queryUserEmailAddress(string userId, string emailAddressId)", null);

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
                                    ActionPostUser,
                                    ActionPatchUserContact,
                                    ActionPatchUserName,
                                    ActionGetUsers, // order is problematic unless parameter matching short circuit used
                                    ActionGetUser,
                                    actionGetUserEmailAddress
                                };

            var resource = ConfigurationResource.NewResourceFor("user", ResourceHandlerType, 5, actions, ConsoleLogger.TestInstance());

            var actionGetUsersMatch = resource.MatchWith(Method.Get, "/users".ToMatchableUri());
            Assert.True(actionGetUsersMatch.IsMatched);
            Assert.Equal(ActionGetUsers, actionGetUsersMatch.Action);

            var actionGetUserMatch = resource.MatchWith(Method.Get, "/users/1234567".ToMatchableUri());
            Assert.True(actionGetUserMatch.IsMatched);
            Assert.Equal(ActionGetUser, actionGetUserMatch.Action);

            var actionGetUserEmailAddressMatch = resource.MatchWith(Method.Get, "/users/1234567/emailAddresses/890".ToMatchableUri());
            Assert.True(actionGetUserEmailAddressMatch.IsMatched);
            Assert.Equal(actionGetUserEmailAddress, actionGetUserEmailAddressMatch.Action);

            var actionPatchUserNameMatch = resource.MatchWith(Method.Patch, "/users/1234567/name".ToMatchableUri());
            Assert.True(actionPatchUserNameMatch.IsMatched);
            Assert.Equal(ActionPatchUserName, actionPatchUserNameMatch.Action);

            var actionPostUserMatch = resource.MatchWith(Method.Post, "/users".ToMatchableUri());
            Assert.True(actionPostUserMatch.IsMatched);
            Assert.Equal(ActionPostUser, actionPostUserMatch.Action);
        }
        
        public ConfigurationResourceTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new UserDataConverter());
        }
        
        private IConsumerByteBuffer ConsumerByteBuffer(string message)
        {
            var requestContent = Encoding.UTF8.GetBytes(message);
            var buffer = BasicConsumerByteBuffer.Allocate(3, requestContent.Length).Put(requestContent);
            buffer.Rewind();
            return buffer;
        }
    }
}
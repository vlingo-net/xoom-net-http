// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Http.Tests.Sample.User;
using Vlingo.Xoom.Http.Tests.Sample.User.Model;
using Vlingo.Xoom.Wire.Channel;
using Vlingo.Xoom.Wire.Message;
using Xunit.Abstractions;
using Action = Vlingo.Xoom.Http.Resource.Action;
using IDispatcher = Vlingo.Xoom.Http.Resource.IDispatcher;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public abstract class ResourceTestFixtures : IDisposable
    {
        public const string WorldName = "resource-test";
        protected Action ActionPostUser;
        protected Action ActionPutUser;
        protected Action ActionPatchUserContact;
        protected Action ActionPatchUserName;
        protected Action ActionGetUser;
        protected Action ActionGetUsers;
        protected readonly Action ActionGetUserError;
        
        protected readonly IConfigurationResource Resource;
        protected readonly Type ResourceHandlerType;
        protected readonly Resources Resources;
        protected readonly IDispatcher Dispatcher;
        protected readonly World World;
        
        protected UserData JohnDoeUserData { get; } = UserData.From(NameData.From("John", "Doe"), ContactData.From("john.doe@vlingo.io", "+1 212-555-1212"));

        protected string JohnDoeUserSerialized => JsonSerialization.Serialized(JohnDoeUserData);

        protected UserData JaneDoeUserData { get; } = UserData.From(NameData.From("Jane", "Doe"), ContactData.From("jane.doe@vlingo.io", "+1 212-555-1212"));

        protected string JaneDoeUserSerialized => JsonSerialization.Serialized(JaneDoeUserData);

        protected string PostJohnDoeUserMessage => $"POST /users HTTP/1.1\nHost: vlingo.io\nContent-Length: {JohnDoeUserSerialized.Length}\n\n{JohnDoeUserSerialized}";

        protected string PostJaneDoeUserMessage => $"POST /users HTTP/1.1\nHost: vlingo.io\nContent-Length: {JaneDoeUserSerialized.Length}\n\n{JaneDoeUserSerialized}";

        private readonly MemoryStream _buffer = new MemoryStream(65535);
        private readonly IConsumerByteBuffer _consumerByteBuffer = BasicConsumerByteBuffer.Allocate(1, 65535);

        private int _uniqueId = 1;
        
        protected MemoryStream ToStream(string requestContent)
        {
            _buffer.Clear();
            _buffer.Write(Converters.TextToBytes(requestContent));
            _buffer.Flip();
            return _buffer;
        }
        
        protected IConsumerByteBuffer ToByteBuffer(string requestContent)
        {
            _consumerByteBuffer.Clear();
            _consumerByteBuffer.Put(Converters.TextToBytes(requestContent));
            _consumerByteBuffer.Flip();
            return _consumerByteBuffer;
        }
        
        protected string CreatedResponse(string body) => $"HTTP/1.1 201 CREATED\nContent-Length: {body.Length}\n\n{body}";
        
        protected string GetUsersOriginHeader() => "GET /users HTTP/1.1\nHost: vlingo.io\nOrigin: hello.cors" + "\n\n";

        protected string PostRequestCloseFollowing(string body) => $"POST /users HTTP/1.1\nHost: vlingo.io\nContent-Length: {body.Length}\nConnection: close\n\n{body}";
        
        protected string PostRequest(string body) => $"POST /users HTTP/1.1\nHost: vlingo.io\nConnection: keep-alive\nContent-Length: {body.Length}\n\n{body}";
        
        protected string PutRequest(string userId, string body) => $"PUT /users/{userId} HTTP/1.1\nHost: vlingo.io\nConnection: keep-alive\nContent-Length: {body.Length}\n\n{body}";

        protected string GetExceptionRequest(string userId) => $"GET /users/{userId}/error HTTP/1.1\nHost: vlingo.io\nConnection: keep-alive\n\n";
        
        protected string JaneDoeCreated() => CreatedResponse(JaneDoeUserSerialized);

        protected string UniqueJaneDoe()
        {
            var unique =
                UserData.From(
                    $"{_uniqueId}",
                    NameData.From("Jane", "Doe"),
                    ContactData.From("jane.doe@vlingo.io", "+1 212-555-1212"));

            ++_uniqueId;

            string serialized = JsonSerialization.Serialized(unique);

            return serialized;
        }
        
        protected string UniqueJaneDoePostCreated() => CreatedResponse(UniqueJaneDoe());

        protected string UniqueJaneDoePostRequest() => PostRequestCloseFollowing(UniqueJaneDoe());

        protected string UniqueJohnDoe()
        {
            var id = $"{_uniqueId}";
            if (id.Length == 1) id = $"00{id}";
            if (id.Length == 2) id = $"0{id}";
            var unique =
                UserData.From(
                    id, //"" + uniqueId,
                    NameData.From("John", "Doe"),
                    ContactData.From("john.doe@vlingo.io", "+1 212-555-1212"));

            ++_uniqueId;

            var serialized = JsonSerialization.Serialized(unique);

            return serialized;
        }

        protected string JohnDoeCreated() => CreatedResponse(JohnDoeUserSerialized);

        protected string UniqueJohnDoePostCreated() => CreatedResponse(UniqueJohnDoe());

        protected string UniqueJohnDoePostRequest() => PostRequestCloseFollowing(UniqueJohnDoe());

        public ResourceTestFixtures(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);

            World = World.Start(WorldName);

            ActionPostUser = new Action(0, "POST", "/users", "Register(body:Vlingo.Xoom.Http.Tests.Sample.User.UserData userData)", "Vlingo.Xoom.Http.Tests.Sample.User.UserDataMapper");
            ActionPatchUserContact = new Action(1, "PATCH", "/users/{userId}/contact", "changeContact(string userId, body:Vlingo.Xoom.Http.Tests.Sample.User.ContactData contactData)", "Vlingo.Xoom.Http.Tests.Sample.User.UserDataMapper");
            ActionPatchUserName = new Action(2, "PATCH", "/users/{userId}/name", "changeName(string userId, body:Vlingo.Xoom.Http.Tests.Sample.User.NameData nameData)", "Vlingo.Xoom.Http.Tests.Sample.User.UserDataMapper");
            ActionGetUser = new Action(3, "GET", "/users/{userId}", "queryUser(string userId)", "Vlingo.Xoom.Http.Tests.Sample.User.UserDataMapper");
            ActionGetUsers = new Action(4, "GET", "/users", "queryUsers()", "Vlingo.Xoom.Http.Tests.Sample.User.UserDataMapper");
            ActionGetUserError = new Action(5, "GET", "/users/{userId}/error", "queryUserError(string userId)", "Vlingo.Xoom.Http.Tests.Sample.User.UserDataMapper");
            ActionPutUser = new Action(6, "PUT", "/users/{userId}", "changeUser(string userId, body:Vlingo.Xoom.Http.Tests.Sample.User.UserData userData)", "Vlingo.Xoom.Http.Tests.Sample.User.UserDataMapper");


            var actions = new List<Action> {
                ActionPostUser,
                ActionPatchUserContact,
                ActionPatchUserName,
                ActionGetUser,
                ActionGetUsers,
                ActionGetUserError,
                ActionPutUser
            };

            ResourceHandlerType = ConfigurationResource.NewResourceHandlerTypeFor("Vlingo.Xoom.Http.Tests.Sample.User.UserResource");

            Resource = ConfigurationResource.NewResourceFor("user", ResourceHandlerType, 7, actions, World.DefaultLogger);

            Resource.AllocateHandlerPool(World.Stage);

            var oneResource = new Dictionary<string, IResource>(1);

            oneResource.Add(Resource.Name, Resource);

            Resources = new Resources(oneResource);
            Dispatcher = new TestDispatcher(Resources, World.DefaultLogger);
        }

        public virtual void Dispose()
        {
            World.Terminate();

            UserRepository.Reset();
        }
    }
}
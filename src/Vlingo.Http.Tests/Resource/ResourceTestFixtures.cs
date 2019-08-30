// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Http.Resource;
using Vlingo.Http.Tests.Sample.User;
using Xunit.Abstractions;
using Action = Vlingo.Http.Resource.Action;
using IDispatcher = Vlingo.Http.Resource.IDispatcher;

namespace Vlingo.Http.Tests.Resource
{
    public abstract class ResourceTestFixtures : IDisposable
    {
        public const string WorldName = "resource-test";
        protected Action _actionPostUser;
        protected Action _actionPatchUserContact;
        protected Action _actionPatchUserName;
        protected Action _actionGetUser;
        protected Action _actionGetUsers;
        protected Action _actionGetUserError;
        
        protected ConfigurationResource<ResourceHandler> _resource;
        protected Type _resourceHandlerClass;
        protected Resources _resources;
        protected IDispatcher _dispatcher;
        protected World _world;

        public ResourceTestFixtures(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            _world = World.Start(WorldName);

            _actionPostUser = new Action(0, "POST", "/users", "register(body:io.vlingo.http.sample.user.UserData userData)", null, true);
            _actionPatchUserContact = new Action(1, "PATCH", "/users/{userId}/contact", "changeContact(String userId, body:io.vlingo.http.sample.user.ContactData contactData)", null, true);
            _actionPatchUserName = new Action(2, "PATCH", "/users/{userId}/name", "changeName(String userId, body:io.vlingo.http.sample.user.NameData nameData)", null, true);
            _actionGetUser = new Action(3, "GET", "/users/{userId}", "queryUser(String userId)", null, true);
            _actionGetUsers = new Action(4, "GET", "/users", "queryUsers()", null, true);
            _actionGetUserError = new Action(5, "GET", "/users/{userId}/error", "queryUserError(String userId)", null, true);


            var actions = new List<Action> {
                _actionPostUser,
                _actionPatchUserContact,
                _actionPatchUserName,
                _actionGetUser,
                _actionGetUsers,
                _actionGetUserError};

            _resourceHandlerClass = ConfigurationResource<ResourceHandler>.NewResourceHandlerTypeFor("io.vlingo.http.sample.user.UserResource");

            _resource = ConfigurationResource<ResourceHandler>.NewResourceFor("user", _resourceHandlerClass, 6, actions);

            _resource.AllocateHandlerPool(_world.Stage);

            var oneResource = new Dictionary<string, Resource<ResourceHandler>>(1);

            oneResource.Add(_resource.Name, _resource);

            _resources = new Resources(oneResource);
            _dispatcher = new TestDispatcher(_resources, _world.DefaultLogger);
        }

        public void Dispose()
        {
            _world.Terminate();

            UserRepository.Reset();
        }
    }
}
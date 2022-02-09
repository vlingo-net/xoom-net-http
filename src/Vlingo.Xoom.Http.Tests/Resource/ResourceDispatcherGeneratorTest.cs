// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors.Plugin.Logging.Console;
using Vlingo.Xoom.Http.Resource;
using Xunit;
using Xunit.Abstractions;
using Action = Vlingo.Xoom.Http.Resource.Action;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class ResourceDispatcherGeneratorTest
    {
        private readonly List<Action> _actions;
        private readonly IConfigurationResource _resource;

        [Fact]
        public void TestSourceCodeGeneration()
        {
            var generator = ResourceDispatcherGenerator.ForTest(_actions, false, ConsoleLogger.TestInstance());

            var result = generator.GenerateFor(_resource.ResourceHandlerClass);

            Assert.NotNull(result);
            Assert.NotNull(result.SourceFile);
            Assert.False(result.SourceFile.Exists);
            Assert.NotNull(result.FullyQualifiedClassName);
            Assert.NotNull(result.ClassName);
            Assert.NotNull(result.Source);
        }
        
        [Fact]
        public void TestSourceCodeGenerationWithPersistence()
        {
            var generator = ResourceDispatcherGenerator.ForTest(_actions, true, ConsoleLogger.TestInstance());

            var result = generator.GenerateFor(_resource.ResourceHandlerClass);

            Assert.NotNull(result);
            Assert.NotNull(result.SourceFile);
            Assert.True(result.SourceFile.Exists);
            Assert.NotNull(result.FullyQualifiedClassName);
            Assert.NotNull(result.ClassName);
            Assert.NotNull(result.Source);
        }
    
        public ResourceDispatcherGeneratorTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            var actionPostUser = new Action(0, "POST", "/users", "register(body:Vlingo.Xoom.Http.Tests.Sample.User.UserData userData)", null);
            var actionPatchUserContact = new Action(1, "PATCH", "/users/{userId}/contact", "changeContact(string userId, body:Vlingo.Xoom.Http.Tests.Sample.User.ContactData contactData)", null);
            var actionPatchUserName = new Action(2, "PATCH", "/users/{userId}/name", "changeName(string userId, body:Vlingo.Xoom.Http.Tests.Sample.User.NameData nameData)", null);
            var actionGetUser = new Action(3, "GET", "/users/{userId}", "queryUser(string userId)", null);
            var actionGetUsers = new Action(4, "GET", "/users", "queryUsers()", null);
            var actionQueryUserError = new Action(5, "GET", "/users/{userId}/error", "queryUserError(string userId)", null);
            var actionPutUser = new Action(6, "PUT", "/users/{userId}", "changeUser(string userId, body:Vlingo.Xoom.Http.Tests.Sample.User.UserData userData)", null);

            _actions = new List<Action>
            {
                actionPostUser,
                actionPatchUserContact,
                actionPatchUserName,
                actionGetUser,
                actionGetUsers,
                actionQueryUserError,
                actionPutUser
            };

            Type resourceHandlerClass;
            
            try
            {
                resourceHandlerClass = Type.GetType("Vlingo.Xoom.Http.Tests.Sample.User.UserResource");
            }
            catch (Exception)
            {
                resourceHandlerClass = ConfigurationResource.NewResourceHandlerTypeFor("Vlingo.Xoom.Http.Tests.Sample.User.UserResource");
            }
            
            _resource = ConfigurationResource.NewResourceFor("user", resourceHandlerClass, 5, _actions, ConsoleLogger.TestInstance());
        }
    }
}
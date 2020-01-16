// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Http.Resource;
using Vlingo.Http.Resource.Sse;
using Vlingo.Http.Tests.Sample.User;
using Xunit;
using Action = Vlingo.Http.Resource.Action;

namespace Vlingo.Http.Tests.Resource
{
    public class ResourcesTest
    {
        private readonly Resources _resources = Loader.LoadResources(HttpProperties.Instance);
        
        [Fact]
        public void TestLoadResources()
        {
            var user = (ConfigurationResource<UserResource>)_resources.ResourceOf("user");
    
            Assert.NotNull(user);
            Assert.Equal("user", user.Name);
            Assert.NotNull(user.ResourceHandlerClass);
            Assert.Equal("Vlingo.Http.Tests.Sample.User.UserResource", user.ResourceHandlerClass.FullName);
            Assert.Equal(10, user.HandlerPoolSize);
    
            var countUserActions = 0;

            foreach (var action in user.Actions)
            {
                ++countUserActions;
      
                Assert.True(action.Method.IsPost() || action.Method.IsPatch() || action.Method.IsGet());
      
                Assert.NotNull(action.Uri);
                Assert.NotNull(action.To);
                Assert.NotNull(action.Mapper);
            }

            Assert.Equal(6, countUserActions);
    
            var profile = (ConfigurationResource<ProfileResource>) _resources.ResourceOf("profile");
    
            Assert.NotNull(profile);
            Assert.Equal("profile", profile.Name);
            Assert.NotNull(profile.ResourceHandlerClass);
            Assert.Equal("Vlingo.Http.Tests.Sample.User.ProfileResource", profile.ResourceHandlerClass.FullName);
            Assert.Equal(5, profile.HandlerPoolSize);
    
            var countProfileActions = 0;

            foreach (var action in profile.Actions)
            {
                ++countProfileActions;
      
                Assert.True(action.Method.IsPut() || action.Method.IsGet());
      
                Assert.NotNull(action.Uri);
                Assert.NotNull(action.To);
                Assert.NotNull(action.Mapper);   
            }

            Assert.Equal(2, countProfileActions);
        }
        
        [Fact]
        public void TestLoadSseResources()
        {
            var allStream = (ConfigurationResource<SseStreamResource>)_resources.ResourceOf("all");
    
            Assert.NotNull(allStream);
            Assert.Equal("all", allStream.Name);
            Assert.NotNull(allStream.ResourceHandlerClass);
            Assert.Equal("Vlingo.Http.Resource.Sse.SseStreamResource", allStream.ResourceHandlerClass.FullName);
            Assert.Equal(10, allStream.HandlerPoolSize);
    
            Assert.Equal(2, allStream.Actions.Count);
            Assert.True(allStream.Actions[0].Method.IsGet());
            Assert.NotNull(allStream.Actions[0].Uri);
            Assert.Equal("/eventstreams/{streamName}", allStream.Actions[0].Uri);
            Assert.NotNull(allStream.Actions[0].To);
            Assert.NotNull(allStream.Actions[0].Mapper);
            Assert.True(allStream.Actions[1].Method.IsDelete());
            Assert.NotNull(allStream.Actions[1].Uri);
            Assert.Equal("/eventstreams/{streamName}/{id}", allStream.Actions[1].Uri);
            Assert.NotNull(allStream.Actions[1].To);
            Assert.NotNull(allStream.Actions[1].Mapper);
        }
        
        [Fact]
        public void TestThatResourcesBuildFluently()
        {
            var resources =
                Resources
                    .Are(ConfigurationResource<UserResource>.Defining("user", typeof(UserResource), 10,
            Actions.CanBe("POST", "/users", "register(body:Vlingo.Http.Tests.Sample.User.UserData userData)")
                .Also("PATCH", "/users/{userId}/contact", "changeContact(string userId, body:Vlingo.Http.Tests.Sample.User.ContactData contactData)")
                .Also("PATCH", "/users/{userId}/name", "changeName(string userId, body:Vlingo.Http.Tests.Sample.User.NameData nameData)")
                .Also("GET", "/users/{userId}", "queryUser(string userId)")
                .Also("GET", "/users", "queryUsers()")
                .ThatsAll()),
            ConfigurationResource<ProfileResource>.Defining("profile", typeof(ProfileResource), 5,
            Actions.CanBe("PUT", "/users/{userId}/profile", "define(string userId, body:Vlingo.Http.Tests.Sample.User.ProfileData profileData)", "Vlingo.Http.Tests.Sample.User.ProfileDataMapper")
                .Also("GET", "/users/{userId}/profile", "query(string userId)", "Vlingo.Http.Tests.Sample.User.ProfileDataMapper")
                .ThatsAll()));

            Assert.NotNull(resources.ResourceOf("user"));
            Assert.NotNull(resources.ResourceOf("profile"));
        }
        
        [Fact]
        public void TestThatWrongIdSequenceBreaks()
        {
            var actionPostUser = new Action(0, "POST", "/users", "register(body:Vlingo.Http.Tests.Sample.User.UserData userData)", null);
            var actionPatchUserContact = new Action(3, "PATCH", "/users/{userId}/contact", "changeContact(string userId, body:Vlingo.Http.Tests.Sample.User.ContactData contactData)", null);

            var actions = new List<Action> {actionPostUser, actionPatchUserContact};

            var resourceHandlerClass = ConfigurationResource<UserResource>.NewResourceHandlerTypeFor("Vlingo.Http.Tests.Sample.User.UserResource");
    
            Assert.Throws<ArgumentException>(() => ConfigurationResource<UserResource>.NewResourceFor("user", resourceHandlerClass, 5, actions));
        }
        
        [Fact]
        public void TestThatWrongIdOrderBreaks()
        {
            var actionPostUser = new Action(3, "POST", "/users", "register(body:Vlingo.Http.Tests.Sample.User.UserData userData)", null);
            var actionPatchUserContact = new Action(1, "PATCH", "/users/{userId}/contact", "changeContact(string userId, body:Vlingo.Http.Tests.Sample.User.ContactData contactData)", null);

            var actions = new List<Action> {actionPostUser, actionPatchUserContact};

            var resourceHandlerClass = ConfigurationResource<UserResource>.NewResourceHandlerTypeFor("Vlingo.Http.Tests.Sample.User.UserResource");
    
            Assert.Throws<ArgumentException>(() => ConfigurationResource<UserResource>.NewResourceFor("user", resourceHandlerClass, 5, actions));
        }
    }
}
// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Http.Resource;
using Vlingo.Http.Resource.Serialization;
using Vlingo.Http.Tests.Resource;
using Vlingo.Http.Tests.Sample.User.Model;
using Name = Vlingo.Http.Tests.Sample.User.Model.Name;

namespace Vlingo.Http.Tests.Sample.User
{
    public class UserResourceFluent : ResourceHandler
    {
        private readonly UserRepository _repository = UserRepository.Instance();
        private readonly Stage _stage;

        public UserResourceFluent(World world) => _stage = world.StageNamed("service");

        public ICompletes<Response> Register(UserData userData)
        {
            var userAddress =
                    ServerBootstrap.Instance.World.AddressFactory
                        .UniquePrefixedWith("u-"); // stage().world().addressFactory().uniquePrefixedWith("u-");
            var userState =
                UserStateFactory.From(
                    userAddress.IdString,
                    Name.From(userData.NameData.Given, userData.NameData.Family),
                    Contact.From(userData.ContactData.EmailAddress, userData.ContactData.TelephoneNumber));

            _stage.ActorFor<IUser>(Definition.Has<UserActor>(Definition.Parameters(userState)), userAddress);

            _repository.Save(userState);

            return Vlingo.Common.Completes.WithSuccess(
                Response.Of(
                    Response.ResponseStatus.Created,
                    Headers.Of(
                        ResponseHeader.Of(ResponseHeader.Location, UserLocation(userState.Id))),
                    JsonSerialization.Serialized(UserData.From(userState))));
        }

        public ICompletes<Response> ChangeContact(string userId, ContactData contactData)
        {
            return _stage.ActorOf<IUser>(Stage.World.AddressFactory.From(userId))
                .AndThenTo(user => user.WithContact(new Contact(contactData.EmailAddress, contactData.TelephoneNumber)))
                .OtherwiseConsume(noUser => Completes.With(Response.Of(Response.ResponseStatus.NotFound, UserLocation(userId))))
                .AndThenTo(userState => Vlingo.Common.Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(UserData.From(userState)))));
        }

        public ICompletes<Response> ChangeName(string userId, NameData nameData)
        {
            return _stage.ActorOf<IUser>(Stage.World.AddressFactory.From(userId))
                .AndThenTo(user => user.WithName(new Name(nameData.Given, nameData.Family)))
                .OtherwiseConsume(noUser => Completes.With(Response.Of(Response.ResponseStatus.NotFound, UserLocation(userId))))
                .AndThenTo(userState => {
                    _repository.Save(userState);
                    return Vlingo.Common.Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(UserData.From(userState))));
                });
        }

        public ICompletes<Response> QueryUser(string userId)
        {
            var userState = _repository.UserOf(userId);
            if (userState.DoesNotExist())
            {
                return Vlingo.Common.Completes.WithSuccess(Response.Of(Response.ResponseStatus.NotFound, UserLocation(userId)));
            }

            return Vlingo.Common.Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(UserData.From(userState))));
        }

        public void QueryUserError(string userId)
        {
            throw new Exception("Test exception");
        }

        public ICompletes<Response> QueryUsers()
        {
            var users = new List<UserData>();
            foreach (var userState in _repository.Users)
            {
                users.Add(UserData.From(userState));
            }
            return Vlingo.Common.Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(users)));
        }

        public override Http.Resource.Resource Routes()
        {
            return ResourceBuilder.Resource("user resource fluent api",
                    ResourceBuilder.Post("/users")
                        .Body<UserData>(typeof(UserData))
                        .Handle(Register),
                    ResourceBuilder.Patch("/users/{userId}/contact")
                .Param<string>()
                .Body<ContactData>(typeof(ContactData))
                .Handle(ChangeContact),
                    ResourceBuilder.Patch("/users/{userId}/name")
                .Param<string>()
                .Body<NameData>(typeof(NameData))
                .Handle(ChangeName),
                    ResourceBuilder.Get("/users/{userId}")
                .Param<string>()
                .Handle(QueryUser),
                    ResourceBuilder.Get("/users")
                .Handle(QueryUsers));
        }

        private string UserLocation(string userId) => $"/users/{userId}";
    }
}
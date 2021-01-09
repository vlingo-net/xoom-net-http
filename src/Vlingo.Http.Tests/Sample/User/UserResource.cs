// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Common.Serialization;
using Vlingo.Http.Resource;
using Vlingo.Http.Tests.Sample.User.Model;

namespace Vlingo.Http.Tests.Sample.User
{
    public sealed class UserResource : ResourceHandler
    {
        private readonly UserRepository _repository = UserRepository.Instance();
        private readonly IAddressFactory _addressFactory;

        public UserResource(World world)
        {
            Stage = world.StageNamed("service");
            _addressFactory = Stage.AddressFactory;
        }

        public void Register(UserData userData)
        {
            var userAddress = _addressFactory.UniquePrefixedWith("u-");
            var userState =
                UserStateFactory.From(
                    userAddress.IdString,
                    Name.From(userData.NameData.Given, userData.NameData.Family),
                    Contact.From(userData.ContactData.EmailAddress, userData.ContactData.TelephoneNumber));

            Stage.ActorFor<IUser>(() => new UserActor(userState), userAddress);

            _repository.Save(userState);

            Completes.With(Response.Of(
                ResponseStatus.Created,
                Headers.Of(ResponseHeader.Of(ResponseHeader.Location, UserLocation(userState.Id))), 
                JsonSerialization.Serialized(UserData.From(userState))));
        }
        
        public void ChangeUser(string userId, UserData userData)
        {
            if (userId.EndsWith("123"))
            {
                Completes.With(Response.Of(ResponseStatus.PermanentRedirect, Headers.Of(ResponseHeader.Of(ResponseHeader.Location, "/"))));
            } 
            else
            {
                Completes.With(Response.Of(ResponseStatus.Ok));
            }
        }
        
        public void ChangeContact(string userId, ContactData contactData)
        {
            Stage.ActorOf<IUser>(_addressFactory.From(userId))
                .AndThenTo(user => user.WithContact(new Contact(contactData.EmailAddress, contactData.TelephoneNumber)))
                .OtherwiseConsume(noUser => Completes.With(Response.Of(ResponseStatus.NotFound, UserLocation(userId))))
                .AndThenConsume(userState => Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized(UserData.From(userState))));
        }

        public void ChangeName(string userId, NameData nameData)
        {
            Stage.ActorOf<IUser>(_addressFactory.From(userId))
                .AndThenTo(user => user.WithName(new Name(nameData.Given, nameData.Family)))
                .OtherwiseConsume(noUser => Completes.With(Response.Of(ResponseStatus.NotFound, UserLocation(userId))))
                .AndThenConsume(userState => {
                    _repository.Save(userState);
                    Completes.With(Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized(UserData.From(userState))));
            });
        }

        public void QueryUser(string userId)
        {
            var userState = _repository.UserOf(userId);
            if (userState.DoesNotExist())
            {
                Completes.With(Response.Of(ResponseStatus.NotFound, UserLocation(userId)));
            }
            else
            {
                Completes.With(Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized(UserData.From(userState))));
            }
        }

        public void QueryUserError(string userId) => throw new Exception("Test exception");

        public void QueryUsers()
        {
            var users = new List<UserData>();
            foreach (var userState in _repository.Users)
            {
                users.Add(UserData.From(userState));
            }
            
            Completes.With(Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized(users)));
        }
        
        private string UserLocation(string userId) => $"/users/{userId}";
    }
}
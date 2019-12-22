// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Http.Resource;
using Vlingo.Http.Resource.Serialization;
using Vlingo.Http.Tests.Sample.User.Model;

namespace Vlingo.Http.Tests.Sample.User
{
    public class ProfileResourceFluent : ResourceHandler
    {
        private ProfileRepository repository = ProfileRepository.Instance();
        private Stage stage;

        public ProfileResourceFluent(World world) => stage = world.StageNamed("service");

        public ICompletes<Response> Define(string userId, ProfileData profileData)
        {
            return stage.ActorOf<IProfile>(stage.World.AddressFactory.FindableBy(int.Parse(userId)))
                .AndThenTo(profile => {
                    var profileState = repository.ProfileOf(userId);
                    return Vlingo.Common.Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, Headers.Of(ResponseHeader.Of(ResponseHeader.Location, ProfileLocation(userId))),
                        JsonSerialization.Serialized(ProfileData.From(profileState))));
                })
                .Otherwise<Response>(noProfile => {
                    var profileState =
                        ProfileStateFactory.From(
                            userId,
                            profileData.TwitterAccount,
                            profileData.LinkedInAccount,
                            profileData.Website);

                    Stage.ActorFor<IProfile>(Definition.Has<ProfileActor>(Definition.Parameters(profileState)));

                repository.Save(profileState);
                return Response.Of(Response.ResponseStatus.Created, JsonSerialization.Serialized(ProfileData.From(profileState)));
            });
        }

        public ICompletes<Response> Query(string userId)
        {
            var profileState = repository.ProfileOf(userId);
            if (profileState.DoesNotExist)
            {
                return Vlingo.Common.Completes.WithSuccess(Response.Of(Response.ResponseStatus.NotFound, ProfileLocation(userId)));
            }

            return Vlingo.Common.Completes.WithSuccess(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(ProfileData.From(profileState))));
        }

        public override Http.Resource.Resource Routes()
        {
            return ResourceBuilder.Resource("profile resource fluent api",
                    ResourceBuilder.Put("/users/{userId}/profile")
                        .Param<string>(typeof(string))
                        .Body<ProfileData>(typeof(ProfileData))
                .Handle(Define),
                    ResourceBuilder.Get("/users/{userId}/profile")
                .Param<string>(typeof(string))
                .Handle(Query));
        }

        private string ProfileLocation(string userId) => "/users/" + userId + "/profile";
    }
}
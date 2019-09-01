// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Http.Resource;
using Vlingo.Http.Resource.Serialization;
using Vlingo.Http.Tests.Sample.User.Model;

namespace Vlingo.Http.Tests.Sample.User
{
    public class ProfileResource : ResourceHandler
    {
        private ProfileRepository _repository = ProfileRepository.Instance();

        public ProfileResource(World world) => _stage = world.StageNamed("service");
        
        public void Define(string userId, ProfileData profileData)
        {
            _stage.ActorOf<IProfile>(_stage.World.AddressFactory.FindableBy(int.Parse(userId)))
                .AndThenConsume(profile => {
                    var profileState = _repository.ProfileOf(userId);
                    Completes.With(Response.Of(
                        Response.ResponseStatus.Ok,
                        Headers.Of(ResponseHeader.Of(ResponseHeader.Location, ProfileLocation(userId))),
                        JsonSerialization.Serialized(ProfileData.From(profileState))));
            })
            .OtherwiseConsume(noProfile => {
                var profileState =
                    ProfileStateFactory.From(
                        userId,
                        profileData.TwitterAccount,
                        profileData.LinkedInAccount,
                        profileData.Website);
  
                _stage.ActorFor<IProfile>(Definition.Has<ProfileActor>(Definition.Parameters(profileState)));
  
                _repository.Save(profileState);
                Completes.With(Response.Of(Response.ResponseStatus.Created, JsonSerialization.Serialized(ProfileData.From(profileState))));
            });
        }
        
        public void Query(string userId)
        {
            var profileState = _repository.ProfileOf(userId);
            if (profileState.DoesNotExist)
            {
                Completes.With(Response.Of(Response.ResponseStatus.NotFound, ProfileLocation(userId)));
            }
            else
            {
                Completes.With(Response.Of(Response.ResponseStatus.Ok, JsonSerialization.Serialized(ProfileData.From(profileState))));
            }
        }

        private string ProfileLocation(string userId) => $"/users/{userId}/profile";
    }
}
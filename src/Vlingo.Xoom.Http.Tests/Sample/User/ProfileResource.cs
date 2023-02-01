// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Http.Tests.Sample.User.Model;

namespace Vlingo.Xoom.Http.Tests.Sample.User;

public sealed class ProfileResource : ResourceHandler
{
    private readonly ProfileRepository _repository = ProfileRepository.Instance();

    public ProfileResource(World world) => Stage = world.StageNamed("service");
        
    public void Define(string userId, ProfileData profileData)
    {
        Stage?.ActorOf<IProfile>(Stage.World.AddressFactory.FindableBy(int.Parse(userId)))
            .AndThenConsume(profile => {
                var profileState = _repository.ProfileOf(userId);
                Completes?.With(Response.Of(
                    ResponseStatus.Ok,
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
  
                Stage?.ActorFor<IProfile>(() => new ProfileActor(profileState));
  
                _repository.Save(profileState);
                Completes?.With(Response.Of(ResponseStatus.Created, JsonSerialization.Serialized(ProfileData.From(profileState))));
            });
    }
        
    public void Query(string userId)
    {
        var profileState = _repository.ProfileOf(userId);
        if (profileState.DoesNotExist)
        {
            Completes?.With(Response.Of(ResponseStatus.NotFound, ProfileLocation(userId)));
        }
        else
        {
            Completes?.With(Response.Of(ResponseStatus.Ok, JsonSerialization.Serialized(ProfileData.From(profileState))));
        }
    }

    private string ProfileLocation(string userId) => $"/users/{userId}/profile";
}
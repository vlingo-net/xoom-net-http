using System;
using System.Collections.Generic;
using Vlingo.Http;
using Vlingo.Http.Resource;
using Vlingo.Common;
namespace Vlingo.Http.Tests.Sample.User
{
public class UserResourceDispatcher : ConfigurationResource<Vlingo.Http.Tests.Sample.User.UserResource>
{
  public UserResourceDispatcher(string name, Type resourceHandlerClass, int handlerPoolSize, IList<Vlingo.Http.Resource.Action> actions) : base(name, resourceHandlerClass, handlerPoolSize, actions)
  {
  }
  public override void DispatchToHandlerWith(Context context, Vlingo.Http.Resource.Action.MappedParameters mappedParameters) {
    Action<Vlingo.Http.Tests.Sample.User.UserResource> consumer = null;

    try {
      switch (mappedParameters.ActionId) {
      case 0: // POST /users Register(body:Vlingo.Http.Tests.Sample.User.UserData userData)
        consumer = handler => handler.Register((Vlingo.Http.Tests.Sample.User.UserData) mappedParameters.Mapped[0].Value);
        PooledHandler.HandleFor(context, consumer);
        break;
      case 1: // PATCH /users/{userId}/contact changeContact(string userId, body:Vlingo.Http.Tests.Sample.User.ContactData contactData)
        consumer = handler => handler.ChangeContact((string) mappedParameters.Mapped[0].Value, (Vlingo.Http.Tests.Sample.User.ContactData) mappedParameters.Mapped[1].Value);
        PooledHandler.HandleFor(context, consumer);
        break;
      case 2: // PATCH /users/{userId}/name changeName(string userId, body:Vlingo.Http.Tests.Sample.User.NameData nameData)
        consumer = handler => handler.ChangeName((string) mappedParameters.Mapped[0].Value, (Vlingo.Http.Tests.Sample.User.NameData) mappedParameters.Mapped[1].Value);
        PooledHandler.HandleFor(context, consumer);
        break;
      case 3: // GET /users/{userId} queryUser(string userId)
        consumer = handler => handler.QueryUser((string) mappedParameters.Mapped[0].Value);
        PooledHandler.HandleFor(context, consumer);
        break;
      case 4: // GET /users queryUsers()
        consumer = handler => handler.QueryUsers();
        PooledHandler.HandleFor(context, consumer);
        break;
      case 5: // GET /users/{userId}/error queryUserError(string userId)
        consumer = handler => handler.QueryUserError((string) mappedParameters.Mapped[0].Value);
        PooledHandler.HandleFor(context, consumer);
        break;
      }
    } catch (Exception e) {
      throw new ArgumentException("Action mismatch: Request: " + context.Request + "Parameters: " + mappedParameters, e);
    }
  }

}
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vlingo.Actors;
using Vlingo.Common;

namespace Vlingo.Http.Resource
{
    public class ResourceRequestHandler__Proxy : Vlingo.Http.Resource.IResourceRequestHandler
    {
        private const string HandleForRepresentation1 = "HandleFor<T>(Vlingo.Http.Context, Action<T>)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public ResourceRequestHandler__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void HandleFor<T>(Vlingo.Http.Context context, Action<T>? consumer)
            where T : Vlingo.Http.Resource.ResourceHandler
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Http.Resource.IResourceRequestHandler> cons128873 = __ =>
                    __.HandleFor<T>(context, consumer);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons128873, null, HandleForRepresentation1);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Http.Resource.IResourceRequestHandler>(this.actor, cons128873,
                            HandleForRepresentation1));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, HandleForRepresentation1));
            }
        }
    }
}
// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Actors;

namespace Vlingo.Http.Resource
{
    public class DynamicResource : Resource
    {
        internal IList<RequestHandler> Handlers { get; }
        public IList<Action>? Actions { get; }

        public DynamicResource(string name, int handlerPoolSize, IList<RequestHandler> unsortedHandlers)
            : base(name, handlerPoolSize)
        {
            Handlers = SortHandlersBySlashes(unsortedHandlers);
            var currentId = 0;
            foreach (var predicate in Handlers)
            {
                Actions?.Add(new Action(
                    currentId++,
                    predicate.Method.ToString(),
                    predicate.Path,
                    "Dynamic" + currentId + "(" + predicate.ActionSignature + ")",
                    null));
            }
        }

        public override void DispatchToHandlerWith(Context context, Action.MappedParameters? mappedParameters)
        {
            try
            {
                Action<ResourceHandler> consumer = resource =>
                    Handlers[mappedParameters!.ActionId]
                    .Execute(context.Request!, mappedParameters, resource.Logger!)
                    .AndThenConsume(context.Completes.With);

                PooledHandler.HandleFor(context, consumer);
            }
            catch
            {
                throw new ArgumentException($"Action mismatch: Request: {context.Request} Parameters: {mappedParameters}");
            }
        }

        public override void Log(ILogger logger)
        {
            logger.Info($"Resource: {Name}");

            if (Actions != null)
            {
                foreach (var action in Actions)
                {
                    logger.Info(
                        $"Action: id={action.Id}, method={action.Method}, uri={action.Uri}, to={action.To.Signature}");
                }
            }
        }

        public override Action.MatchResults MatchWith(Method? method, Uri? uri)
        {
            if (Actions != null)
            {
                foreach (var action in Actions)
                {
                    var matchResults = action.MatchWith(method, uri);
                    if (matchResults.IsMatched)
                    {
                        return matchResults;
                    }
                }    
            }
            
            return Action.UnmatchedResults;
        }

        public override ResourceHandler ResourceHandlerInstance(Stage stage)
            => new SpecificResourceHandler(stage);

        private IList<RequestHandler> SortHandlersBySlashes(IList<RequestHandler> unsortedHandlers)
            => unsortedHandlers.OrderBy(x => x.Path.LongCount(c => c == '/')).ToList();


        private sealed class SpecificResourceHandler : ResourceHandler
        {
            public SpecificResourceHandler(Stage stage)
            {
                Stage = stage;
            }
        }
    }
}

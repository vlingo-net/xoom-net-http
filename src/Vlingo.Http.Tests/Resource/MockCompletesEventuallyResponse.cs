// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors;
using Vlingo.Actors.TestKit;

namespace Vlingo.Http.Tests.Resource
{
    public class MockCompletesEventuallyResponse : ICompletesEventually
    {
        private AccessSafely _withCalls = AccessSafely.AfterCompleting(0);

        public Response Response { get; private set; }
        
        /// <summary>
        /// Answer with an AccessSafely which writes nulls to "with" and reads the write count from the "completed".
        /// </summary>
        /// <param name="n">Number of times With(outcome) must be called before ReadFrom(...) will return.</param>
        /// <returns>AccessSafely instance</returns>
        /// <remarks>Note: Clients can replace the default lambdas with their own via readingWith/writingWith.</remarks>
        public AccessSafely ExpectWithTimes(int n)
        {
            _withCalls = AccessSafely.AfterCompleting(n)
                .WritingWith<object>("with", x => {})
                .ReadingWith("completed", () => _withCalls.TotalWrites);
            return _withCalls;
        }
        public void Conclude()
        {
        }

        public void Stop()
        {
        }

        public bool IsStopped => false;
        public void With(object outcome)
        {
            Response = (Response) outcome;
            _withCalls.WriteUsing<object>("with", null);
        }

        public IAddress Address => null;
    }
}
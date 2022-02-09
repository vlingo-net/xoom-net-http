// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Http.Resource
{
    /// <summary>
    /// A pool of <code>IDispatcher</code> instances.
    /// </summary>
    public interface IDispatcherPool
    {
        /// <summary>
        /// Close the <code>IDispatcher</code> instances of my internal pool.
        /// </summary>
        void Close();

        /// <summary>
        /// Answer an available <code>IDispatcher</code> from my pool.
        /// </summary>
        /// <returns><see cref="IDispatcher"/></returns>
        IDispatcher Dispatcher();
    }
}
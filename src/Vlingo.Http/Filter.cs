// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Http
{
    /// <summary>
    /// Base filter supporting starting and stopping.
    /// </summary>
    public abstract class Filter
    {
        /// <summary>
        /// Sent when I am to be stopped.
        /// </summary>
        public abstract void Stop();
    }
}
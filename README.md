# vlingo-http

[![Build status](https://ci.appveyor.com/api/projects/status/1c2u6kbrpbvfjxgf/branch/master?svg=true)](https://ci.appveyor.com/project/VlingoNetOwner/vlingo-net-http/branch/master) 
[![NuGet](https://img.shields.io/nuget/v/Vlingo.Http.svg)](https://www.nuget.org/packages/Vlingo.Http)
[![Gitter](https://badges.gitter.im/vlingo-platform-net/community.svg)](https://gitter.im/vlingo-platform-net/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

### Usage

Reactive, scalable, and resilient HTTP servers and RESTful services running on vlingo-net/cluster and vlingo-net/actors.

1. The essential features are completed
  * Fully actor-based asynchronous requests and responses.
  * The request handling is resource based.
  * Requests that require message body content are auto-mapped to simple Java objects.

2. To run the Server:
  * [Use Server#StartWith() to start the Server actor](https://github.com/vlingo/vlingo-net-http/blob/master/src/Vlingo.Http/Resource/Server.cs)
  * The light-weight Server is meant to be run inside vlingo/cluster nodes the require RESTful HTTP support.
  
3. See the following for usage examples:
  * [vlingo/http properties file](https://github.com/vlingo/vlingo-net-http/blob/master/src/Vlingo.Http.Tests/Resources/vlingo-http.properties)
  * [The user resource sample](#) (Sample link to be provided)
  * [The user profile resource sample](#) (Sample link to be provided)


License (See LICENSE file for full license)
-------------------------------------------
Copyright © 2012-2019 VLINGO LABS. All rights reserved.

This Source Code Form is subject to the terms of the
Mozilla Public License, v. 2.0. If a copy of the MPL
was not distributed with this file, You can obtain
one at https://mozilla.org/MPL/2.0/.

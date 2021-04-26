// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Http.Tests.Sample.User.Model;

namespace Vlingo.Xoom.Http.Tests.Sample.User
{
    public class UserData
    {
        public string Id { get; }
        public NameData NameData { get; }
        public ContactData ContactData { get; }
  
        public static UserData From(NameData nameData, ContactData contactData) => new UserData(nameData, contactData);
  
        public static UserData From(string id, NameData nameData, ContactData contactData) => new UserData(id, nameData, contactData);
  
        public static UserData From(UserState userState) =>
            new UserData(userState.Id, NameData.From(userState.Name), ContactData.From(userState.Contact));

        public static UserData UserAt(string location, List<UserData> userData)
        {
            var index = location.LastIndexOf("/", StringComparison.Ordinal);
            var id = location.Substring(index + 1);
            return UserOf(id, userData);
        }

        public static UserData UserOf(string id, List<UserData> userData)
        {
            foreach (var data in userData)
            {
                if (data.Id.Equals(id))
                {
                    return data;
                }   
            }
            
            return null;
        }

        private UserData(NameData nameData, ContactData contactData)
        {
            Id = Guid.NewGuid().ToString();
            NameData = nameData;
            ContactData = contactData;
        }
  
        public UserData(string id, NameData nameData, ContactData contactData)
        {
            Id = id;
            NameData = nameData;
            ContactData = contactData;
        }

        public override string ToString() => $"UserData[id={Id}, nameData={NameData}, contactData={ContactData}]";
    }
}
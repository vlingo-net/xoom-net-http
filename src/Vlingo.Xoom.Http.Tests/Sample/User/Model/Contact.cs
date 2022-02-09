// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Http.Tests.Sample.User.Model
{
    public class Contact
    {
        public string EmailAddress { get; }
        public string TelephoneNumber { get; }
  
        public static Contact From(string emailAddress, string telephoneNumber) => new Contact(emailAddress, telephoneNumber);

        public Contact(string emailAddress, string telephoneNumber)
        {
            EmailAddress = emailAddress;
            TelephoneNumber = telephoneNumber;
        }

        public override string ToString() => $"Contact[emailAddress={EmailAddress}, telephoneNumber={TelephoneNumber}]";
    }
}
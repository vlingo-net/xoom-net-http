// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Http.Tests.Sample.User.Model;

namespace Vlingo.Xoom.Http.Tests.Sample.User;

public class ContactData
{
    public string EmailAddress { get; }
    public string TelephoneNumber { get; }

    public static ContactData From(string emailAddress, string telephoneNumber) => new ContactData(emailAddress, telephoneNumber);

    public static ContactData From(Contact contact) => new ContactData(contact.EmailAddress, contact.TelephoneNumber);
  
    public ContactData(string emailAddress, string telephoneNumber)
    {
        EmailAddress = emailAddress;
        TelephoneNumber = telephoneNumber;
    }

    public override string ToString() => $"ContactData[emailAddress={EmailAddress}, telephoneNumber={TelephoneNumber}]";

    public override bool Equals(object obj)
    {
        if (this == obj) return true;
        if (!(obj is ContactData)) return false;
        var contactData = (ContactData) obj;
        return EmailAddress.Equals(contactData.EmailAddress) && TelephoneNumber.Equals(contactData.TelephoneNumber);
    }

    public override int GetHashCode() => 13 * EmailAddress.GetHashCode() + TelephoneNumber.GetHashCode();
}
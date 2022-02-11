// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Http.Media;
using Xunit;

namespace Vlingo.Xoom.Http.Tests.Media;

public class AcceptMediaTypeTest
{
    [Fact]
    public void SpecificMimeTypeGreaterThanGeneric()
    {
        var acceptMediaType1 = new ResponseMediaTypeSelector.AcceptMediaType("application", "json");
        var acceptMediaType2 = new ResponseMediaTypeSelector.AcceptMediaType("*", "*");
        Assert.Equal( 1, acceptMediaType1.CompareTo(acceptMediaType2));
        Assert.Equal( -1, acceptMediaType2.CompareTo(acceptMediaType1));
    }
        
    [Fact]
    public void SpecificMimeSubTypeGreaterThanGeneric()
    {
        var acceptMediaType1 = new ResponseMediaTypeSelector.AcceptMediaType("application", "json");
        var acceptMediaType2 = new ResponseMediaTypeSelector.AcceptMediaType("application", "*");
        Assert.Equal( 1, acceptMediaType1.CompareTo(acceptMediaType2));
        Assert.Equal( -1, acceptMediaType2.CompareTo(acceptMediaType1));
    }
        
    [Fact]
    public void SpecificParameterGreaterThanGenericWithSameQualityFactor()
    {
        var acceptMediaType1 = new MediaTypeDescriptor.Builder<ResponseMediaTypeSelector.AcceptMediaType>(
                (a, b, c) => new ResponseMediaTypeSelector.AcceptMediaType(a, b, c))
            .WithMimeType("application")
            .WithMimeSubType("xml")
            .WithParameter("version", "1.0")
            .Build();

        var acceptMediaType2 = new ResponseMediaTypeSelector.AcceptMediaType("application", "json");
        Assert.Equal( 1, acceptMediaType1.CompareTo(acceptMediaType2));
        Assert.Equal( -1, acceptMediaType2.CompareTo(acceptMediaType1));
    }
        
    [Fact]
    public void QualityFactorTrumpsSpecificity()
    {
        var acceptMediaType1 = new MediaTypeDescriptor.Builder<ResponseMediaTypeSelector.AcceptMediaType>(
                (a, b, c) => new ResponseMediaTypeSelector.AcceptMediaType(a, b, c))
            .WithMimeType("text")
            .WithMimeSubType("*")
            .Build();

        var acceptMediaType2 = new MediaTypeDescriptor.Builder<ResponseMediaTypeSelector.AcceptMediaType>(
                (a, b, c) => new ResponseMediaTypeSelector.AcceptMediaType(a, b, c))
            .WithMimeType("text")
            .WithMimeSubType("json")
            .WithParameter("q", "0.8")
            .Build();

        Assert.Equal( 1, acceptMediaType1.CompareTo(acceptMediaType2));
        Assert.Equal( -1, acceptMediaType2.CompareTo(acceptMediaType1));
    }
}
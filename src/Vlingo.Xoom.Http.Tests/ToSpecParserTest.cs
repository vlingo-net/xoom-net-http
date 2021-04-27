// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;

namespace Vlingo.Xoom.Http.Tests
{
    using Vlingo.Xoom.Http.Resource;
    
    public class ToSpecParserTest
    {
        [Fact]
        public void TestSimpleUnqualifiedNonBodyParameterMapping()
        {
            new Action(0, "PATCH", "/airports/{airportId}/stringProperty", "changeStringProperty(string value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/integerProperty", "changeIntegerProperty(int value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/longProperty", "changeLongProperty(long value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/booleanProperty", "changeBooleanProperty(bool value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/booleanProperty", "changeBooleanProperty(long value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/doubleProperty", "changeDoubleProperty(double value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/shortProperty", "changeShortProperty(short value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/floatProperty", "changeFloatProperty(float value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/decimalProperty", "changeDecimalProperty(decimal value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/characterProperty", "changeCharacterProperty(char value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/byteProperty", "changeByteProperty(byte value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/dateTimeOffsetProperty", "changeDateTimeOffsetProperty(DateTimeOffset value)", null);
        }

        [Fact]
        public void TestComplexUnqualifiedNonBodyParameterMapping()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new Action(0, "PATCH", "/airports/{airportId}/geocode", "name(body:Geocode geocode)", null));
        }

        [Fact]
        public void TestSimpleQualifiedBodyParameterMapping()
        {
            new Action(0, "PATCH", "/airports/{airportId}/stringProperty",
                "changeStringProperty(body:System.String value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/integerProperty",
                "changeIntegerProperty(body:System.Int32 value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/decimalProperty",
                "changeDecimalProperty(body:System.Decimal value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/doubleProperty",
                "changeDoubleProperty(body:System.Double value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/shortProperty",
                "changeShortProperty(body:System.Int16 value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/dateTimeOffsetProperty",
                "changeFloatProperty(body:System.DateTimeOffset value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/characterProperty",
                "changeCharacterProperty(body:System.Char value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/byteProperty",
                "changeByteProperty(body:System.Byte value)", null);
            new Action(0, "PATCH", "/airports/{airportId}/boolProperty",
                "changeByteProperty(body:System.Boolean value)", null);
        }

        [Fact]
        public void TestComplexQualifiedBodyParameterMapping()
        {
            new Action(0, "PATCH", "/airports/{airportId}/geocode",
                "name(body:Vlingo.Xoom.Http.Tests.ToSpecParserTest+Geocode geocode)", null);
        }

        public class Geocode
        {
            public double latitude;
            public double longitude;

            Geocode(double lat, double lon)
            {
                latitude = lat;
                longitude = lon;
            }
        }
    }
}
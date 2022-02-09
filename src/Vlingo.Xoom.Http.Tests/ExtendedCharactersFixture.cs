// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Text;

namespace Vlingo.Xoom.Http.Tests
{
    public class ExtendedCharactersFixture
    {
        public static string AsciiWithExtendedCharacters()
        {
            var builder = new StringBuilder();

            var asciiBegin = 0x0020;
            var asciiEnd = 0x007E;

            for (var ascii = asciiBegin; ascii <= asciiEnd; ++ascii)
            {
                builder.Append((char) ascii);
            }

            var cyrillicBegin = 0x0409;
            var cyrillicEnd = 0x04FF;

            for (var cyrillic = cyrillicBegin; cyrillic <= cyrillicEnd; ++cyrillic)
            {
                builder.Append((char) cyrillic);
            }

            var greekCopticBegin = 0x0370;
            var greekCopticEnd = 0x03FF;

            for (var greekCoptic = greekCopticBegin; greekCoptic <= greekCopticEnd; ++greekCoptic)
            {
                builder.Append((char) greekCoptic);
            }

            return builder.ToString();
        }
    }
}
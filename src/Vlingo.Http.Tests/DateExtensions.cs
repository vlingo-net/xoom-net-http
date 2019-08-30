using System;

namespace Vlingo.Http.Tests
{
    public static class DateExtensions
    {
        public static double GetCurrentMillis()
        {
            var jan1970 = new DateTime(1970, 1, 1, 0, 0,0, DateTimeKind.Utc);
            var javaSpan = DateTime.UtcNow - jan1970;
            return javaSpan.TotalMilliseconds;
        }
    }
}
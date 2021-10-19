using System;

namespace ProjectLighthouse.Helpers {
    public static class TimestampHelper {
        public static long Timestamp => (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
}
using System;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class TimeHelper
{
    public static long Timestamp => (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

    public static long TimestampMillis => (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
}

// 1397109686193
// 1635389749454
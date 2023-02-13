using System;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class TimeHelper
{
    public static long Timestamp => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static long TimestampMillis => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}

// 1397109686193
// 1635389749454
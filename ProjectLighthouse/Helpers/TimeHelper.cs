using System;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class TimeHelper
{
    public static long Timestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public static long TimestampMillis => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}

// 1397109686193
// 1635389749454
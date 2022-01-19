using System;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class TimeHelper
{
    public static long UnixTimeMilliseconds() => DateTimeOffset.Now.ToUnixTimeMilliseconds();
    public static long UnixTimeSeconds() => DateTimeOffset.Now.ToUnixTimeSeconds();
}

// 1397109686193
// 1635389749454
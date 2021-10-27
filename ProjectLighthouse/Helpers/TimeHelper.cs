using System;

namespace LBPUnion.ProjectLighthouse.Helpers {
    public static class TimeHelper {
        public static long UnixTimeMilliseconds() => DateTimeOffset.Now.ToUnixTimeMilliseconds();
        public static long UnixTimeSeconds() => DateTimeOffset.Now.ToUnixTimeSeconds();
    }
}
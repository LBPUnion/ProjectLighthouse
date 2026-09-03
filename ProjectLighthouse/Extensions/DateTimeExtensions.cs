using System;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTimeMilliseconds(this DateTime dateTime) =>
        ((DateTimeOffset)DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)).ToUnixTimeMilliseconds();

    public static DateTime FromUnixTimeMilliseconds(long timestamp) =>
        DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
}
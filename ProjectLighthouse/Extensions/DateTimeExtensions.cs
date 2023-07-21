using System;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTimeMilliseconds(this DateTime dateTime) =>
        new DateTimeOffset(dateTime).ToUniversalTime().ToUnixTimeMilliseconds();

    public static DateTime FromUnixTimeMilliseconds(long timestamp) =>
        DateTimeOffset.FromUnixTimeMilliseconds(timestamp).ToUniversalTime().DateTime;
}
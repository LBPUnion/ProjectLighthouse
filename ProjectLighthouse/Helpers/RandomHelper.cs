using System;
using System.Collections.Generic;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class RandomHelper
{
    /// <summary>
    /// An instance of Random. Must be locked when in use.
    /// </summary>
    public static readonly Random Random = new();

    /// <summary>
    ///     Generates a specified amount of random bytes in an array.
    /// </summary>
    /// <param name="count">The amount of bytes to generate.</param>
    /// <returns>The bytes generated</returns>
    public static IEnumerable<byte> GenerateRandomBytes(int count)
    {
        byte[] b = new byte[count];

        lock(Random)
        {
            Random.NextBytes(b);
        }

        return b;
    }
}
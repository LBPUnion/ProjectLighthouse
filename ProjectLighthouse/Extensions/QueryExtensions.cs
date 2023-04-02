using System;
using System.Collections.Generic;
using System.Linq;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class QueryExtensions
{
    public static List<T2> ToSerializableList<T, T2>(this IEnumerable<T> enumerable, Func<T, T2> selector)
        => enumerable.Select(selector).ToList();
}
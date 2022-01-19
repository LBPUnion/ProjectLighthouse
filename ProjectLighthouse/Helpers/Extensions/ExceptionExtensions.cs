using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LBPUnion.ProjectLighthouse.Helpers.Extensions;

// https://stackoverflow.com/a/8039737
public static class ExceptionExtensions
{
    public static string ToDetailedException(this Exception exception)
    {
        PropertyInfo[] properties = exception.GetType().GetProperties();

        IEnumerable<string> fields = properties.Select
            (
                property => new
                {
                    property.Name,
                    Value = property.GetValue(exception, null),
                }
            )
            .Select(x => $"{x.Name} = {(x.Value != null ? x.Value.ToString() : string.Empty)}");

        return string.Join("\n", fields);
    }
}
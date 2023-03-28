#nullable enable
using System;
using System.Reflection;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class ReflectionHelper
{

    /// <summary>
    /// Use reflection to copy the values of all fields from one object to another
    /// Fields are copied based on name 
    /// </summary>
    /// <param name="sourceObj">The source object to copy from</param>
    /// <param name="destObj">The destination object to copy all fields to</param>
    public static void CopyAllFields(object sourceObj, object destObj)
    {
        Type sourceObjectType = sourceObj.GetType();
        Type destObjectType = destObj.GetType();
        foreach (PropertyInfo sourceProperty in sourceObjectType.GetProperties())
        {
            PropertyInfo? destProperty = destObjectType.GetProperty(sourceProperty.Name);
            if (destProperty == null) continue;

            if (sourceProperty.Name != destProperty.Name) continue;
            if (sourceProperty.PropertyType != destProperty.PropertyType) continue;

            object? sourcePropertyValue = sourceProperty.GetValue(sourceObj);
            if (sourcePropertyValue == null) continue;

            destProperty.SetValue(destObj, sourcePropertyValue);
        }
    }
}
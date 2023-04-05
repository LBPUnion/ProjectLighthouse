#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace LBPUnion.ProjectLighthouse.Serialization;

public static class LighthouseSerializer
{
    private static readonly ConcurrentDictionary<(Type, XmlRootAttribute?), CustomXmlSerializer> serializerCache = new();

    private static readonly XmlSerializerNamespaces emptyNamespace = new(new[]
    {
        XmlQualifiedName.Empty,
    });

    private static readonly XmlWriterSettings defaultWriterSettings = new()
    {
        OmitXmlDeclaration = true,
        CheckCharacters = false,
    };

    public static CustomXmlSerializer GetSerializer(Type type, XmlRootAttribute? rootAttribute = null)
    {
        if (serializerCache.TryGetValue((type, rootAttribute), out CustomXmlSerializer? value)) return value;

        CustomXmlSerializer serializer = new(type, rootAttribute);

        serializerCache.TryAdd((type, rootAttribute), serializer);
        return serializer;
    }

    public static string Serialize(IServiceProvider serviceProvider, ILbpSerializable? serializableObject)
    {
        if (serializableObject == null) return "";

        XmlRootAttribute? rootAttribute = null;

        if (serializableObject is IHasCustomRoot customRoot) rootAttribute = new XmlRootAttribute(customRoot.GetRoot());

        CustomXmlSerializer serializer = GetSerializer(serializableObject.GetType(), rootAttribute);

        using StringWriter stringWriter = new();

        WriteFullClosingTagXmlWriter xmlWriter = new(stringWriter, defaultWriterSettings);
            
        serializer.Serialize(serviceProvider, xmlWriter, serializableObject, emptyNamespace);
        string finalResult = stringWriter.ToString();
        stringWriter.Dispose();
        return finalResult;
    }

    public static void PrepareForSerialization(IServiceProvider serviceProvider, INeedsPreparationForSerialization serializableObject)
    {
        MethodInfo? methodInfo = serializableObject.GetType().GetMethod("PrepareSerialization");
        if (methodInfo == null) return;
        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
        List<object> parameters = new();
        foreach (ParameterInfo info in parameterInfos)
        {
            try
            {
                parameters.Add(serviceProvider.GetRequiredService(info.ParameterType));
            }
            catch (Exception e)
            {
                Logger.Error(
                    $"Failed to resolve dependency '{info.Name}' during serialization of {serializableObject.GetType().FullName}: {e.ToDetailedException()}",
                    LogArea.Serialization);
                return;
            }
        }

        if (parameterInfos.Length != parameters.Count)
        {
            Logger.Error(
                $"Failed to fetch one or more dependencies during serialization of {serializableObject.GetType().FullName}",
                LogArea.Serialization);
            return;
        }

        try
        {
            Task? methodTask = (Task?)methodInfo.Invoke(serializableObject, parameters.ToArray());
            // Using await here causes the IServiceProvider to be disposed so it must be called with .Wait() instead
            methodTask?.Wait();
        }
        catch (Exception e)
        {
            Logger.Error(
                @$"Failed to prepare '{serializableObject.GetType().FullName}' for serialization: {e.ToDetailedException()}",
                LogArea.Serialization);
        }
    }
}
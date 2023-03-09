using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Serialization;

/// <summary>
/// This class is used in lieu of the regular XmlSerializer so we can add a custom function trigger 
/// such that before a class is about to be serialized a custom function will be called for that object.
/// </summary>
public class CustomXmlSerializer : XmlSerializer
{
    private readonly IServiceProvider provider;

    public CustomXmlSerializer(Type type, IServiceProvider provider) : base(type)
    {
        this.provider = provider;
    }

    public CustomXmlSerializer(Type type, IServiceProvider provider, XmlRootAttribute rootAttribute) : base(type, rootAttribute)
    {
        this.provider = provider;
    }

    public new void Serialize(object o, XmlSerializationWriter xmlSerializationWriter)
    {
        this.TriggerCallback(o);
        base.Serialize(o, xmlSerializationWriter);
    }

    public new void Serialize(Stream stream, object o)
    {
        this.TriggerCallback(o);
        base.Serialize(stream, o);
    }

    public new void Serialize(TextWriter textWriter, object o)
    {
        this.TriggerCallback(o);
        base.Serialize(textWriter, o);
    }

    public new void Serialize(XmlWriter xmlWriter, object o)
    {
        this.TriggerCallback(o);
        base.Serialize(xmlWriter, o);
    }

    public new void Serialize(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces)
    {
        this.TriggerCallback(o);
        base.Serialize(xmlWriter, o, namespaces);
    }

    /// <summary>
    /// Recursively finds all properties of an object
    /// </summary>
    /// <param name="obj">The object to recursively find all properties of</param>
    /// <param name="alreadyChecked">A list of type references that have already been checked to prevent infinite loops</param>
    /// <returns>A list of object references of all properties of the object</returns>
    public void RecursivelyPrepare(object obj, List<INeedsPreparationForSerialization> alreadyChecked)
    {
        List<INeedsPreparationForSerialization> list = new();
        switch (obj)
        {
            case null: return;
            case INeedsPreparationForSerialization cb when alreadyChecked.Contains(cb): return;
            case INeedsPreparationForSerialization cb:
            {
                this.PrepareForSerialization(cb);
                list.Add(cb);
                break;
            }
        }

        foreach (PropertyInfo info in obj.GetType().GetProperties())
        {
            if (info.PropertyType.IsPrimitive) continue;


            // If the property isn't a list or a ILbpSerializable
            if (typeof(IList).IsAssignableFrom(info.PropertyType) && info.PropertyType.GetGenericArguments().Length > 0)
            {
                if (!typeof(ILbpSerializable).IsAssignableFrom(info.PropertyType.GetGenericArguments()[0])) continue;
            }
            else
            {
                if (!typeof(INeedsPreparationForSerialization).IsAssignableFrom(info.PropertyType)) continue;
            }

            object val = info.GetValue(obj, null);
            switch (val)
            {
                // Recursively find items in list
                case IList elems:
                {
                    foreach (object item in elems)
                    {
                        this.RecursivelyPrepare(item, list);
                    }
                    break;
                }
                // Otherwise we already checked that it is serializable so add it to the list  
                case INeedsPreparationForSerialization prep:
                    if (list.Contains(prep)) continue;

                    this.PrepareForSerialization(prep);
                    list.Add(prep);
                    break;
            }
        }
    }

    public void PrepareForSerialization(INeedsPreparationForSerialization obj) 
        => LighthouseSerializer.PrepareForSerialization(this.provider, obj);

    public void TriggerCallback(object o)
    {
        this.RecursivelyPrepare(o, new List<INeedsPreparationForSerialization>());
    }
}
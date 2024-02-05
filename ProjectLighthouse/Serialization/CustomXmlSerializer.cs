using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Serialization;

namespace LBPUnion.ProjectLighthouse.Serialization;

/// <summary>
/// This class is used in lieu of the regular XmlSerializer so we can add a custom function trigger 
/// such that before a class is about to be serialized a custom function will be called for that object.
/// </summary>
public class CustomXmlSerializer : XmlSerializer
{
    public CustomXmlSerializer(Type type, XmlRootAttribute rootAttribute) : base(type, rootAttribute)
    { }

    public void Serialize(IServiceProvider provider, XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces)
    {
        this.TriggerCallback(provider, o);
        base.Serialize(xmlWriter, o, namespaces);
    }

    private static readonly ConcurrentDictionary<PropertyInfo, Func<object, object>> propertyCache = new ();

    // https://stackoverflow.com/a/35759009
    // TLDR: Reflection is pretty slow, so we compile an expression tree to fetch the field we want then cache it
    // so we incur less expense next time we try and access this property.
    private static object GetFromCache(object owner, PropertyInfo propertyInfo)
    {
        if (propertyCache.TryGetValue(propertyInfo, out Func<object,object> getter))
        {
            return getter.Invoke(owner);
        }

        if (!propertyInfo.CanRead) throw new Exception("Tried to get from object that has no getter");

        ParameterExpression param = Expression.Parameter(typeof(object), "param");
        Expression propExpression =
            Expression.Convert(Expression.Property(Expression.Convert(param, owner.GetType()), propertyInfo),
                typeof(object));
        LambdaExpression lambda = Expression.Lambda(propExpression,
            param);
        if (lambda.Compile() is not Func<object, object> compiled)
            throw new Exception("Failed to compile lambda getter expression");
        
        propertyCache.TryAdd(propertyInfo, compiled);
        return compiled.Invoke(owner);

    }

    /// <summary>
    /// Recursively finds all properties of an object
    /// </summary>
    /// <param name="provider">The service provider from the ASP.NET request that is used to resolve dependencies</param>
    /// <param name="obj">The object to recursively find all properties of</param>
    /// <param name="alreadyPrepared">A list of type references that have already been prepared to prevent duplicate preparing</param>
    /// <param name="recursionDepth">A number tracking how deep into the recursion call stack we are to prevent recursive loops</param>
    /// <returns>A list of object references of all properties of the object</returns>
    private void RecursivelyPrepare(IServiceProvider provider, object obj, ICollection<INeedsPreparationForSerialization> alreadyPrepared, int recursionDepth = 0)
    {
        if (recursionDepth > 5) return;
        switch (obj)
        {
            case INeedsPreparationForSerialization needsPreparation:
                if (alreadyPrepared.Contains(needsPreparation)) break;

                PrepareForSerialization(provider, needsPreparation);
                alreadyPrepared.Add(needsPreparation);
                break;
            case null: return;
        }

        foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
        {
            if (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof(string)) continue;

            // If the property is a list
            if (typeof(IList).IsAssignableFrom(propertyInfo.PropertyType))
            {
                // If the list doesn't contain objects of ILbpSerializable, skip
                if (!typeof(ILbpSerializable).IsAssignableFrom(propertyInfo.PropertyType.GetGenericArguments()
                        .ElementAtOrDefault(0)))
                {
                    continue;
                }
                
            }
            // Otherwise if the object isn't a ILbpSerializable or a nullable ILbpSerializable, skip
            else if (!typeof(ILbpSerializable).IsAssignableFrom(propertyInfo.PropertyType) &&
                     !typeof(ILbpSerializable).IsAssignableFrom(Nullable.GetUnderlyingType(propertyInfo.PropertyType)))
            {
                continue;
            }

            object val = GetFromCache(obj, propertyInfo);
            switch (val)
            {
                case IList list:
                    foreach (object o in list)
                    {
                        this.RecursivelyPrepare(provider, o, alreadyPrepared, recursionDepth+1);
                    }
                    break;
                case INeedsPreparationForSerialization nP:
                    if (alreadyPrepared.Contains(nP)) break;

                    // Prepare object
                    PrepareForSerialization(provider, nP);
                    alreadyPrepared.Add(nP);

                    // Recursively find objects in this INeedsPreparationForSerialization object
                    this.RecursivelyPrepare(provider, nP, alreadyPrepared, recursionDepth+1);
                    break;
                case ILbpSerializable serializable:
                    // Recursively find objects in this ILbpSerializable object
                    this.RecursivelyPrepare(provider, serializable, alreadyPrepared, recursionDepth+1);
                    break;
            }
        }
    }

    private static void PrepareForSerialization(IServiceProvider provider, INeedsPreparationForSerialization obj) 
        => LighthouseSerializer.PrepareForSerialization(provider, obj);

    private void TriggerCallback(IServiceProvider provider, object o)
    {
        this.RecursivelyPrepare(provider, o, new List<INeedsPreparationForSerialization>());
    }
}
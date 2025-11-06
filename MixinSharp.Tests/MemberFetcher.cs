using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MixinUtils.Tests;

/// <summary>
/// Provides utility methods to fetch and analyze members (methods, properties, and fields)
/// of a specified type.
/// </summary>
public static class MemberFetcher
{
    /// <summary>
    /// Retrieves all method signatures for the given type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object whose methods are to be fetched.</typeparam>
    /// <returns>A set of strings representing the full method signatures of all instance methods (including public, private, and protected methods) of the specified type.</returns>
    public static HashSet<string> GetMethods<T>()
    {
        HashSet<string> results = new HashSet<string>();
        
        Type t = typeof(T);

        StringBuilder builder = new StringBuilder();
        foreach (var methodInfo in t.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            builder.Clear();
            var returnType = methodInfo.ReturnType.FullName;
            builder.Append(returnType);
            builder.Append(" ");
            builder.Append(methodInfo.Name);
            builder.Append("(");
            var parameters = methodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                builder.Append(parameter.ParameterType.FullName);
                if(i != parameters.Length - 1)
                    builder.Append(", ");
            }
            builder.Append(")");
            results.Add(builder.ToString());
        }
        return results;
    }

    /// <summary>
    /// Retrieves the properties of the specified type <typeparamref name="T"/>.
    /// The information includes the type and name of each property, including public
    /// and non-public instance properties.
    /// </summary>
    /// <typeparam name="T">The type whose properties are to be retrieved.</typeparam>
    /// <returns>A set of strings representing the type and name of each property for the specified type <typeparamref name="T"/>.</returns>
    public static HashSet<string> GetProperties<T>()
    {
        HashSet<string> results = new HashSet<string>();
        
        Type t = typeof(T);

        StringBuilder builder = new StringBuilder();
        
        foreach (var propertyInfo in t.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            builder.Clear();
            var returnType = propertyInfo.PropertyType.FullName;
            builder.Append(returnType);
            builder.Append(" ");
            builder.Append(propertyInfo.Name);
            
            results.Add(builder.ToString());
        }
        return results;
    }

    /// Retrieves a set of field signatures for the specified type.
    /// This method uses reflection to extract information about the fields
    /// defined in the specified type. Both public and non-public instance fields
    /// are included. The format of each field signature in the resulting set
    /// includes its type and name.
    /// <typeparam name="T">
    /// The type from which the field information is to be retrieved.
    /// </typeparam>
    /// <returns>
    /// A HashSet containing the signatures of the fields in the specified type.
    /// Each signature includes the full name of the field type and the field name.
    /// </returns>
    public static HashSet<string> GetFields<T>()
    {
        HashSet<string> results = new HashSet<string>();
        
        Type t = typeof(T);

        StringBuilder builder = new StringBuilder();
        
        foreach (var fieldInfo in t.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            builder.Clear();
            var returnType = fieldInfo.FieldType.FullName;
            builder.Append(returnType);
            builder.Append(" ");
            builder.Append(fieldInfo.Name);
            
            results.Add(builder.ToString());
        }
        return results;
    }
}
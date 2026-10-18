using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Meta.WitAi.Utilities;

public class ReflectionUtils
{
	private static Dictionary<string, FieldInfo> _cachedFields = new Dictionary<string, FieldInfo>();

	private static Dictionary<string, PropertyInfo> _cachedProperties = new Dictionary<string, PropertyInfo>();

	private static Dictionary<string, MethodInfo> _cachedMethods = new Dictionary<string, MethodInfo>();

	private const string NAMESPACE_PREFIX = "Meta";

	private static FieldInfo GetCachedField(Type type, string fieldName)
	{
		string key = type.FullName + "." + fieldName;
		if (!_cachedFields.TryGetValue(key, out var value))
		{
			value = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			_cachedFields[key] = value;
		}
		return value;
	}

	private static PropertyInfo GetCachedProperty(Type type, string propertyName)
	{
		string key = type.FullName + "." + propertyName;
		if (!_cachedProperties.TryGetValue(key, out var value))
		{
			value = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			_cachedProperties[key] = value;
		}
		return value;
	}

	private static MethodInfo GetCachedMethod(Type type, string methodName)
	{
		string key = type.FullName + "." + methodName;
		if (!_cachedMethods.TryGetValue(key, out var value))
		{
			value = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			_cachedMethods[key] = value;
		}
		return value;
	}

	public static bool ReflectFieldValue<T>(object obj, string fieldName, out T data)
	{
		FieldInfo cachedField = GetCachedField(obj.GetType(), fieldName);
		if (null != cachedField)
		{
			data = (T)cachedField.GetValue(obj);
		}
		else
		{
			data = default(T);
		}
		return null != cachedField;
	}

	public static bool ReflectPropertyValue<T>(object obj, string fieldName, out T data)
	{
		PropertyInfo cachedProperty = GetCachedProperty(obj.GetType(), fieldName);
		if (null != cachedProperty)
		{
			data = (T)cachedProperty.GetValue(obj);
		}
		else
		{
			data = default(T);
		}
		return null != cachedProperty;
	}

	public static bool ReflectMethodValue<T>(object obj, string fieldName, out T data)
	{
		MethodInfo cachedMethod = GetCachedMethod(obj.GetType(), fieldName);
		if (null != cachedMethod)
		{
			data = (T)cachedMethod.Invoke(obj, null);
		}
		else
		{
			data = default(T);
		}
		return null != cachedMethod;
	}

	public static bool TryReflectValue<T>(object obj, string fieldName, out T value)
	{
		if (!ReflectFieldValue<T>(obj, fieldName, out value) && !ReflectPropertyValue<T>(obj, fieldName, out value))
		{
			return ReflectMethodValue<T>(obj, fieldName, out value);
		}
		return true;
	}

	public static T ReflectValue<T>(object obj, string fieldName)
	{
		if (TryReflectValue<T>(obj, fieldName, out var value))
		{
			return value;
		}
		throw new ArgumentException("No field, property, or method named '" + fieldName + "' was found.");
	}

	private static bool IsValidNamespace(Type type)
	{
		if ((object)type != null && type.Namespace != null)
		{
			return type.Namespace.StartsWith("Meta");
		}
		return false;
	}

	private static IEnumerable<Type> GetTypes()
	{
		return AppDomain.CurrentDomain.GetAssemblies().SelectMany(delegate(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch
			{
				return new Type[0];
			}
		}).Where(IsValidNamespace);
	}

	private static IEnumerable<MethodInfo> GetMethods()
	{
		return GetTypes().SelectMany((Type type) => type.GetMethods());
	}

	internal static Type[] GetAllAssignableTypes<T>()
	{
		return (from type in GetTypes()
			where typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract
			select type).ToArray();
	}

	internal static Type[] GetTypesWithAttribute<T>() where T : Attribute
	{
		return (from type in GetTypes()
			where type.GetCustomAttributes(typeof(T), inherit: false).Length != 0
			select type).ToArray();
	}

	internal static MethodInfo[] GetMethodsWithAttribute<T>() where T : Attribute
	{
		return (from method in GetMethods()
			where method.GetCustomAttributes(typeof(T), inherit: false).Length != 0
			select method).ToArray();
	}
}

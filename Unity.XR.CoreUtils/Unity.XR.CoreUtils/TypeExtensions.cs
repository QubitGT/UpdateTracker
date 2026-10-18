using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.XR.CoreUtils;

public static class TypeExtensions
{
	private static readonly List<FieldInfo> k_Fields = new List<FieldInfo>();

	private static readonly List<string> k_TypeNames = new List<string>();

	public static void GetAssignableTypes(this Type type, List<Type> list, Func<Type, bool> predicate = null)
	{
		ReflectionUtils.ForEachType(delegate(Type t)
		{
			if (type.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && (predicate == null || predicate(t)))
			{
				list.Add(t);
			}
		});
	}

	public static void GetImplementationsOfInterface(this Type type, List<Type> list)
	{
		if (type.IsInterface)
		{
			type.GetAssignableTypes(list);
		}
	}

	public static void GetExtensionsOfClass(this Type type, List<Type> list)
	{
		if (type.IsClass)
		{
			type.GetAssignableTypes(list);
		}
	}

	public static void GetGenericInterfaces(this Type type, Type genericInterface, List<Type> interfaces)
	{
		Type[] interfaces2 = type.GetInterfaces();
		foreach (Type type2 in interfaces2)
		{
			if (type2.IsGenericType && type2.GetGenericTypeDefinition() == genericInterface)
			{
				interfaces.Add(type2);
			}
		}
	}

	public static PropertyInfo GetPropertyRecursively(this Type type, string name, BindingFlags bindingAttr)
	{
		PropertyInfo propertyInfo = type.GetProperty(name, bindingAttr);
		if (propertyInfo != null)
		{
			return propertyInfo;
		}
		if (type.BaseType != null)
		{
			propertyInfo = type.BaseType.GetPropertyRecursively(name, bindingAttr);
		}
		return propertyInfo;
	}

	public static FieldInfo GetFieldRecursively(this Type type, string name, BindingFlags bindingAttr)
	{
		FieldInfo fieldInfo = type.GetField(name, bindingAttr);
		if (fieldInfo != null)
		{
			return fieldInfo;
		}
		if (type.BaseType != null)
		{
			fieldInfo = type.BaseType.GetFieldRecursively(name, bindingAttr);
		}
		return fieldInfo;
	}

	public static void GetFieldsRecursively(this Type type, List<FieldInfo> fields, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
	{
		while (true)
		{
			FieldInfo[] fields2 = type.GetFields(bindingAttr);
			foreach (FieldInfo item in fields2)
			{
				fields.Add(item);
			}
			Type baseType = type.BaseType;
			if (baseType != null)
			{
				type = baseType;
				continue;
			}
			break;
		}
	}

	public static void GetPropertiesRecursively(this Type type, List<PropertyInfo> fields, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
	{
		while (true)
		{
			PropertyInfo[] properties = type.GetProperties(bindingAttr);
			foreach (PropertyInfo item in properties)
			{
				fields.Add(item);
			}
			Type baseType = type.BaseType;
			if (baseType != null)
			{
				type = baseType;
				continue;
			}
			break;
		}
	}

	public static void GetInterfaceFieldsFromClasses(this IEnumerable<Type> classes, List<FieldInfo> fields, List<Type> interfaceTypes, BindingFlags bindingAttr)
	{
		foreach (Type interfaceType in interfaceTypes)
		{
			if (!interfaceType.IsInterface)
			{
				throw new ArgumentException($"Type {interfaceType} in interfaceTypes is not an interface!");
			}
		}
		foreach (Type @class in classes)
		{
			if (!@class.IsClass)
			{
				throw new ArgumentException($"Type {@class} in classes is not a class!");
			}
			k_Fields.Clear();
			@class.GetFieldsRecursively(k_Fields, bindingAttr);
			foreach (FieldInfo k_Field in k_Fields)
			{
				Type[] interfaces = k_Field.FieldType.GetInterfaces();
				foreach (Type item in interfaces)
				{
					if (interfaceTypes.Contains(item))
					{
						fields.Add(k_Field);
						break;
					}
				}
			}
		}
	}

	public static TAttribute GetAttribute<TAttribute>(this Type type, bool inherit = false) where TAttribute : Attribute
	{
		return (TAttribute)type.GetCustomAttributes(typeof(TAttribute), inherit)[0];
	}

	public static void IsDefinedGetInheritedTypes<TAttribute>(this Type type, List<Type> types) where TAttribute : Attribute
	{
		while (type != null)
		{
			if (type.IsDefined(typeof(TAttribute), inherit: true))
			{
				types.Add(type);
			}
			type = type.BaseType;
		}
	}

	public static FieldInfo GetFieldInTypeOrBaseType(this Type type, string fieldName)
	{
		FieldInfo field;
		while (true)
		{
			if (type == null)
			{
				return null;
			}
			field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (field != null)
			{
				break;
			}
			type = type.BaseType;
		}
		return field;
	}

	public static string GetNameWithGenericArguments(this Type type)
	{
		string name = type.Name;
		name = name.Replace('+', '.');
		if (!type.IsGenericType)
		{
			return name;
		}
		name = name.Split('`')[0];
		Type[] genericArguments = type.GetGenericArguments();
		int num = genericArguments.Length;
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = genericArguments[i].GetNameWithGenericArguments();
		}
		return name + "<" + string.Join(", ", array) + ">";
	}

	public static string GetNameWithFullGenericArguments(this Type type)
	{
		string name = type.Name;
		name = name.Replace('+', '.');
		if (!type.IsGenericType)
		{
			return name;
		}
		name = name.Split('`')[0];
		Type[] genericArguments = type.GetGenericArguments();
		int num = genericArguments.Length;
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = genericArguments[i].GetFullNameWithGenericArgumentsInternal();
		}
		return name + "<" + string.Join(", ", array) + ">";
	}

	public static string GetFullNameWithGenericArguments(this Type type)
	{
		Type type2 = type.DeclaringType;
		if (type2 != null && !type.IsGenericParameter)
		{
			k_TypeNames.Clear();
			string nameWithFullGenericArguments = type.GetNameWithFullGenericArguments();
			k_TypeNames.Add(nameWithFullGenericArguments);
			while (true)
			{
				Type declaringType = type2.DeclaringType;
				if (declaringType == null)
				{
					break;
				}
				nameWithFullGenericArguments = type2.GetNameWithFullGenericArguments();
				k_TypeNames.Insert(0, nameWithFullGenericArguments);
				type2 = declaringType;
			}
			nameWithFullGenericArguments = type2.GetFullNameWithGenericArguments();
			k_TypeNames.Insert(0, nameWithFullGenericArguments);
			return string.Join(".", k_TypeNames.ToArray());
		}
		return type.GetFullNameWithGenericArgumentsInternal();
	}

	private static string GetFullNameWithGenericArgumentsInternal(this Type type)
	{
		string fullName = type.FullName;
		if (!type.IsGenericType)
		{
			return fullName;
		}
		fullName = fullName.Split('`')[0];
		Type[] genericArguments = type.GetGenericArguments();
		int num = genericArguments.Length;
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = genericArguments[i].GetFullNameWithGenericArguments();
		}
		return fullName + "<" + string.Join(", ", array) + ">";
	}

	public static bool IsAssignableFromOrSubclassOf(this Type checkType, Type baseType)
	{
		if (!checkType.IsAssignableFrom(baseType))
		{
			return checkType.IsSubclassOf(baseType);
		}
		return true;
	}

	public static MethodInfo GetMethodRecursively(this Type type, string name, BindingFlags bindingAttr)
	{
		MethodInfo methodInfo = type.GetMethod(name, bindingAttr);
		if (methodInfo != null)
		{
			return methodInfo;
		}
		if (type.BaseType != null)
		{
			methodInfo = type.BaseType.GetMethodRecursively(name, bindingAttr);
		}
		return methodInfo;
	}
}

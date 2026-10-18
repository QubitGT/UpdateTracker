using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Utils;

internal static class MemberInfoExtensions
{
	public static object GetValue(this MemberInfo memberInfo, object instance)
	{
		MemberTypes memberType = memberInfo.MemberType;
		if ((memberType & MemberTypes.Field) != 0)
		{
			return ((FieldInfo)memberInfo).GetValue(instance);
		}
		if ((memberType & MemberTypes.Property) != 0)
		{
			PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
			if (propertyInfo.CanRead)
			{
				return propertyInfo.GetValue(instance);
			}
			Debug.LogWarning("Calling GetValue() from property cannot be read");
			return null;
		}
		Debug.LogWarning("Calling GetValue() from wrong member type, expect field/property");
		return null;
	}

	public static void SetValue(this MemberInfo memberInfo, object instance, object value)
	{
		MemberTypes memberType = memberInfo.MemberType;
		if ((memberType & MemberTypes.Field) != 0)
		{
			((FieldInfo)memberInfo).SetValue(instance, value);
		}
		else if ((memberType & MemberTypes.Property) != 0)
		{
			PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
			if (propertyInfo.CanWrite)
			{
				propertyInfo.SetValue(instance, value);
			}
			else
			{
				Debug.LogWarning("Calling SetValue() from property cannot be written");
			}
		}
		else
		{
			Debug.LogWarning("Calling SetValue() from wrong member type, expect field/property");
		}
	}

	public static Type GetDataType(this MemberInfo memberInfo)
	{
		MemberTypes memberType = memberInfo.MemberType;
		if ((memberType & MemberTypes.Field) != 0)
		{
			return ((FieldInfo)memberInfo).FieldType;
		}
		if ((memberType & MemberTypes.Property) != 0)
		{
			return ((PropertyInfo)memberInfo).PropertyType;
		}
		Debug.LogWarning("Calling GetDataType() from wrong member type, expect field/property");
		return null;
	}

	public static bool IsStatic(this MemberInfo memberInfo)
	{
		MemberTypes memberType = memberInfo.MemberType;
		if ((memberType & MemberTypes.Field) != 0)
		{
			return ((FieldInfo)memberInfo).IsStatic;
		}
		if ((memberType & MemberTypes.Property) != 0)
		{
			PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
			if (!propertyInfo.CanRead || !propertyInfo.GetMethod.IsStatic)
			{
				if (propertyInfo.CanWrite)
				{
					return propertyInfo.SetMethod.IsStatic;
				}
				return false;
			}
			return true;
		}
		if ((memberType & MemberTypes.Method) != 0)
		{
			return ((MethodInfo)memberInfo).IsStatic;
		}
		Debug.LogWarning("Calling IsStatic() from wrong member type, expect field/property");
		return false;
	}

	public static bool IsPublic(this MemberInfo memberInfo)
	{
		MemberTypes memberType = memberInfo.MemberType;
		if ((memberType & MemberTypes.Field) != 0)
		{
			return ((FieldInfo)memberInfo).IsPublic;
		}
		if ((memberType & MemberTypes.Property) != 0)
		{
			PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
			if (!propertyInfo.CanRead || !propertyInfo.GetMethod.IsPublic)
			{
				if (propertyInfo.CanWrite)
				{
					return propertyInfo.SetMethod.IsPublic;
				}
				return false;
			}
			return true;
		}
		if ((memberType & MemberTypes.Method) != 0)
		{
			return ((MethodInfo)memberInfo).IsPublic;
		}
		return false;
	}

	public static string BuildSignatureForDebugInspector(this MemberInfo memberInfo)
	{
		MemberTypes memberType = memberInfo.MemberType;
		if ((memberType & MemberTypes.Field) != 0)
		{
			FieldInfo fieldInfo = (FieldInfo)memberInfo;
			string text = (fieldInfo.IsPublic ? "public" : (fieldInfo.IsPrivate ? "private" : (fieldInfo.IsFamily ? "protected" : "internal")));
			return "<i>" + text + " " + fieldInfo.FieldType.Name + "</i> <b>" + fieldInfo.Name + "</b>";
		}
		if ((memberType & MemberTypes.Method) != 0)
		{
			MethodInfo methodInfo = (MethodInfo)memberInfo;
			string text2 = (methodInfo.IsPublic ? "public" : (methodInfo.IsPrivate ? "private" : (methodInfo.IsFamily ? "protected" : "internal")));
			return "<i>" + text2 + " " + methodInfo.ReturnType.Name + "</i> <b>" + methodInfo.Name + "</b>()";
		}
		if ((memberType & MemberTypes.Property) != 0)
		{
			PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
			MethodInfo getMethod = propertyInfo.GetMethod;
			string text3 = (getMethod.IsPublic ? "public" : (getMethod.IsPrivate ? "private" : (getMethod.IsFamily ? "protected" : "internal")));
			return "<i>" + text3 + " " + propertyInfo.PropertyType.Name + "</i> <b>" + propertyInfo.Name + "</b>";
		}
		return memberInfo.Name;
	}

	public static bool IsCompatibleWithDebugInspector(this MemberInfo memberInfo)
	{
		if (memberInfo as ConstructorInfo != null)
		{
			return false;
		}
		MemberTypes memberType = memberInfo.MemberType;
		if ((memberType & (MemberTypes.Field | MemberTypes.Method | MemberTypes.Property)) == 0)
		{
			return false;
		}
		if (memberInfo.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
		{
			return false;
		}
		if ((memberType & MemberTypes.Method) != 0)
		{
			MethodInfo methodInfo = (MethodInfo)memberInfo;
			if (methodInfo.GetParameters().Length != 0 || methodInfo.ReturnType != typeof(void))
			{
				return false;
			}
		}
		if (memberInfo is PropertyInfo { CanRead: false })
		{
			return false;
		}
		return true;
	}

	public static bool IsTypeEqual(this MemberInfo member, Type type)
	{
		if (!((member as FieldInfo)?.FieldType == type))
		{
			return (member as PropertyInfo)?.PropertyType == type;
		}
		return true;
	}

	public static bool IsBaseTypeEqual(this MemberInfo member, Type type)
	{
		if (!((member as FieldInfo)?.FieldType.BaseType == type))
		{
			return (member as PropertyInfo)?.PropertyType.BaseType == type;
		}
		return true;
	}

	public static bool CanBeChanged(this MemberInfo memberInfo)
	{
		return (memberInfo.MemberType & (MemberTypes.Field | MemberTypes.Property)) != 0;
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SouthPointe.Serialization.MessagePack;

public class MapDefinition
{
	private const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

	private static readonly Type[] serializableUnityTypes = new Type[8]
	{
		typeof(Color),
		typeof(Color32),
		typeof(Vector2),
		typeof(Vector3),
		typeof(Vector4),
		typeof(Quaternion),
		typeof(Vector2Int),
		typeof(Vector3Int)
	};

	private static readonly Type[] callbackTypes = new Type[4]
	{
		typeof(OnDeserializingAttribute),
		typeof(OnDeserializedAttribute),
		typeof(OnSerializingAttribute),
		typeof(OnSerializedAttribute)
	};

	public readonly Type Type;

	public readonly Dictionary<string, FieldInfo> FieldInfos;

	public readonly Dictionary<string, ITypeHandler> FieldHandlers;

	public readonly Dictionary<Type, MethodInfo[]> Callbacks;

	internal MapDefinition(SerializationContext context, Type type)
	{
		Type = type;
		if (!IsSerializable(context, type))
		{
			throw new CustomAttributeFormatException(type?.ToString() + " does not have System.SerializableAttribute defined");
		}
		FieldInfos = new Dictionary<string, FieldInfo>();
		FieldInfo[] fields = type.GetFields(context.MapOptions.FieldFlags);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (IsFieldSerializable(context, fieldInfo))
			{
				FieldInfos[fieldInfo.Name] = fieldInfo;
			}
		}
		FieldHandlers = new Dictionary<string, ITypeHandler>();
		foreach (FieldInfo value in FieldInfos.Values)
		{
			FieldHandlers.Add(value.Name, context.TypeHandlers.Get(value.FieldType));
		}
		Callbacks = new Dictionary<Type, MethodInfo[]>();
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
		Type[] array = callbackTypes;
		foreach (Type type2 in array)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			MethodInfo[] array2 = methods;
			foreach (MethodInfo methodInfo in array2)
			{
				if (AttributesExist(methodInfo, type2))
				{
					list.Add(methodInfo);
				}
			}
			if (list.Count > 0)
			{
				Callbacks[type2] = list.ToArray();
			}
		}
	}

	private bool IsSerializable(SerializationContext context, Type type)
	{
		if (!context.MapOptions.RequireSerializableAttribute)
		{
			return true;
		}
		if (Array.IndexOf(serializableUnityTypes, type) != -1)
		{
			return true;
		}
		return type.IsSerializable;
	}

	private bool AttributesExist(MemberInfo info, Type attributeType)
	{
		return info.GetCustomAttributes(attributeType, inherit: true).Length != 0;
	}

	private bool IsFieldSerializable(SerializationContext context, FieldInfo info)
	{
		if (AttributesExist(info, typeof(NonSerializedAttribute)))
		{
			return false;
		}
		if (context.MapOptions.IgnoreAutoPropertyValues && info.Name.StartsWith("<"))
		{
			return false;
		}
		return IsSerializable(context, info.FieldType);
	}
}

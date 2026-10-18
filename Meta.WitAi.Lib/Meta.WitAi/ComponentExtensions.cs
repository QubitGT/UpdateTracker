using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Meta.WitAi;

public static class ComponentExtensions
{
	[Serializable]
	public struct ComponentCopyData
	{
		public Type ComponentType;

		public FieldInfo[] Fields;

		public PropertyInfo[] Properties;
	}

	private static Dictionary<Type, ComponentCopyData> _data = new Dictionary<Type, ComponentCopyData>();

	public static void Copy<T>(this T toComponent, T fromComponent) where T : Component
	{
		if (!(toComponent == null))
		{
			ComponentCopyData copyData = fromComponent.GetCopyData();
			FieldInfo[] fields = copyData.Fields;
			foreach (FieldInfo fieldInfo in fields)
			{
				fieldInfo.SetValue(toComponent, fieldInfo.GetValue(fromComponent));
			}
			PropertyInfo[] properties = copyData.Properties;
			foreach (PropertyInfo propertyInfo in properties)
			{
				propertyInfo.SetValue(toComponent, propertyInfo.GetValue(fromComponent));
			}
		}
	}

	public static void PreloadCopyData<T>(this T thisComponent) where T : Component
	{
		thisComponent.GetCopyData();
	}

	private static ComponentCopyData GetCopyData<T>(this T thisComponent) where T : Component
	{
		Type typeFromHandle = typeof(T);
		if (!_data.ContainsKey(typeFromHandle))
		{
			ComponentCopyData value = default(ComponentCopyData);
			value.ComponentType = typeFromHandle;
			List<FieldInfo> list = new List<FieldInfo>();
			FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (!IsObsolete(fieldInfo.CustomAttributes))
				{
					list.Add(fieldInfo);
				}
			}
			value.Fields = list.ToArray();
			List<PropertyInfo> list2 = new List<PropertyInfo>();
			PropertyInfo[] properties = typeFromHandle.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (!IsObsolete(propertyInfo.CustomAttributes) && propertyInfo.CanWrite && propertyInfo.CanRead && !string.Equals(propertyInfo.Name, "name"))
				{
					list2.Add(propertyInfo);
				}
			}
			value.Properties = list2.ToArray();
			_data[typeFromHandle] = value;
		}
		return _data[typeFromHandle];
	}

	private static bool IsObsolete(IEnumerable<CustomAttributeData> attributes)
	{
		return HasCustomAttributes<ObsoleteAttribute>(attributes);
	}

	private static bool HasCustomAttributes<TAttribute>(IEnumerable<CustomAttributeData> attributes) where TAttribute : Attribute
	{
		if (attributes != null)
		{
			foreach (CustomAttributeData attribute in attributes)
			{
				if (attribute.AttributeType == typeof(TAttribute))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static T GetFirstComponentOfType<T>(this Component component)
	{
		return component.GetComponentsOfType<T>().First();
	}

	public static IEnumerable<T> GetComponentsOfType<T>(this Component component)
	{
		return (from c in component.GetComponents<MonoBehaviour>()
			where c is T
			select c).Cast<T>();
	}
}

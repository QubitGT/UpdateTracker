using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Liv.Lck.DependencyInjection;

public class LckMonoBehaviourDependencyInjector
{
	private const BindingFlags _bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private readonly LckServiceProvider _lckServiceProvider;

	public LckMonoBehaviourDependencyInjector(LckServiceProvider lckServiceProvider)
	{
		_lckServiceProvider = lckServiceProvider;
	}

	public void Inject(MonoBehaviour instance)
	{
		if (!IsInjectable(instance))
		{
			return;
		}
		Type type = instance.GetType();
		while (type != null && type != typeof(MonoBehaviour))
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (Attribute.IsDefined(fieldInfo, typeof(InjectLckAttribute)))
				{
					object service = _lckServiceProvider.GetService(fieldInfo.FieldType);
					if (service != null)
					{
						fieldInfo.SetValue(instance, service);
					}
				}
			}
			PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (Attribute.IsDefined(propertyInfo, typeof(InjectLckAttribute)) && propertyInfo.CanWrite)
				{
					object service2 = _lckServiceProvider.GetService(propertyInfo.PropertyType);
					if (service2 != null)
					{
						propertyInfo.SetValue(instance, service2);
					}
				}
			}
			foreach (MethodInfo item in from member in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where Attribute.IsDefined(member, typeof(InjectLckAttribute))
				select member)
			{
				Type[] array = (from parameter in item.GetParameters()
					select parameter.ParameterType).ToArray();
				List<object> list = new List<object>();
				Type[] array2 = array;
				foreach (Type type2 in array2)
				{
					object service3 = _lckServiceProvider.GetService(type2);
					if (service3 != null)
					{
						list.Add(service3);
						continue;
					}
					throw new Exception($"Failed to inject dependency {type2} into method '{item.Name}' of class '{type.Name}'.");
				}
				item.Invoke(instance, list.ToArray());
			}
			type = type.BaseType;
		}
	}

	private static bool IsInjectable(object obj)
	{
		Type type = obj.GetType();
		while (type != null && type != typeof(object))
		{
			if (type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any((MemberInfo member) => Attribute.IsDefined(member, typeof(InjectLckAttribute))))
			{
				return true;
			}
			type = type.BaseType;
		}
		return false;
	}
}

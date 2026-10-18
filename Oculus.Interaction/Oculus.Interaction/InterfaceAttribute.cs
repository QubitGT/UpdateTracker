using System;
using UnityEngine;

namespace Oculus.Interaction;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class InterfaceAttribute : PropertyAttribute
{
	public Type[] Types;

	public string TypeFromFieldName;

	public InterfaceAttribute(Type type, params Type[] types)
	{
		Types = new Type[types.Length + 1];
		Types[0] = type;
		for (int i = 0; i < types.Length; i++)
		{
			Types[i + 1] = types[i];
		}
	}

	public InterfaceAttribute(string typeFromFieldName)
	{
		TypeFromFieldName = typeFromFieldName;
	}
}

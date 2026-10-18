using System;
using UnityEngine;

namespace Unity.Cinemachine;

public sealed class HideIfNoComponentAttribute : PropertyAttribute
{
	public Type ComponentType;

	public HideIfNoComponentAttribute(Type type)
	{
		ComponentType = type;
	}
}

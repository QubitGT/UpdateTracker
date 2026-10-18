using System;

namespace UnityEngine.Rendering;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
public class DisplayInfoAttribute : Attribute
{
	public string name;

	public int order;
}

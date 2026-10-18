using System;

namespace UnityEngine.VFX.Utility;

[AttributeUsage(AttributeTargets.Class)]
public class VFXBinderAttribute : PropertyAttribute
{
	public string MenuPath;

	public VFXBinderAttribute(string menuPath)
	{
		MenuPath = menuPath;
	}
}

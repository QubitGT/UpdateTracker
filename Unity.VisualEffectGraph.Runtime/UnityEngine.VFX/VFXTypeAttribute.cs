using System;

namespace UnityEngine.VFX;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public class VFXTypeAttribute : Attribute
{
	[Flags]
	public enum Usage
	{
		Default = 1,
		GraphicsBuffer = 2,
		ExcludeFromProperty = 4
	}

	internal Usage usages { get; private set; }

	internal string name { get; private set; }

	public VFXTypeAttribute(Usage usages = Usage.Default, string name = null)
	{
		this.usages = usages;
		this.name = name;
	}
}

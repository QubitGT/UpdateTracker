using System;

namespace UnityEngine.XR.Interaction.Toolkit;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CanFocusMultipleAttribute : Attribute
{
	public bool allowMultiple { get; }

	public CanFocusMultipleAttribute(bool allowMultiple = true)
	{
		this.allowMultiple = allowMultiple;
	}
}

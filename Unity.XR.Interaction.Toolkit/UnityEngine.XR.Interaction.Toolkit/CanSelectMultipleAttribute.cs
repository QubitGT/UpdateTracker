using System;

namespace UnityEngine.XR.Interaction.Toolkit;

[AttributeUsage(AttributeTargets.Class)]
public sealed class CanSelectMultipleAttribute : Attribute
{
	public bool allowMultiple { get; }

	public CanSelectMultipleAttribute(bool allowMultiple = true)
	{
		this.allowMultiple = allowMultiple;
	}
}

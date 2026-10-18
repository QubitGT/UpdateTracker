using System;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

internal class RequireInterfaceAttribute : PropertyAttribute
{
	public Type interfaceType { get; }

	public RequireInterfaceAttribute(Type interfaceType)
	{
		this.interfaceType = interfaceType;
	}
}

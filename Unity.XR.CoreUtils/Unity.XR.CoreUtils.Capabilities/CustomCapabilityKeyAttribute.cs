using System;

namespace Unity.XR.CoreUtils.Capabilities;

[AttributeUsage(AttributeTargets.Field)]
public sealed class CustomCapabilityKeyAttribute : Attribute
{
	public readonly int Order;

	public CustomCapabilityKeyAttribute(int order = 1000)
	{
		Order = order;
	}
}

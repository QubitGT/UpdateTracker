using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Class)]
internal class UnityPropertyAttributeProxyAttribute : Attribute
{
	public UnityPropertyAttributeProxyAttribute(Type type)
	{
	}
}

using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class OnChangedRenderAttribute : Attribute
{
	public string MethodName { get; private set; }

	public OnChangedRenderAttribute(string methodName)
	{
		if (string.IsNullOrEmpty(methodName))
		{
			throw new ArgumentNullException("methodName", "Method name cannot be null or empty.");
		}
		MethodName = methodName;
	}
}

using System;

namespace Unity.Pipeline.MethodReload;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MethodReloadOverrideMethodAttribute : Attribute
{
	public string TargetMethodId { get; }

	public string Description { get; set; }

	public MethodReloadOverrideMethodAttribute(string targetMethodId)
	{
		TargetMethodId = targetMethodId ?? throw new ArgumentNullException("targetMethodId");
	}
}

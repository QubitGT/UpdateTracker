using System;

namespace Unity.Pipeline.MethodReload;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MethodReloadAttribute : Attribute
{
	public string Id { get; set; }

	public bool RequireMainThread { get; set; } = true;

}

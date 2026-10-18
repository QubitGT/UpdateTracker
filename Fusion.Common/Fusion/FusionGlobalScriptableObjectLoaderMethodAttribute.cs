using System;

namespace Fusion;

[Obsolete("Use one of FusionGlobalScriptableObjectSourceAttribute-derived types instead", true)]
[AttributeUsage(AttributeTargets.Method)]
public class FusionGlobalScriptableObjectLoaderMethodAttribute : Attribute
{
	public int Order { get; set; }
}

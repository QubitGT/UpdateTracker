using System;
using System.Collections.Generic;
using System.Reflection;

namespace Meta.Conduit;

internal class InvocationContext
{
	public Type Type { get; set; }

	public MethodInfo MethodInfo { get; set; }

	public float MinConfidence { get; set; }

	public float MaxConfidence { get; set; } = 1f;


	public bool ValidatePartial { get; set; }

	public Dictionary<string, string> ParameterMap { get; set; } = new Dictionary<string, string>();


	public Type CustomAttributeType { get; set; }
}

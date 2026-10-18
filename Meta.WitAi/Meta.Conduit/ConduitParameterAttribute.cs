using System;
using System.Collections.Generic;
using System.Linq;

namespace Meta.Conduit;

[AttributeUsage(AttributeTargets.Parameter)]
public class ConduitParameterAttribute : Attribute
{
	public List<string> Examples { get; }

	public List<string> Aliases { get; }

	public ConduitParameterAttribute(params string[] examples)
	{
		Examples = examples.ToList();
		Aliases = new List<string>();
	}

	public ConduitParameterAttribute(string[] aliases, params string[] examples)
	{
		Examples = examples.ToList();
		Aliases = aliases.ToList();
	}
}

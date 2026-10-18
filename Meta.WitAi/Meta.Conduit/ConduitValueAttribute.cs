using System;

namespace Meta.Conduit;

[AttributeUsage(AttributeTargets.Field)]
public class ConduitValueAttribute : Attribute
{
	public string[] Aliases { get; }

	public ConduitValueAttribute(params string[] aliases)
	{
		Aliases = aliases;
	}
}

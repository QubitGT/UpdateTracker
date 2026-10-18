using System;

namespace Meta.Conduit;

[AttributeUsage(AttributeTargets.Enum)]
public class ConduitEntityAttribute : Attribute
{
	public string Name { get; }

	public string ID { get; }

	public ConduitEntityAttribute(string name, string id = null)
	{
		Name = name;
		ID = id;
	}
}

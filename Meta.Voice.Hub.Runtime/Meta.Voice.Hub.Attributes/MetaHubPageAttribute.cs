using System;
using Meta.Voice.Hub.Interfaces;

namespace Meta.Voice.Hub.Attributes;

public class MetaHubPageAttribute : Attribute, IPageInfo
{
	public string Name { get; private set; }

	public string Context { get; private set; }

	public int Priority { get; private set; }

	public string Prefix { get; private set; }

	public MetaHubPageAttribute(string name = null, string context = "", string prefix = "", int priority = 0)
	{
		Name = name;
		Context = context;
		Priority = priority;
		Prefix = prefix;
	}
}

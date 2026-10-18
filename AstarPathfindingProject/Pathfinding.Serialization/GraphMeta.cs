using System;
using System.Collections.Generic;

namespace Pathfinding.Serialization;

public class GraphMeta
{
	public Version version;

	public int graphs;

	public List<string> guids;

	public List<string> typeNames;

	public Type GetGraphType(int index, Type[] availableGraphTypes)
	{
		if (string.IsNullOrEmpty(typeNames[index]))
		{
			return null;
		}
		for (int i = 0; i < availableGraphTypes.Length; i++)
		{
			if (availableGraphTypes[i].FullName == typeNames[index])
			{
				return availableGraphTypes[i];
			}
		}
		throw new Exception("No graph of type '" + typeNames[index] + "' could be created, type does not exist");
	}
}

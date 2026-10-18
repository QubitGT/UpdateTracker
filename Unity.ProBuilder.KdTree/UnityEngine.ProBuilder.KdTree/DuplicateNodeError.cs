using System;

namespace UnityEngine.ProBuilder.KdTree;

internal class DuplicateNodeError : Exception
{
	public DuplicateNodeError()
		: base("Cannot Add Node With Duplicate Coordinates")
	{
	}
}

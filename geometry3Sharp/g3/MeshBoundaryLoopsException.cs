using System;

namespace g3;

public class MeshBoundaryLoopsException : Exception
{
	public bool UnclosedLoop;

	public bool BowtieFailure;

	public bool RepeatedEdge;

	public MeshBoundaryLoopsException(string message)
		: base(message)
	{
	}
}

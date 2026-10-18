using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property)]
public class NetworkedWeavedLinkedListAttribute : Attribute
{
	public int Capacity { get; }

	public int ElementWordCount { get; }

	public Type ElementReaderWriterType { get; }

	public NetworkedWeavedLinkedListAttribute(int capacity, int elementWordCount, Type elementReaderWriterType)
	{
		Capacity = capacity;
		ElementWordCount = elementWordCount;
		ElementReaderWriterType = elementReaderWriterType;
	}
}

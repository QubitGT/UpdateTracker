using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property)]
public class NetworkedWeavedArrayAttribute : Attribute
{
	public int Capacity { get; }

	public int ElementWordCount { get; }

	public Type ElementReaderWriterType { get; }

	public NetworkedWeavedArrayAttribute(int capacity, int elementWordCount, Type elementReaderWriter)
	{
		Capacity = capacity;
		ElementWordCount = elementWordCount;
		ElementReaderWriterType = elementReaderWriter;
	}
}

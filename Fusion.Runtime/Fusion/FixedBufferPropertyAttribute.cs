using System;

namespace Fusion;

public class FixedBufferPropertyAttribute : PropertyAttribute
{
	public Type Type { get; }

	public Type SurrogateType { get; }

	public int Capacity { get; }

	public FixedBufferPropertyAttribute(Type fieldType, Type surrogateType, int capacity)
	{
		Type = fieldType;
		SurrogateType = surrogateType;
		Capacity = capacity;
	}
}

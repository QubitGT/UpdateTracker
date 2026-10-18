using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property)]
public class NetworkedWeavedDictionaryAttribute : Attribute
{
	public int Capacity { get; }

	public int KeyWordCount { get; }

	public int ValueWordCount { get; }

	public Type KeyReaderWriterType { get; set; }

	public Type ValueReaderWriterType { get; set; }

	public NetworkedWeavedDictionaryAttribute(int capacity, int keyWordCount, int elementWordCount, Type keyReaderWriterType, Type valueReaderWriterType)
	{
		Capacity = capacity;
		KeyWordCount = keyWordCount;
		ValueWordCount = elementWordCount;
		KeyReaderWriterType = keyReaderWriterType;
		ValueReaderWriterType = valueReaderWriterType;
	}
}

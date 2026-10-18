using System;

namespace Fusion.Internal;

[Serializable]
public abstract class UnityLinkedListSurrogate<T, ReaderWriter> : UnitySurrogateBase where T : unmanaged where ReaderWriter : unmanaged, IElementReaderWriter<T>
{
	private static IElementReaderWriter<T> _readerWriter = new ReaderWriter();

	public abstract T[] DataProperty { get; set; }

	public unsafe override void Read(int* data, int capacity)
	{
		NetworkLinkedList<T> networkLinkedList = new NetworkLinkedList<T>((byte*)data, capacity, _readerWriter);
		T[] array = DataProperty;
		Array.Resize(ref array, networkLinkedList.Count);
		int num = 0;
		foreach (T item in networkLinkedList)
		{
			array[num++] = item;
		}
		DataProperty = array;
	}

	public unsafe override void Write(int* data, int capacity)
	{
		NetworkLinkedList<T> networkLinkedList = new NetworkLinkedList<T>((byte*)data, capacity, _readerWriter);
		networkLinkedList.Clear();
		T[] dataProperty = DataProperty;
		foreach (T value in dataProperty)
		{
			networkLinkedList.Add(value);
		}
	}

	public override void Init(int capacity)
	{
		DataProperty = Array.Empty<T>();
	}
}

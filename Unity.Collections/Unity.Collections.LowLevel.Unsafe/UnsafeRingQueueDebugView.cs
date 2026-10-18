namespace Unity.Collections.LowLevel.Unsafe;

internal sealed class UnsafeRingQueueDebugView<T> where T : unmanaged
{
	private UnsafeRingQueue<T> Data;

	public unsafe T[] Items
	{
		get
		{
			T[] array = new T[Data.Length];
			int read = Data.m_Read;
			int capacity = Data.m_Capacity;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Data.Ptr[(read + i) % capacity];
			}
			return array;
		}
	}

	public UnsafeRingQueueDebugView(UnsafeRingQueue<T> data)
	{
		Data = data;
	}
}

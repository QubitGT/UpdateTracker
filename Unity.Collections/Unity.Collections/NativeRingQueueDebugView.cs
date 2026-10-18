using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

internal sealed class NativeRingQueueDebugView<T> where T : unmanaged
{
	private unsafe UnsafeRingQueue<T>* Data;

	public unsafe T[] Items
	{
		get
		{
			T[] array = new T[Data->Length];
			int read = Data->m_Read;
			int capacity = Data->m_Capacity;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Data->Ptr[(read + i) % capacity];
			}
			return array;
		}
	}

	public unsafe NativeRingQueueDebugView(NativeRingQueue<T> data)
	{
		Data = data.m_RingQueue;
	}
}

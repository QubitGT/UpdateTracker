using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

internal sealed class NativeListDebugView<T> where T : unmanaged
{
	private unsafe UnsafeList<T>* Data;

	public unsafe T[] Items
	{
		get
		{
			if (Data == null)
			{
				return null;
			}
			int length = Data->Length;
			T[] array = new T[length];
			fixed (T* destination = &array[0])
			{
				UnsafeUtility.MemCpy(destination, Data->Ptr, length * UnsafeUtility.SizeOf<T>());
			}
			return array;
		}
	}

	public unsafe NativeListDebugView(NativeList<T> array)
	{
		Data = array.m_ListData;
	}
}

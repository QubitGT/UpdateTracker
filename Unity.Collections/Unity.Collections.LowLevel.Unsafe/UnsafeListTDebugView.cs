namespace Unity.Collections.LowLevel.Unsafe;

internal sealed class UnsafeListTDebugView<T> where T : unmanaged
{
	private UnsafeList<T> Data;

	public unsafe T[] Items
	{
		get
		{
			T[] array = new T[Data.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Data.Ptr[i];
			}
			return array;
		}
	}

	public UnsafeListTDebugView(UnsafeList<T> data)
	{
		Data = data;
	}
}

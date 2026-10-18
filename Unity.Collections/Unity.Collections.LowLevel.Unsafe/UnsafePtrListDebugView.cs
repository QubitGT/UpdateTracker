namespace Unity.Collections.LowLevel.Unsafe;

internal sealed class UnsafePtrListDebugView<T> where T : unmanaged
{
	private UnsafePtrList<T> Data;

	public unsafe T*[] Items
	{
		get
		{
			T*[] array = new T*[Data.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Data.Ptr[i];
			}
			return array;
		}
	}

	public UnsafePtrListDebugView(UnsafePtrList<T> data)
	{
		Data = data;
	}
}

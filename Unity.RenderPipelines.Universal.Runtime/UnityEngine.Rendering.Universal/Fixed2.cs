using System.Diagnostics;

namespace UnityEngine.Rendering.Universal;

internal struct Fixed2<T> where T : unmanaged
{
	public T item1;

	public T item2;

	public unsafe T this[int index]
	{
		get
		{
			fixed (T* ptr = &item1)
			{
				return ptr[index];
			}
		}
		set
		{
			fixed (T* ptr = &item1)
			{
				ptr[index] = value;
			}
		}
	}

	public Fixed2(T item1)
		: this(item1, item1)
	{
	}

	public Fixed2(T item1, T item2)
	{
		this.item1 = item1;
		this.item2 = item2;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckRange(int index)
	{
	}
}

using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

internal struct Long1024 : IIndexable<long>
{
	internal Long512 f0;

	internal Long512 f1;

	public int Length
	{
		get
		{
			return 1024;
		}
		set
		{
		}
	}

	public unsafe ref long ElementAt(int index)
	{
		fixed (Long512* ptr = &f0)
		{
			return ref UnsafeUtility.AsRef<long>((byte*)ptr + (nint)index * (nint)8);
		}
	}
}

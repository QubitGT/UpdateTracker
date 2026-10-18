using System;
using System.Runtime.CompilerServices;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct NativeParallelMultiHashMapIterator<TKey> where TKey : unmanaged
{
	internal TKey key;

	internal int NextEntryIndex;

	internal int EntryIndex;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetEntryIndex()
	{
		return EntryIndex;
	}
}

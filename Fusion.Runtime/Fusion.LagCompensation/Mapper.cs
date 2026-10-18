#define DEBUG
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fusion.LagCompensation;

internal class Mapper
{
	private readonly Dictionary<HitboxRoot, int> _rootToNodeIndex = new Dictionary<HitboxRoot, int>();

	internal int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _rootToNodeIndex.Count;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool TryGetLeafIndex(HitboxRoot root, out int index)
	{
		return _rootToNodeIndex.TryGetValue(root, out index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int GetLeafIndex(HitboxRoot root)
	{
		Assert.Check(_rootToNodeIndex.ContainsKey(root));
		return _rootToNodeIndex[root];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void RegisterMapping(HitboxRoot root, int leafIndex)
	{
		_rootToNodeIndex[root] = leafIndex;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void DeRegister(HitboxRoot root)
	{
		_rootToNodeIndex.Remove(root);
	}
}

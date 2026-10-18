#define DEBUG
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion;

internal class NetworkObjectHeaderSnapshot
{
	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private Allocator _003Callocator_003EP;

	internal NetworkObjectHeaderSnapshot Prev;

	internal NetworkObjectHeaderSnapshot Next;

	public Tick Tick;

	public int WordCount;

	private unsafe int* _ptr;

	public unsafe NetworkObjectHeaderPtr HeaderPtr
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new NetworkObjectHeaderPtr((NetworkObjectHeader*)_ptr);
		}
	}

	public unsafe ref NetworkObjectHeader Header
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref *(NetworkObjectHeader*)_ptr;
		}
	}

	public unsafe Span<int> Raw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new Span<int>(_ptr, WordCount);
		}
	}

	public NetworkObjectHeaderSnapshot(Allocator allocator)
	{
		_003Callocator_003EP = allocator;
		base._002Ector();
	}

	public void Init(NetworkObjectMeta meta, bool copyData)
	{
		Init(meta.WordCount);
		if (copyData)
		{
			CopyFrom(meta);
		}
	}

	public unsafe void Init(int wordCount)
	{
		Assert.Check(_003Callocator_003EP);
		Assert.Check(_ptr == null);
		WordCount = wordCount;
		_ptr = (int*)Allocator.AllocAndClear(_003Callocator_003EP, wordCount * 4);
	}

	public unsafe void Release()
	{
		Tick = default(Tick);
		WordCount = 0;
		Allocator.Free(_003Callocator_003EP, ref _ptr);
	}

	public NetworkObjectHeaderSnapshot Clone(Simulation simulation)
	{
		NetworkObjectHeaderSnapshot snapshot = simulation.GetSnapshot();
		snapshot.Init(WordCount);
		snapshot.CopyFrom(this);
		return snapshot;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CopyTo(int[] target)
	{
		Native.MemCpy(target, Raw);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CopyFrom(NetworkObjectMeta meta)
	{
		Native.MemCpy(Raw, meta.Raw);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CopyTo(NetworkObjectMeta meta)
	{
		Native.MemCpy(meta.Raw, Raw);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CopyFrom(NetworkObjectHeaderSnapshot target)
	{
		Native.MemCpy(Raw, target.Raw);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CopyTo(NetworkObjectHeaderSnapshot target)
	{
		Native.MemCpy(target.Raw, Raw);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CopyFrom(NetworkObjectHeaderSnapshotRef target)
	{
		Native.MemCpy(Raw, target.Raw);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CopyTo(NetworkObjectHeaderSnapshotRef target)
	{
		Native.MemCpy(target.Raw, Raw);
	}

	internal unsafe int* GetBehaviourPtr(NetworkBehaviour behaviour)
	{
		return _ptr + behaviour.WordOffset;
	}

	internal ulong BuildCRC()
	{
		return CRC64.Compute(Raw);
	}
}

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion;

internal readonly ref struct NetworkObjectHeaderSnapshotRef
{
	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly NetworkObjectHeaderSnapshot _003Csnapshot_003EP;

	public Tick Tick
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _003Csnapshot_003EP.Tick;
		}
	}

	public ref NetworkObjectHeader Header
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref _003Csnapshot_003EP.Header;
		}
	}

	public ulong SnapshotCRC
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _003Csnapshot_003EP.BuildCRC();
		}
	}

	internal Span<int> Raw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _003Csnapshot_003EP.Raw;
		}
	}

	public NetworkObjectHeaderSnapshotRef(NetworkObjectHeaderSnapshot snapshot)
	{
		_003Csnapshot_003EP = snapshot;
	}

	public static implicit operator NetworkObjectHeaderSnapshotRef(NetworkObjectHeaderSnapshot snapshot)
	{
		return new NetworkObjectHeaderSnapshotRef(snapshot);
	}

	public void CopyFrom(NetworkObjectMeta target)
	{
		_003Csnapshot_003EP.CopyFrom(target);
	}

	public void CopyFrom(NetworkObjectHeaderSnapshotRef target)
	{
		_003Csnapshot_003EP.CopyFrom(target);
	}

	public void CopyTo(NetworkObjectMeta target)
	{
		_003Csnapshot_003EP.CopyTo(target);
	}

	public void CopyTo(int[] target)
	{
		_003Csnapshot_003EP.CopyTo(target);
	}

	public void CopyTo(NetworkObjectHeaderSnapshotRef target)
	{
		_003Csnapshot_003EP.CopyTo(target);
	}

	internal unsafe int* GetBehaviourPtr(NetworkBehaviour behaviour)
	{
		return _003Csnapshot_003EP.GetBehaviourPtr(behaviour);
	}
}

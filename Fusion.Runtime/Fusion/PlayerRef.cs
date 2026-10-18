#define DEBUG
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Fusion.Sockets;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(1)]
public struct PlayerRef : INetworkStruct, IEquatable<PlayerRef>
{
	private sealed class IndexEqualityComparer : IEqualityComparer<PlayerRef>
	{
		public bool Equals(PlayerRef x, PlayerRef y)
		{
			return x._index == y._index;
		}

		public int GetHashCode(PlayerRef obj)
		{
			return obj._index;
		}
	}

	public const int SIZE = 4;

	private const int MASTER_CLIENT_RAW = -1;

	private const int INVALID_RAW = -10;

	[FieldOffset(0)]
	private int _index;

	public static IEqualityComparer<PlayerRef> Comparer { get; } = new IndexEqualityComparer();


	public static PlayerRef Invalid
	{
		get
		{
			PlayerRef result = default(PlayerRef);
			result._index = -10;
			return result;
		}
	}

	public static PlayerRef None => default(PlayerRef);

	public static PlayerRef MasterClient
	{
		get
		{
			PlayerRef result = default(PlayerRef);
			result._index = -1;
			return result;
		}
	}

	public bool IsRealPlayer
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _index > 0;
		}
	}

	public bool IsNone
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _index == 0;
		}
	}

	public bool IsMasterClient
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _index == -1;
		}
	}

	public int RawEncoded => _index;

	public int AsIndex => _index - 1;

	public int PlayerId => _index - 1;

	public override bool Equals(object obj)
	{
		return obj is PlayerRef other && Equals(other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return _index;
	}

	public override string ToString()
	{
		return (_index > 0) ? $"[Player:{_index - 1}]" : ((_index == -1) ? "[Player:MasterClient]" : "[Player:None]");
	}

	public static PlayerRef FromEncoded(int encoded)
	{
		PlayerRef result = default(PlayerRef);
		result._index = encoded;
		return result;
	}

	public static PlayerRef FromIndex(int index)
	{
		PlayerRef result = default(PlayerRef);
		result._index = index + 1;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(PlayerRef a, PlayerRef b)
	{
		return a._index == b._index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(PlayerRef a, PlayerRef b)
	{
		return a._index != b._index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Write(NetBitBuffer* buffer, PlayerRef playerRef)
	{
		if (buffer->WriteBoolean(playerRef.IsRealPlayer))
		{
			buffer->WriteInt32VarLength(playerRef.AsIndex);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Write<T>(T* buffer, PlayerRef playerRef) where T : unmanaged, INetBitWriteStream
	{
		if (buffer->WriteBoolean(playerRef.IsRealPlayer))
		{
			buffer->WriteInt32VarLength(playerRef.AsIndex);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static PlayerRef Read(NetBitBuffer* buffer)
	{
		if (buffer->ReadBoolean())
		{
			PlayerRef result = FromIndex(buffer->ReadInt32VarLength());
			Assert.Check(!result.IsNone);
			return result;
		}
		return default(PlayerRef);
	}

	public bool Equals(PlayerRef other)
	{
		return _index == other._index;
	}
}

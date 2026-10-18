using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Fusion.Sockets;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(1)]
public struct NetworkId : INetworkStruct, IEquatable<NetworkId>, IComparable, IComparable<NetworkId>
{
	public sealed class EqualityComparer : IEqualityComparer<NetworkId>
	{
		public bool Equals(NetworkId a, NetworkId b)
		{
			return a.Raw == b.Raw;
		}

		public int GetHashCode(NetworkId id)
		{
			return (int)id.Raw;
		}
	}

	public const int BLOCK_SIZE = 8;

	public const int SIZE = 4;

	public const int ALIGNMENT = 4;

	[FieldOffset(0)]
	public uint Raw;

	internal const int MAX_RESERVED_ID = 1023;

	private const uint RAW_RUNTIME_CONFIG = 1u;

	private const uint RAW_PLAYER_REF_DATA_ARRAY = 2u;

	private const uint RAW_SCENE_INFO = 3u;

	private const uint RAW_PHYSICS_INFO = 4u;

	public static EqualityComparer Comparer { get; } = new EqualityComparer();


	public bool IsValid
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Raw != 0;
		}
	}

	public bool IsReserved
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Raw != 0 && Raw <= 1023;
		}
	}

	internal static NetworkId RuntimeConfig => new NetworkId(1u);

	internal static NetworkId SceneInfo => new NetworkId(3u);

	internal static NetworkId PhysicsInfo => new NetworkId(4u);

	internal NetworkId(uint raw)
	{
		Raw = raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(NetworkId other)
	{
		return Raw == other.Raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(NetworkId other)
	{
		return (int)(Raw - other.Raw);
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkId networkId && Raw == networkId.Raw;
	}

	int IComparable.CompareTo(object obj)
	{
		return (obj is NetworkId other) ? CompareTo(other) : 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(NetworkId a, NetworkId b)
	{
		return a.Raw == b.Raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(NetworkId a, NetworkId b)
	{
		return a.Raw != b.Raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator bool(NetworkId id)
	{
		return id.Raw != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Write(NetBitBuffer* buffer, NetworkId id)
	{
		buffer->WriteUInt32VarLength(id.Raw, 8);
	}

	public unsafe static NetworkId Read(NetBitBuffer* buffer)
	{
		NetworkId result = default(NetworkId);
		result.Raw = buffer->ReadUInt32VarLength(8);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(NetBitBuffer* buffer)
	{
		Write(buffer, this);
	}

	public override int GetHashCode()
	{
		return (int)Raw;
	}

	public override string ToString()
	{
		if (IsValid)
		{
			return Raw switch
			{
				1u => "[Id:RuntimeConfig]", 
				3u => "[Id:SceneInfo]", 
				4u => "[Id:Physics]", 
				2u => "[Id:PlayerDataArray]", 
				_ => $"[Id:{Raw}]", 
			};
		}
		return "[Id:None]";
	}

	public string ToNamePrefixString()
	{
		return IsValid ? $"[{Raw}] " : "[Invalid] ";
	}
}

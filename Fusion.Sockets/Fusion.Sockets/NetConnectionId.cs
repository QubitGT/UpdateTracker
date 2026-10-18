using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fusion.Sockets;

[StructLayout(LayoutKind.Explicit)]
public struct NetConnectionId : IEquatable<NetConnectionId>
{
	public class EqualityComparer : IEqualityComparer<NetConnectionId>
	{
		public bool Equals(NetConnectionId x, NetConnectionId y)
		{
			return x.Raw == y.Raw;
		}

		public int GetHashCode(NetConnectionId obj)
		{
			return obj.Raw.GetHashCode();
		}
	}

	[FieldOffset(0)]
	internal ulong Raw;

	[FieldOffset(0)]
	public short Group;

	[FieldOffset(2)]
	public short GroupIndex;

	[FieldOffset(4)]
	internal uint Generation;

	public bool Equals(NetConnectionId other)
	{
		return Raw == other.Raw;
	}

	public override bool Equals(object obj)
	{
		return obj is NetConnectionId other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Raw.GetHashCode();
	}

	public static bool operator ==(NetConnectionId a, NetConnectionId b)
	{
		return a.Raw == b.Raw;
	}

	public static bool operator !=(NetConnectionId a, NetConnectionId b)
	{
		return a.Raw != b.Raw;
	}

	public override string ToString()
	{
		return $"[NetConnectionId Group:{Group}, GroupIndex:{GroupIndex}, Generation:{Generation}]";
	}
}

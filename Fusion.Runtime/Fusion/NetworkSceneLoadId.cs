using System;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 1)]
public readonly struct NetworkSceneLoadId : IEquatable<NetworkSceneLoadId>
{
	public readonly byte Value;

	public NetworkSceneLoadId(byte value)
	{
		Value = value;
	}

	public bool Equals(NetworkSceneLoadId other)
	{
		return Value == other.Value;
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkSceneLoadId other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(NetworkSceneLoadId left, NetworkSceneLoadId right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(NetworkSceneLoadId left, NetworkSceneLoadId right)
	{
		return !left.Equals(right);
	}

	public static implicit operator NetworkSceneLoadId(byte value)
	{
		return new NetworkSceneLoadId(value);
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}

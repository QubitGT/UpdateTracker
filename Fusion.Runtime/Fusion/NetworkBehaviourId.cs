using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(2)]
public struct NetworkBehaviourId : INetworkStruct, IEquatable<NetworkBehaviourId>
{
	public const int SIZE = 8;

	[FieldOffset(0)]
	public NetworkId Object;

	[FieldOffset(4)]
	public int Behaviour;

	public bool IsValid => Object.IsValid && Behaviour >= 0;

	public static NetworkBehaviourId None => default(NetworkBehaviourId);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(NetworkBehaviourId other)
	{
		return Object.Equals(other.Object) && Behaviour == other.Behaviour;
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkBehaviourId other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (Object.GetHashCode() * 397) ^ Behaviour;
	}

	public override string ToString()
	{
		return $"[Object:{Object}, Behaviour:{Behaviour}]";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(NetworkBehaviourId a, NetworkBehaviourId b)
	{
		return a.Equals(b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(NetworkBehaviourId a, NetworkBehaviourId b)
	{
		return !a.Equals(b);
	}
}

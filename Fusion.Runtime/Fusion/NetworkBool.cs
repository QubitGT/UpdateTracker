using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(1)]
public struct NetworkBool : INetworkStruct, IEquatable<NetworkBool>
{
	public const int SIZE = 4;

	[FieldOffset(0)]
	[SerializeField]
	private int _value;

	public NetworkBool(bool value)
	{
		_value = (value ? 1 : 0);
	}

	public bool Equals(NetworkBool other)
	{
		return _value == other._value;
	}

	public override string ToString()
	{
		return (_value == 0) ? "false" : "true";
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkBool other && Equals(other);
	}

	public override int GetHashCode()
	{
		return _value;
	}

	public static implicit operator bool(NetworkBool val)
	{
		return val._value == 1;
	}

	public static implicit operator NetworkBool(bool val)
	{
		NetworkBool result = default(NetworkBool);
		result._value = (val ? 1 : 0);
		return result;
	}
}

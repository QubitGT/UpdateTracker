using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(4)]
public struct NetworkObjectGuid : INetworkStruct, IEquatable<NetworkObjectGuid>, IComparable<NetworkObjectGuid>
{
	public sealed class EqualityComparer : IEqualityComparer<NetworkObjectGuid>
	{
		public bool Equals(NetworkObjectGuid x, NetworkObjectGuid y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(NetworkObjectGuid obj)
		{
			return obj.GetHashCode();
		}
	}

	public const int SIZE = 16;

	public const int ALIGNMENT = 4;

	[FieldOffset(0)]
	public unsafe fixed long RawGuidValue[2];

	[FieldOffset(0)]
	[NonSerialized]
	private long _data0;

	[FieldOffset(8)]
	[NonSerialized]
	private long _data1;

	public static NetworkObjectGuid Empty => default(NetworkObjectGuid);

	public bool IsValid => _data0 != 0L || _data1 != 0;

	public NetworkObjectGuid(string guid)
	{
		this = new Guid(guid);
	}

	public NetworkObjectGuid(long data0, long data1)
	{
		_data0 = data0;
		_data1 = data1;
	}

	public NetworkObjectGuid(byte[] guid)
	{
		_data0 = BitConverter.ToInt64(guid, 0);
		_data1 = BitConverter.ToInt64(guid, 8);
	}

	public unsafe NetworkObjectGuid(byte* guid)
	{
		_data0 = *(long*)guid;
		_data1 = *(long*)(guid + 8);
	}

	public unsafe static implicit operator NetworkObjectGuid(Guid guid)
	{
		NetworkObjectGuid result = default(NetworkObjectGuid);
		NetworkObjectGuidUtils.CopyAndMangleGuid((byte*)(&guid), (byte*)(&result));
		return result;
	}

	public unsafe static implicit operator Guid(NetworkObjectGuid guid)
	{
		Guid result = default(Guid);
		NetworkObjectGuidUtils.CopyAndMangleGuid((byte*)(&guid), (byte*)(&result));
		return result;
	}

	public static bool TryParse(string str, out NetworkObjectGuid guid)
	{
		if (Guid.TryParse(str, out var result))
		{
			guid = result;
			return true;
		}
		guid = default(NetworkObjectGuid);
		return false;
	}

	public static NetworkObjectGuid Parse(string str)
	{
		return Guid.Parse(str);
	}

	public static bool operator ==(NetworkObjectGuid a, NetworkObjectGuid b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(NetworkObjectGuid a, NetworkObjectGuid b)
	{
		return !a.Equals(b);
	}

	public bool Equals(NetworkObjectGuid other)
	{
		return _data0 == other._data0 && _data1 == other._data1;
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkObjectGuid other && Equals(other);
	}

	public override int GetHashCode()
	{
		return ((Guid)this).GetHashCode();
	}

	public override string ToString()
	{
		return ((Guid)this).ToString();
	}

	public string ToUnityGuidString()
	{
		return ToString("N");
	}

	public string ToString(string format)
	{
		return ((Guid)this).ToString(format);
	}

	public int CompareTo(NetworkObjectGuid other)
	{
		long num = _data0 - other._data0;
		if (num == 0)
		{
			num = _data1 - other._data1;
			if (num == 0)
			{
				return 0;
			}
		}
		if (num < 0)
		{
			return -1;
		}
		return 1;
	}

	public unsafe static explicit operator NetworkPrefabRef(NetworkObjectGuid t)
	{
		return new NetworkPrefabRef((byte*)(&t));
	}
}

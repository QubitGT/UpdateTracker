using System;

namespace Steamworks;

[Serializable]
public struct RemotePlayCursorID_t : IEquatable<RemotePlayCursorID_t>, IComparable<RemotePlayCursorID_t>
{
	public uint m_RemotePlayCursorID;

	public RemotePlayCursorID_t(uint value)
	{
		m_RemotePlayCursorID = value;
	}

	public override string ToString()
	{
		return m_RemotePlayCursorID.ToString();
	}

	public override bool Equals(object other)
	{
		if (other is RemotePlayCursorID_t)
		{
			return this == (RemotePlayCursorID_t)other;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_RemotePlayCursorID.GetHashCode();
	}

	public static bool operator ==(RemotePlayCursorID_t x, RemotePlayCursorID_t y)
	{
		return x.m_RemotePlayCursorID == y.m_RemotePlayCursorID;
	}

	public static bool operator !=(RemotePlayCursorID_t x, RemotePlayCursorID_t y)
	{
		return !(x == y);
	}

	public static explicit operator RemotePlayCursorID_t(uint value)
	{
		return new RemotePlayCursorID_t(value);
	}

	public static explicit operator uint(RemotePlayCursorID_t that)
	{
		return that.m_RemotePlayCursorID;
	}

	public bool Equals(RemotePlayCursorID_t other)
	{
		return m_RemotePlayCursorID == other.m_RemotePlayCursorID;
	}

	public int CompareTo(RemotePlayCursorID_t other)
	{
		return m_RemotePlayCursorID.CompareTo(other.m_RemotePlayCursorID);
	}
}

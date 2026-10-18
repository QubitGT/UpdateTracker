using System;

namespace Technie.PhysicsCreator;

[Serializable]
public class Hash160
{
	public byte[] data;

	public Hash160()
	{
		data = new byte[0];
	}

	public Hash160(byte[] data)
	{
		this.data = data;
	}

	public bool IsValid()
	{
		if (data != null)
		{
			return data.Length != 0;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (data == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < data.Length; i += 4)
		{
			num |= data[i + 1];
			num |= data[i + 1] << 8;
			num |= data[i + 1] << 16;
			num |= data[i + 1] << 24;
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		Hash160 hash = obj as Hash160;
		if (hash == null)
		{
			return false;
		}
		if (data == hash.data)
		{
			return true;
		}
		if (data == null || hash.data == null)
		{
			return false;
		}
		if (data.Length != hash.data.Length)
		{
			return false;
		}
		for (int i = 0; i < data.Length; i++)
		{
			if (data[i] != hash.data[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool operator ==(Hash160 lhs, Hash160 rhs)
	{
		if ((object)lhs == null)
		{
			if ((object)rhs == null)
			{
				return true;
			}
			return false;
		}
		return lhs.Equals(rhs);
	}

	public static bool operator !=(Hash160 lhs, Hash160 rhs)
	{
		return !(lhs == rhs);
	}
}

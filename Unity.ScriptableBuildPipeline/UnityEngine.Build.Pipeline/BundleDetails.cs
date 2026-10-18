using System;

namespace UnityEngine.Build.Pipeline;

[Serializable]
public struct BundleDetails : IEquatable<BundleDetails>
{
	[SerializeField]
	private string m_FileName;

	[SerializeField]
	private uint m_Crc;

	[SerializeField]
	private string m_Hash;

	[SerializeField]
	private string[] m_Dependencies;

	public string FileName
	{
		get
		{
			return m_FileName;
		}
		set
		{
			m_FileName = value;
		}
	}

	public uint Crc
	{
		get
		{
			return m_Crc;
		}
		set
		{
			m_Crc = value;
		}
	}

	public Hash128 Hash
	{
		get
		{
			return Hash128.Parse(m_Hash);
		}
		set
		{
			m_Hash = value.ToString();
		}
	}

	public string[] Dependencies
	{
		get
		{
			return m_Dependencies;
		}
		set
		{
			m_Dependencies = value;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is BundleDetails)
		{
			return Equals((BundleDetails)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(((((uint)(((FileName != null) ? FileName.GetHashCode() : 0) * 397) ^ Crc) * 397) ^ (uint)Hash.GetHashCode()) * 397) ^ ((Dependencies != null) ? Dependencies.GetHashCode() : 0);
	}

	public static bool operator ==(BundleDetails a, BundleDetails b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(BundleDetails a, BundleDetails b)
	{
		return !(a == b);
	}

	public bool Equals(BundleDetails other)
	{
		if (string.Equals(FileName, other.FileName) && Crc == other.Crc && Hash.Equals(other.Hash))
		{
			return object.Equals(Dependencies, other.Dependencies);
		}
		return false;
	}
}

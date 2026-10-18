using System;
using UnityEngine;

namespace Unity.XR.CoreUtils;

[Serializable]
public struct SerializableGuid : IEquatable<SerializableGuid>
{
	private static readonly SerializableGuid k_Empty = new SerializableGuid(0uL, 0uL);

	[SerializeField]
	[HideInInspector]
	private ulong m_GuidLow;

	[SerializeField]
	[HideInInspector]
	private ulong m_GuidHigh;

	public static SerializableGuid Empty => k_Empty;

	public Guid Guid => GuidUtil.Compose(m_GuidLow, m_GuidHigh);

	public SerializableGuid(ulong guidLow, ulong guidHigh)
	{
		m_GuidLow = guidLow;
		m_GuidHigh = guidHigh;
	}

	public override int GetHashCode()
	{
		return m_GuidLow.GetHashCode() * 486187739 + m_GuidHigh.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SerializableGuid other))
		{
			return false;
		}
		return Equals(other);
	}

	public override string ToString()
	{
		return Guid.ToString();
	}

	public string ToString(string format)
	{
		return Guid.ToString(format);
	}

	public string ToString(string format, IFormatProvider provider)
	{
		return Guid.ToString(format, provider);
	}

	public bool Equals(SerializableGuid other)
	{
		if (m_GuidLow == other.m_GuidLow)
		{
			return m_GuidHigh == other.m_GuidHigh;
		}
		return false;
	}

	public static bool operator ==(SerializableGuid lhs, SerializableGuid rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(SerializableGuid lhs, SerializableGuid rhs)
	{
		return !lhs.Equals(rhs);
	}
}

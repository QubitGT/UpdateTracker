using System;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands;

internal readonly struct HandExpressionName : IEquatable<HandExpressionName>
{
	public static readonly HandExpressionName Default = new HandExpressionName("Default");

	private readonly InternedString m_InternedString;

	public HandExpressionName(string value)
	{
		m_InternedString = new InternedString(value);
	}

	public override bool Equals(object obj)
	{
		if (obj is HandExpressionName other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(HandExpressionName other)
	{
		return m_InternedString.Equals(other.m_InternedString);
	}

	public override string ToString()
	{
		return m_InternedString.ToString();
	}

	public override int GetHashCode()
	{
		return m_InternedString.GetHashCode();
	}

	public static bool operator ==(HandExpressionName lhs, HandExpressionName rhs)
	{
		return lhs.m_InternedString == rhs.m_InternedString;
	}

	public static bool operator !=(HandExpressionName lhs, HandExpressionName rhs)
	{
		return lhs.m_InternedString != rhs.m_InternedString;
	}

	public static implicit operator string(HandExpressionName value)
	{
		return value.m_InternedString.ToString();
	}

	public static implicit operator HandExpressionName(string value)
	{
		return new HandExpressionName(value);
	}
}

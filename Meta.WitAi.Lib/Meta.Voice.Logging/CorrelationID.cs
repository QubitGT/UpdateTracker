using System;

namespace Meta.Voice.Logging;

public readonly struct CorrelationID
{
	private string Value { get; }

	public bool IsAssigned => Value != null;

	private CorrelationID(string value)
	{
		Value = value;
	}

	public override string ToString()
	{
		return Value;
	}

	public static implicit operator string(CorrelationID correlationId)
	{
		return correlationId.Value;
	}

	public static explicit operator CorrelationID(string value)
	{
		return new CorrelationID(value);
	}

	public static implicit operator CorrelationID(Guid value)
	{
		return new CorrelationID(value.ToString());
	}

	public override bool Equals(object obj)
	{
		if (obj is CorrelationID correlationID)
		{
			return Value == correlationID.Value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}

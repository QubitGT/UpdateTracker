using System;

namespace Meta.Voice.TelemetryUtilities;

public readonly struct OperationID
{
	public static readonly OperationID INVALID;

	private string Value { get; }

	public bool IsAssigned => Value != null;

	public OperationID(string value)
	{
		if (value == null)
		{
			value = Guid.NewGuid().ToString();
		}
		Value = value;
	}

	public static OperationID Create(string value = null)
	{
		return new OperationID(value);
	}

	public override string ToString()
	{
		return Value;
	}

	public static implicit operator string(OperationID correlationId)
	{
		return correlationId.Value;
	}

	public static explicit operator OperationID(string value)
	{
		return new OperationID(value);
	}

	public static implicit operator OperationID(Guid value)
	{
		return new OperationID(value.ToString());
	}

	public override bool Equals(object obj)
	{
		if (obj is OperationID operationID)
		{
			return Value == operationID.Value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (IsAssigned)
		{
			return Value.GetHashCode();
		}
		return 0;
	}
}

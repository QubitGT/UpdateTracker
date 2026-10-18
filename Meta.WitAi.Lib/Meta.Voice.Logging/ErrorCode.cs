namespace Meta.Voice.Logging;

public readonly struct ErrorCode
{
	private string Value { get; }

	private ErrorCode(string value)
	{
		Value = value;
	}

	public override string ToString()
	{
		return Value;
	}

	public static implicit operator string(ErrorCode errorCode)
	{
		return errorCode.Value;
	}

	public static explicit operator ErrorCode(string value)
	{
		return new ErrorCode(value);
	}

	public static implicit operator ErrorCode(KnownErrorCode value)
	{
		return new ErrorCode(value.ToString());
	}

	public override bool Equals(object obj)
	{
		if (obj is ErrorCode errorCode)
		{
			return Value == errorCode.Value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}

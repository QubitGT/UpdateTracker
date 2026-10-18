using System;
using Modio.Errors;

namespace Modio;

public class Error : IEquatable<Error>
{
	public static readonly Error None = new Error(ErrorCode.NONE);

	public static readonly Error Unknown = new Error(ErrorCode.UNKNOWN);

	public readonly ErrorCode Code;

	public readonly string CustomMessage;

	public bool IsSilent
	{
		get
		{
			ErrorCode code = Code;
			return code == ErrorCode.SHUTTING_DOWN || code == ErrorCode.OPERATION_CANCELLED;
		}
	}

	public Error(ErrorCode code)
	{
		Code = code;
	}

	public Error(ErrorCode code, string customMessage)
	{
		Code = code;
		CustomMessage = customMessage;
	}

	public virtual string GetMessage()
	{
		string customMessage = CustomMessage;
		if (customMessage == null || customMessage.Length <= 0)
		{
			return Code.GetMessage();
		}
		return CustomMessage;
	}

	public static implicit operator bool(Error error)
	{
		return error.Code != ErrorCode.NONE;
	}

	public static explicit operator Error(ErrorCode errorCode)
	{
		if (errorCode != ErrorCode.NONE)
		{
			return new Error(errorCode);
		}
		return None;
	}

	public override string ToString()
	{
		if (Code != ErrorCode.NONE)
		{
			return GetMessage();
		}
		return "Success";
	}

	public bool Equals(Error other)
	{
		return Code == other.Code;
	}

	public override bool Equals(object obj)
	{
		if (this != obj)
		{
			if (obj is Error other)
			{
				return Equals(other);
			}
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return Code.GetHashCode();
	}
}

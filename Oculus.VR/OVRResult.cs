using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

internal static class OVRResult
{
	public static OVRResult<TStatus> From<TStatus>(TStatus status) where TStatus : struct, Enum, IConvertible
	{
		return OVRResult<TStatus>.From(status);
	}

	public static OVRResult<TResult, TStatus> From<TResult, TStatus>(TResult result, TStatus status) where TStatus : struct, Enum, IConvertible
	{
		return OVRResult<TResult, TStatus>.From(result, status);
	}
}
public struct OVRResult<TStatus> : IEquatable<OVRResult<TStatus>> where TStatus : struct, Enum, IConvertible
{
	private readonly bool _initialized;

	private readonly int _statusCode;

	private readonly TStatus _status;

	public bool Success
	{
		get
		{
			if (_initialized)
			{
				return ((OVRPlugin.Result)_statusCode).IsSuccess();
			}
			return false;
		}
	}

	public TStatus Status
	{
		get
		{
			if (_initialized)
			{
				return _status;
			}
			OVRPlugin.Result from = OVRPlugin.Result.Failure_DataIsInvalid;
			return UnsafeUtility.As<OVRPlugin.Result, TStatus>(ref from);
		}
	}

	private OVRResult(TStatus status)
	{
		if (UnsafeUtility.SizeOf<TStatus>() != 4)
		{
			throw new InvalidOperationException("TStatus must have a 4 byte underlying storage type.");
		}
		_initialized = true;
		_status = status;
		_statusCode = UnsafeUtility.EnumToInt(_status);
	}

	public static OVRResult<TStatus> From(TStatus status)
	{
		return new OVRResult<TStatus>(status);
	}

	public static OVRResult<TStatus> FromSuccess(TStatus status)
	{
		if (!UnsafeUtility.As<TStatus, OVRPlugin.Result>(ref status).IsSuccess())
		{
			throw new ArgumentException("Not of a valid success status", "status");
		}
		return new OVRResult<TStatus>(status);
	}

	public static OVRResult<TStatus> FromFailure(TStatus status)
	{
		if (UnsafeUtility.As<TStatus, OVRPlugin.Result>(ref status).IsSuccess())
		{
			throw new ArgumentException("Not of a valid failure status", "status");
		}
		return new OVRResult<TStatus>(status);
	}

	public bool Equals(OVRResult<TStatus> other)
	{
		if (_initialized == other._initialized)
		{
			return _statusCode == other._statusCode;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRResult<TStatus> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (17 * 31 + _initialized.GetHashCode()) * 31 + _statusCode.GetHashCode();
	}

	public override string ToString()
	{
		if (!_initialized)
		{
			return "(invalid result)";
		}
		return _status.ToString();
	}

	public static implicit operator bool(OVRResult<TStatus> value)
	{
		return value.Success;
	}

	public static implicit operator OVRResult<TStatus>(OVRPlugin.Result result)
	{
		return From(UnsafeUtility.As<OVRPlugin.Result, TStatus>(ref result));
	}
}
public struct OVRResult<TValue, TStatus> : IEquatable<OVRResult<TValue, TStatus>> where TStatus : struct, Enum, IConvertible
{
	private readonly bool _initialized;

	private readonly TValue _value;

	private readonly int _statusCode;

	private readonly TStatus _status;

	public bool Success
	{
		get
		{
			if (_initialized)
			{
				return ((OVRPlugin.Result)_statusCode).IsSuccess();
			}
			return false;
		}
	}

	public TStatus Status
	{
		get
		{
			if (_initialized)
			{
				return _status;
			}
			OVRPlugin.Result from = OVRPlugin.Result.Failure_DataIsInvalid;
			return UnsafeUtility.As<OVRPlugin.Result, TStatus>(ref from);
		}
	}

	public bool HasValue => Success;

	public TValue Value
	{
		get
		{
			if (!_initialized)
			{
				throw new InvalidOperationException("The OVRResult object is not valid.");
			}
			if (_statusCode < 0)
			{
				throw new InvalidOperationException("The OVRResult does not have a value because the " + $"operation failed with {_status}.");
			}
			return _value;
		}
	}

	public bool TryGetValue(out TValue value)
	{
		if (HasValue)
		{
			value = _value;
			return true;
		}
		value = default(TValue);
		return false;
	}

	private OVRResult(TValue value, TStatus status)
	{
		if (UnsafeUtility.SizeOf<TStatus>() != 4)
		{
			throw new InvalidOperationException("TStatus must have a 4 byte underlying storage type.");
		}
		_initialized = true;
		_value = value;
		_status = status;
		_statusCode = UnsafeUtility.EnumToInt(_status);
	}

	public static OVRResult<TValue, TStatus> From(TValue value, TStatus status)
	{
		return new OVRResult<TValue, TStatus>(value, status);
	}

	public static OVRResult<TValue, TStatus> FromSuccess(TValue value, TStatus status)
	{
		if (!UnsafeUtility.As<TStatus, OVRPlugin.Result>(ref status).IsSuccess())
		{
			throw new ArgumentException("Not of a valid success status. Success values must have an integral value >= 0.", "status");
		}
		return new OVRResult<TValue, TStatus>(value, status);
	}

	public static OVRResult<TValue, TStatus> FromFailure(TStatus status)
	{
		if (UnsafeUtility.As<TStatus, OVRPlugin.Result>(ref status).IsSuccess())
		{
			throw new ArgumentException("Not of a valid failure status. Failure values must have an integral value < 0.", "status");
		}
		return new OVRResult<TValue, TStatus>(default(TValue), status);
	}

	public bool Equals(OVRResult<TValue, TStatus> other)
	{
		if (_initialized == other._initialized && EqualityComparer<TValue>.Default.Equals(_value, other._value))
		{
			return _statusCode == other._statusCode;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRResult<TValue, TStatus> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = ((17 * 31 + _initialized.GetHashCode()) * 31 + _statusCode.GetHashCode()) * 31;
		TValue value = _value;
		return num + ((value != null) ? value.GetHashCode() : 0);
	}

	public override string ToString()
	{
		if (!_initialized)
		{
			return "(invalid result)";
		}
		if (!HasValue)
		{
			return _status.ToString();
		}
		return $"(Value={_value}, Status={_status})";
	}

	public static implicit operator bool(OVRResult<TValue, TStatus> value)
	{
		return value.Success;
	}

	public static implicit operator OVRResult<TValue, TStatus>(OVRPlugin.Result result)
	{
		return FromFailure(UnsafeUtility.As<OVRPlugin.Result, TStatus>(ref result));
	}
}

using System;

namespace Fusion;

public sealed class SessionProperty
{
	private object _value = null;

	public object PropertyValue => _value;

	public Type PropertyType => _value.GetType();

	public bool IsInt => _value is int;

	public bool IsString => _value is string;

	public bool Isbool => _value is bool;

	private SessionProperty()
	{
	}

	public static implicit operator int(SessionProperty sessionProperty)
	{
		if (sessionProperty._value is int result)
		{
			return result;
		}
		throw new InvalidCastException();
	}

	public static implicit operator SessionProperty(int v)
	{
		return new SessionProperty
		{
			_value = v
		};
	}

	public static implicit operator string(SessionProperty sessionProperty)
	{
		if (sessionProperty._value is string result)
		{
			return result;
		}
		throw new InvalidCastException();
	}

	public static implicit operator SessionProperty(string v)
	{
		return new SessionProperty
		{
			_value = v
		};
	}

	public static implicit operator bool(SessionProperty sessionProperty)
	{
		if (sessionProperty._value is bool result)
		{
			return result;
		}
		throw new InvalidCastException();
	}

	public static implicit operator SessionProperty(bool v)
	{
		return new SessionProperty
		{
			_value = v
		};
	}

	public static bool Support(object obj)
	{
		return obj is int || obj is string || obj is bool;
	}

	public static SessionProperty Convert(object obj)
	{
		if (obj is int num)
		{
			return num;
		}
		if (obj is string text)
		{
			return text;
		}
		if (obj is bool flag)
		{
			return flag;
		}
		throw new ArgumentException("Invalid Object type, not supported");
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}, Type={2}]", "SessionProperty", _value, PropertyType);
	}
}

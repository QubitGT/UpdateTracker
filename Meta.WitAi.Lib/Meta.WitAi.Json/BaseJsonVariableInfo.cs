using System;
using System.Collections.Generic;
using System.Reflection;

namespace Meta.WitAi.Json;

internal abstract class BaseJsonVariableInfo<T> : IJsonVariableInfo where T : MemberInfo
{
	protected T _info;

	protected BaseJsonVariableInfo(T info)
	{
		_info = info;
	}

	protected virtual string GetName()
	{
		return _info.Name;
	}

	protected virtual bool IsDefined<TAttribute>() where TAttribute : Attribute
	{
		return _info.IsDefined(typeof(TAttribute), inherit: false);
	}

	protected virtual IEnumerable<TAttribute> GetCustomAttributes<TAttribute>() where TAttribute : Attribute
	{
		return _info.GetCustomAttributes<TAttribute>(inherit: false);
	}

	public virtual string[] GetSerializeNames()
	{
		if (!IsDefined<JsonPropertyAttribute>())
		{
			return new string[1] { GetName() };
		}
		List<string> list = new List<string>();
		foreach (JsonPropertyAttribute customAttribute in GetCustomAttributes<JsonPropertyAttribute>())
		{
			string text = customAttribute.PropertyName;
			if (string.IsNullOrEmpty(text))
			{
				text = GetName();
			}
			if (!list.Contains(text))
			{
				list.Add(text);
			}
		}
		return list.ToArray();
	}

	public virtual bool GetShouldSerialize()
	{
		if (IsDefined<JsonIgnoreAttribute>() || IsDefined<NonSerializedAttribute>())
		{
			return false;
		}
		if (!HasGet())
		{
			return false;
		}
		if (!IsGetPublic())
		{
			return IsDefined<JsonPropertyAttribute>();
		}
		return true;
	}

	protected abstract bool HasGet();

	protected abstract bool IsGetPublic();

	public virtual bool GetShouldDeserialize()
	{
		if (IsDefined<JsonIgnoreAttribute>() || IsDefined<NonSerializedAttribute>())
		{
			return false;
		}
		if (!HasSet())
		{
			return false;
		}
		if (!IsSetPublic())
		{
			return IsDefined<JsonPropertyAttribute>();
		}
		return true;
	}

	protected abstract bool HasSet();

	protected abstract bool IsSetPublic();

	public abstract Type GetVariableType();

	public abstract object GetValue(object obj);

	public abstract void SetValue(object obj, object newValue);
}

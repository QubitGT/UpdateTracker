using System;
using System.Reflection;

namespace Meta.WitAi.Json;

internal class JsonPropertyInfo : BaseJsonVariableInfo<PropertyInfo>
{
	public JsonPropertyInfo(PropertyInfo info)
		: base(info)
	{
	}

	public override Type GetVariableType()
	{
		return _info.PropertyType;
	}

	protected override bool HasGet()
	{
		return _info.GetMethod != null;
	}

	protected override bool IsGetPublic()
	{
		return _info.GetMethod.IsPublic;
	}

	public override object GetValue(object obj)
	{
		return _info.GetValue(obj);
	}

	protected override bool HasSet()
	{
		return _info.SetMethod != null;
	}

	protected override bool IsSetPublic()
	{
		return _info.SetMethod.IsPublic;
	}

	public override void SetValue(object obj, object value)
	{
		_info.SetValue(obj, value);
	}
}

using System;
using System.Reflection;

namespace Meta.WitAi.Json;

internal class JsonFieldInfo : BaseJsonVariableInfo<FieldInfo>
{
	public JsonFieldInfo(FieldInfo info)
		: base(info)
	{
	}

	public override Type GetVariableType()
	{
		return _info.FieldType;
	}

	protected override bool HasGet()
	{
		return true;
	}

	protected override bool IsGetPublic()
	{
		return _info.IsPublic;
	}

	public override object GetValue(object obj)
	{
		return _info.GetValue(obj);
	}

	protected override bool HasSet()
	{
		return true;
	}

	protected override bool IsSetPublic()
	{
		return _info.IsPublic;
	}

	public override void SetValue(object obj, object value)
	{
		_info.SetValue(obj, value);
	}
}

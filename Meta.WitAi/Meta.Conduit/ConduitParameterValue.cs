using System;
using UnityEngine.Scripting;

namespace Meta.Conduit;

public struct ConduitParameterValue
{
	public readonly object Value;

	public Type DataType;

	[Preserve]
	public ConduitParameterValue(object value)
	{
		Value = value;
		DataType = value.GetType();
	}

	[Preserve]
	public ConduitParameterValue(object value, Type dataType)
	{
		Value = value;
		DataType = dataType;
	}
}

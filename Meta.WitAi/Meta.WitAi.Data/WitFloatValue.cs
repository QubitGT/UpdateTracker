using System;
using Meta.WitAi.Json;
using UnityEngine;

namespace Meta.WitAi.Data;

public class WitFloatValue : WitValue
{
	[SerializeField]
	public float equalityTolerance = 0.0001f;

	public override object GetValue(WitResponseNode response)
	{
		return GetFloatValue(response);
	}

	public override bool Equals(WitResponseNode response, object value)
	{
		float result = 0f;
		if (value is float num)
		{
			result = num;
		}
		else if (value != null && !float.TryParse(value?.ToString() ?? "", out result))
		{
			return false;
		}
		return Math.Abs(GetFloatValue(response) - result) < equalityTolerance;
	}

	public float GetFloatValue(WitResponseNode response)
	{
		return base.Reference.GetFloatValue(response);
	}
}

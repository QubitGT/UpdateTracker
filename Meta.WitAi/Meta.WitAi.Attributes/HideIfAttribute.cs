using System;
using UnityEngine;

namespace Meta.WitAi.Attributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class HideIfAttribute : PropertyAttribute
{
	public string conditionFieldName;

	public HideIfAttribute(string conditionFieldName)
	{
		this.conditionFieldName = conditionFieldName;
	}
}

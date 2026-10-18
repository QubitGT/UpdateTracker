using System;
using UnityEngine;

namespace Meta.WitAi.Attributes;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class ShowIfAttribute : PropertyAttribute
{
	public string conditionFieldName;

	public ShowIfAttribute(string conditionFieldName)
	{
		this.conditionFieldName = conditionFieldName;
	}
}

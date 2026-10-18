using System;
using UnityEngine;

namespace Meta.WitAi.Data.ValueReferences;

[Serializable]
public class StringReference<T> : IStringReference where T : ScriptableObject, IStringReference
{
	[SerializeField]
	private string stringValue;

	[SerializeField]
	private T stringObject;

	public string Value
	{
		get
		{
			if (!stringObject)
			{
				return stringValue;
			}
			return stringObject.Value;
		}
		set
		{
			stringObject = null;
			stringValue = value;
		}
	}
}

using System;
using UnityEngine;

namespace Meta.WitAi.CallbackHandlers;

[Serializable]
public class FormattedValueEvents
{
	[Tooltip("Modify the string output, values can be inserted with {value} or {0}, {1}, {2}")]
	public string format;

	public ValueEvent onFormattedValueEvent = new ValueEvent();
}

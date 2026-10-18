using System;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi.Composer;

[Serializable]
public class ContextEvents
{
	[Header("Context Map Changed Events")]
	public UnityEvent<string, string, string> OnContextMapValueChanged;

	public UnityEvent<string> OnContextMapValueRemoved;
}

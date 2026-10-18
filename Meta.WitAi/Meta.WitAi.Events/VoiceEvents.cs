using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.Events;

[Serializable]
public class VoiceEvents : SpeechEvents
{
	private const string EVENT_CATEGORY_DATA_EVENTS = "Data Events";

	[EventCategory("Data Events")]
	[FormerlySerializedAs("OnByteDataReady")]
	[SerializeField]
	[HideInInspector]
	private WitByteDataEvent _onByteDataReady = new WitByteDataEvent();

	[EventCategory("Data Events")]
	[FormerlySerializedAs("OnByteDataSent")]
	[SerializeField]
	[HideInInspector]
	private WitByteDataEvent _onByteDataSent = new WitByteDataEvent();

	[EventCategory("Activation Response Events")]
	[Tooltip("Called after an on partial response to validate data.  If data.validResponse is true, service will deactivate & use the partial data as final")]
	[FormerlySerializedAs("OnValidatePartialResponse")]
	[SerializeField]
	private WitValidationEvent _onValidatePartialResponse = new WitValidationEvent();

	public WitByteDataEvent OnByteDataReady => _onByteDataReady;

	public WitByteDataEvent OnByteDataSent => _onByteDataSent;

	public WitValidationEvent OnValidatePartialResponse => _onValidatePartialResponse;
}

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.Voice;

[Serializable]
public class NLPRequestEvents<TUnityEvent, TResponseData> : TranscriptionRequestEvents<TUnityEvent> where TUnityEvent : UnityEventBase
{
	[Header("NLP Events")]
	[Tooltip("Called on every request response text.")]
	[SerializeField]
	private TranscriptionRequestEvent _onRawResponse = Activator.CreateInstance<TranscriptionRequestEvent>();

	[Tooltip("Called for partially decoded request responses.")]
	[SerializeField]
	private NLPRequestResponseEvent<TResponseData> _onPartialResponse = Activator.CreateInstance<NLPRequestResponseEvent<TResponseData>>();

	[Tooltip("Called on request language processing once completely analyzed.")]
	[SerializeField]
	private NLPRequestResponseEvent<TResponseData> _onFullResponse = Activator.CreateInstance<NLPRequestResponseEvent<TResponseData>>();

	[Tooltip("Called by request to allow custom validation prior to error determination.")]
	[SerializeField]
	private NLPRequestResponseValidatorEvent<TResponseData> _onValidateResponse = Activator.CreateInstance<NLPRequestResponseValidatorEvent<TResponseData>>();

	public TranscriptionRequestEvent OnRawResponse => _onRawResponse;

	public NLPRequestResponseEvent<TResponseData> OnPartialResponse => _onPartialResponse;

	public NLPRequestResponseEvent<TResponseData> OnFullResponse => _onFullResponse;

	public NLPRequestResponseValidatorEvent<TResponseData> OnValidateResponse => _onValidateResponse;
}

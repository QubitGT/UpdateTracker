using System;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.Voice;

[Serializable]
public class VoiceRequestEvents<TUnityEvent> where TUnityEvent : UnityEventBase
{
	[Header("State Events")]
	[Tooltip("Called whenever a request state changes.")]
	[SerializeField]
	private TUnityEvent _onStateChange = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called on initial request generation.")]
	[SerializeField]
	private TUnityEvent _onInit = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called following the start of data transmission.")]
	[SerializeField]
	private TUnityEvent _onSend = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called following the cancellation of a request.")]
	[SerializeField]
	private TUnityEvent _onCancel = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called following an error response from a request.")]
	[SerializeField]
	private TUnityEvent _onFailed = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called following a successful request & data parse with results provided.")]
	[SerializeField]
	private TUnityEvent _onSuccess = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called following cancellation, failure or success to finalize request.")]
	[SerializeField]
	private TUnityEvent _onComplete = Activator.CreateInstance<TUnityEvent>();

	[Header("Progress Events")]
	[Tooltip("Called on download progress update.")]
	[SerializeField]
	private TUnityEvent _onDownloadProgressChange = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called on upload progress update.")]
	[SerializeField]
	private TUnityEvent _onUploadProgressChange = Activator.CreateInstance<TUnityEvent>();

	public TUnityEvent OnStateChange => _onStateChange;

	public TUnityEvent OnInit => _onInit;

	public TUnityEvent OnSend => _onSend;

	public TUnityEvent OnCancel => _onCancel;

	public TUnityEvent OnFailed => _onFailed;

	public TUnityEvent OnSuccess => _onSuccess;

	public TUnityEvent OnComplete => _onComplete;

	public TUnityEvent OnDownloadProgressChange => _onDownloadProgressChange;

	public TUnityEvent OnUploadProgressChange => _onUploadProgressChange;
}

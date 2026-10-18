using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;

namespace Meta.WitAi.TTS.UX;

public class TTSSpeakerObserver : MonoBehaviour
{
	[Header("Speaker Settings")]
	[SerializeField]
	[Tooltip("TTSSpeaker being observed, if left empty it will grab the speaker from the GameObject")]
	private TTSSpeaker _speaker;

	public TTSSpeaker Speaker => _speaker;

	protected virtual void Awake()
	{
		if (_speaker == null)
		{
			_speaker = base.gameObject.GetComponentInChildren<TTSSpeaker>();
		}
	}

	protected virtual void OnEnable()
	{
		if (!(_speaker == null))
		{
			_speaker.Events.OnPlaybackQueueBegin.AddListener(OnPlaybackQueueBegin);
			_speaker.Events.OnPlaybackQueueComplete.AddListener(OnPlaybackQueueComplete);
			_speaker.Events.OnLoadBegin.AddListener(OnLoadBegin);
			_speaker.Events.OnLoadAbort.AddListener(OnLoadAbort);
			_speaker.Events.OnLoadFailed.AddListener(OnLoadFailed);
			_speaker.Events.OnLoadSuccess.AddListener(OnLoadSuccess);
			_speaker.Events.OnPlaybackReady.AddListener(OnPlaybackReady);
			_speaker.Events.OnPlaybackStart.AddListener(OnPlaybackStart);
			_speaker.Events.OnPlaybackCancelled.AddListener(OnPlaybackCancelled);
			_speaker.Events.OnPlaybackComplete.AddListener(OnPlaybackComplete);
		}
	}

	protected virtual void OnDisable()
	{
		if (!(_speaker == null))
		{
			_speaker.Events.OnPlaybackQueueBegin.RemoveListener(OnPlaybackQueueBegin);
			_speaker.Events.OnPlaybackQueueComplete.RemoveListener(OnPlaybackQueueComplete);
			_speaker.Events.OnLoadBegin.RemoveListener(OnLoadBegin);
			_speaker.Events.OnLoadAbort.RemoveListener(OnLoadAbort);
			_speaker.Events.OnLoadFailed.RemoveListener(OnLoadFailed);
			_speaker.Events.OnLoadSuccess.RemoveListener(OnLoadSuccess);
			_speaker.Events.OnPlaybackReady.RemoveListener(OnPlaybackReady);
			_speaker.Events.OnPlaybackStart.RemoveListener(OnPlaybackStart);
			_speaker.Events.OnPlaybackCancelled.RemoveListener(OnPlaybackCancelled);
			_speaker.Events.OnPlaybackComplete.RemoveListener(OnPlaybackComplete);
		}
	}

	protected virtual void OnPlaybackQueueBegin()
	{
	}

	protected virtual void OnPlaybackQueueComplete()
	{
	}

	protected virtual void OnLoadBegin(TTSSpeaker speaker, TTSClipData clipData)
	{
	}

	protected virtual void OnLoadAbort(TTSSpeaker speaker, TTSClipData clipData)
	{
	}

	protected virtual void OnLoadFailed(TTSSpeaker speaker, TTSClipData clipData, string error)
	{
	}

	protected virtual void OnLoadSuccess(TTSSpeaker speaker, TTSClipData clipData)
	{
	}

	protected virtual void OnPlaybackReady(TTSSpeaker speaker, TTSClipData clipData)
	{
	}

	protected virtual void OnPlaybackStart(TTSSpeaker speaker, TTSClipData clipData)
	{
	}

	protected virtual void OnPlaybackCancelled(TTSSpeaker speaker, TTSClipData clipData, string reason)
	{
	}

	protected virtual void OnPlaybackComplete(TTSSpeaker speaker, TTSClipData clipData)
	{
	}
}

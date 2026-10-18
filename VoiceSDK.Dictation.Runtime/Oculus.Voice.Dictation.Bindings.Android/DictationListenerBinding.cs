using Meta.WitAi;
using Meta.WitAi.Dictation;
using Meta.WitAi.Dictation.Events;
using UnityEngine;

namespace Oculus.Voice.Dictation.Bindings.Android;

public class DictationListenerBinding : AndroidJavaProxy
{
	private IDictationService _dictationService;

	private IServiceEvents _serviceEvents;

	private DictationEvents DictationEvents => _dictationService.DictationEvents;

	public DictationListenerBinding(IDictationService dictationService, IServiceEvents serviceEvents)
		: base("com.oculus.assistant.api.voicesdk.dictation.PlatformDictationListener")
	{
		_dictationService = dictationService;
		_serviceEvents = serviceEvents;
	}

	public void onStart(string sessionId)
	{
		DictationEvents.OnStartListening?.Invoke();
		new PlatformDictationSession
		{
			dictationService = _dictationService,
			platformSessionId = sessionId
		};
	}

	public void onMicAudioLevel(string sessionId, int micLevel)
	{
		DictationEvents.OnMicAudioLevelChanged?.Invoke((float)micLevel / 100f);
	}

	public void onPartialTranscription(string sessionId, string transcription)
	{
		DictationEvents.OnPartialTranscription?.Invoke(transcription);
	}

	public void onFinalTranscription(string sessionId, string transcription)
	{
		DictationEvents.OnFullTranscription?.Invoke(transcription);
	}

	public void onError(string sessionId, string errorType, string errorMessage)
	{
		DictationEvents.OnError?.Invoke(errorType, errorMessage);
	}

	public void onStopped(string sessionId)
	{
		DictationEvents.OnStoppedListening?.Invoke();
		new PlatformDictationSession
		{
			dictationService = _dictationService,
			platformSessionId = sessionId
		};
	}

	public void onServiceNotAvailable(string error, string message)
	{
		VLog.W("Platform dictation service is not available");
		_serviceEvents.OnServiceNotAvailable(error, message);
	}
}

using System.Collections.Concurrent;
using Meta.Voice;
using Meta.WitAi;
using Meta.WitAi.Requests;
using Meta.WitAi.TTS;
using Oculus.Voice;
using UnityEngine;

namespace Oculus.VoiceSDK.Utilities;

public class VoiceErrorSimulator : MonoBehaviour
{
	public VoiceService[] voiceServices;

	public TTSService ttsService;

	private ConcurrentDictionary<VoiceErrorRequestType, VoiceErrorSimulationType> _requests = new ConcurrentDictionary<VoiceErrorRequestType, VoiceErrorSimulationType>();

	protected virtual void OnEnable()
	{
		RefreshServices();
		SetListeners(add: true);
	}

	protected virtual void RefreshServices()
	{
		if (voiceServices == null)
		{
			VoiceService[] componentsInChildren = base.gameObject.GetComponentsInChildren<AppVoiceExperience>(includeInactive: true);
			voiceServices = componentsInChildren;
		}
		if (ttsService == null)
		{
			ttsService = base.gameObject.GetComponentInChildren<TTSService>(includeInactive: true);
		}
	}

	private void SetListeners(bool add)
	{
		if (voiceServices != null)
		{
			VoiceService[] array = voiceServices;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].VoiceEvents.OnRequestInitialized.SetListener(SimulateVoiceRequestError, add);
			}
		}
	}

	protected virtual void OnDisable()
	{
		SetListeners(add: false);
	}

	public void SimulateError(VoiceErrorRequestType requestType, VoiceErrorSimulationType simulationType)
	{
		if (requestType == VoiceErrorRequestType.TextToSpeechRequest)
		{
			ttsService.SimulatedErrorType = simulationType;
		}
		else
		{
			_requests[requestType] = simulationType;
		}
	}

	private void SimulateVoiceRequestError(VoiceServiceRequest request)
	{
		if (request.IsLocalRequest)
		{
			VoiceErrorRequestType voiceErrorRequestType = (VoiceErrorRequestType)(-1);
			VoiceErrorSimulationType value = (VoiceErrorSimulationType)(-1);
			if (request.InputType == NLPRequestInputType.Audio && _requests.ContainsKey(VoiceErrorRequestType.AudioInputAnalysisRequest))
			{
				voiceErrorRequestType = VoiceErrorRequestType.AudioInputAnalysisRequest;
				_requests.TryRemove(voiceErrorRequestType, out value);
			}
			else if (request.InputType == NLPRequestInputType.Text && _requests.ContainsKey(VoiceErrorRequestType.TextInputAnalysisRequest))
			{
				voiceErrorRequestType = VoiceErrorRequestType.TextInputAnalysisRequest;
				_requests.TryRemove(voiceErrorRequestType, out value);
			}
			if (voiceErrorRequestType != (VoiceErrorRequestType)(-1) && value != (VoiceErrorSimulationType)(-1))
			{
				request.SimulateError(value);
			}
		}
	}
}

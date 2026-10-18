using Meta.WitAi.Data;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.CallbackHandlers;

public abstract class WitResponseHandler : MonoBehaviour
{
	[FormerlySerializedAs("wit")]
	[SerializeField]
	public VoiceService Voice;

	[SerializeField]
	public bool ValidateEarly;

	private bool _validated;

	private void OnValidate()
	{
		if (!Voice)
		{
			Voice = Object.FindAnyObjectByType<VoiceService>();
		}
	}

	protected virtual void OnEnable()
	{
		if (!Voice)
		{
			Voice = Object.FindAnyObjectByType<VoiceService>();
		}
		if (!Voice)
		{
			VLog.E("VoiceService not found in scene.\nDisabling " + GetType().Name + " on " + base.gameObject.name);
			base.enabled = false;
		}
		else
		{
			Voice.VoiceEvents.OnSend.AddListener(OnRequestSend);
			Voice.VoiceEvents.OnValidatePartialResponse.AddListener(HandleValidateEarlyResponse);
			Voice.VoiceEvents.OnResponse.AddListener(HandleFinalResponse);
		}
	}

	protected virtual void OnDisable()
	{
		if ((bool)Voice)
		{
			Voice.VoiceEvents.OnSend.RemoveListener(OnRequestSend);
			Voice.VoiceEvents.OnValidatePartialResponse.RemoveListener(HandleValidateEarlyResponse);
			Voice.VoiceEvents.OnResponse.RemoveListener(HandleFinalResponse);
		}
	}

	protected virtual void OnRequestSend(VoiceServiceRequest request)
	{
		_validated = false;
	}

	protected virtual void HandleValidateEarlyResponse(VoiceSession session)
	{
		if (ValidateEarly && !_validated && string.IsNullOrEmpty(OnValidateResponse(session.response, isEarlyResponse: true)))
		{
			_validated = true;
			OnResponseSuccess(session.response);
			session.validResponse = true;
		}
	}

	protected virtual void HandleFinalResponse(WitResponseNode response)
	{
		if (!_validated)
		{
			string text = OnValidateResponse(response, isEarlyResponse: false);
			if (!string.IsNullOrEmpty(text))
			{
				OnResponseInvalid(response, text);
			}
			else
			{
				OnResponseSuccess(response);
			}
			_validated = true;
		}
	}

	protected abstract string OnValidateResponse(WitResponseNode response, bool isEarlyResponse);

	protected abstract void OnResponseInvalid(WitResponseNode response, string error);

	protected abstract void OnResponseSuccess(WitResponseNode response);

	public static bool RefreshConfidenceRange(float confidence, ConfidenceRange[] confidenceRanges, bool allowConfidenceOverlap)
	{
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		while (confidenceRanges != null && num < confidenceRanges.Length)
		{
			ConfidenceRange confidenceRange = confidenceRanges[num];
			if (confidence >= confidenceRange.minConfidence && confidence <= confidenceRange.maxConfidence)
			{
				if (!(!allowConfidenceOverlap && flag))
				{
					confidenceRange.onWithinConfidenceRange?.Invoke();
					flag = true;
				}
			}
			else if (!(!allowConfidenceOverlap && flag2))
			{
				confidenceRange.onOutsideConfidenceRange?.Invoke();
				flag2 = true;
			}
			num++;
		}
		return flag;
	}
}

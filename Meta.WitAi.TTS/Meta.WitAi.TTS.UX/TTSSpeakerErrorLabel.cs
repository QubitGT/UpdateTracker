using System;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.WitAi.TTS.UX;

public class TTSSpeakerErrorLabel : TTSSpeakerObserver
{
	[SerializeField]
	private Text _errorLabel;

	private string _lastError;

	private string _lastLoadError;

	protected override void Awake()
	{
		base.Awake();
		if (_errorLabel == null)
		{
			_errorLabel = base.gameObject.GetComponentInChildren<Text>();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		RefreshError();
	}

	protected override void OnLoadBegin(TTSSpeaker speaker, TTSClipData clipData)
	{
		base.OnLoadBegin(speaker, clipData);
		_lastLoadError = null;
		RefreshError();
	}

	protected override void OnLoadFailed(TTSSpeaker speaker, TTSClipData clipData, string error)
	{
		base.OnLoadFailed(speaker, clipData, error);
		_lastLoadError = error;
		RefreshError();
	}

	public void RefreshError()
	{
		string currentError = GetCurrentError();
		if (string.Equals(_lastError, currentError, StringComparison.CurrentCulture))
		{
			return;
		}
		_lastError = currentError;
		if ((bool)_errorLabel)
		{
			if (string.IsNullOrEmpty(_lastError))
			{
				_errorLabel.text = string.Empty;
			}
			else
			{
				_errorLabel.text = "Error: " + _lastError;
			}
		}
	}

	public string GetCurrentError()
	{
		if (base.Speaker == null)
		{
			return "No TTS Speaker found";
		}
		if (base.Speaker.TTSService == null)
		{
			return "No TTS Service found on speaker";
		}
		string invalidError = base.Speaker.TTSService.GetInvalidError();
		if (!string.IsNullOrEmpty(invalidError))
		{
			return invalidError;
		}
		if (!string.IsNullOrEmpty(_lastLoadError))
		{
			return _lastLoadError;
		}
		return string.Empty;
	}
}

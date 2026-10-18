using Meta.WitAi;
using Meta.WitAi.Configuration;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace Oculus.VoiceSDK.UX;

[RequireComponent(typeof(Button))]
public class VoiceActivationButton : MonoBehaviour
{
	[Tooltip("Reference to the current voice service")]
	[SerializeField]
	private VoiceService _voiceService;

	[Tooltip("Text to be shown while the voice service is not actively recording")]
	[SerializeField]
	private string _activateText = "Activate";

	[Tooltip("Whether to immediately send data to service or to wait for the audio threshold")]
	[SerializeField]
	private bool _activateImmediately;

	[Tooltip("Text to be shown while the voice service is actively recording")]
	[SerializeField]
	private string _deactivateText = "Deactivate";

	[Tooltip("Whether to immediately abort request activation on deactivate")]
	[SerializeField]
	private bool _deactivateAndAbort;

	private Button _button;

	private Text _buttonLabel;

	private VoiceServiceRequest _request;

	private bool _isActive;

	private void Awake()
	{
		_buttonLabel = GetComponentInChildren<Text>();
		_button = GetComponent<Button>();
		if (_voiceService == null)
		{
			_voiceService = Object.FindAnyObjectByType<VoiceService>();
		}
	}

	private void OnEnable()
	{
		RefreshActive();
		if (_voiceService != null)
		{
			_voiceService.VoiceEvents.OnStartListening.AddListener(OnStartListening);
			_voiceService.VoiceEvents.OnStoppedListening.AddListener(OnStopListening);
		}
		if (_button != null)
		{
			_button.onClick.AddListener(OnClick);
		}
	}

	private void OnDisable()
	{
		_isActive = false;
		if (_voiceService != null)
		{
			_voiceService.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
			_voiceService.VoiceEvents.OnStoppedListening.RemoveListener(OnStopListening);
		}
		if (_button != null)
		{
			_button.onClick.RemoveListener(OnClick);
		}
	}

	private void OnClick()
	{
		if (!_isActive)
		{
			Activate();
		}
		else
		{
			Deactivate();
		}
	}

	private void Activate()
	{
		if (!_activateImmediately)
		{
			_request = _voiceService.Activate(new WitRequestOptions(), new VoiceServiceRequestEvents());
		}
		else
		{
			_request = _voiceService.ActivateImmediately(new WitRequestOptions(), new VoiceServiceRequestEvents());
		}
	}

	private void Deactivate()
	{
		if (!_deactivateAndAbort)
		{
			if (_request != null)
			{
				_request.DeactivateAudio();
			}
			else
			{
				_voiceService.Deactivate();
			}
		}
		else if (_request != null)
		{
			_request.Cancel();
		}
	}

	private void OnStartListening()
	{
		_isActive = true;
		RefreshActive();
	}

	private void OnStopListening()
	{
		_isActive = false;
		_request = null;
		RefreshActive();
	}

	private void RefreshActive()
	{
		if (_buttonLabel != null)
		{
			_buttonLabel.text = (_isActive ? _deactivateText : _activateText);
		}
	}
}

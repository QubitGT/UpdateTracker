using Meta.WitAi;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace Oculus.VoiceSDK.UX;

[RequireComponent(typeof(Text))]
[ExecuteInEditMode]
public class VoiceTranscriptionLabel : MonoBehaviour
{
	private Text _label;

	[Header("Listen Settings")]
	[Tooltip("Various voice services to be observed")]
	[SerializeField]
	private VoiceService[] _voiceServices;

	[Tooltip("Text color while receiving text")]
	[SerializeField]
	private Color _transcriptionColor = Color.black;

	[Header("Prompt Settings")]
	[Tooltip("Color to be used for prompt text")]
	[SerializeField]
	private Color _promptColor = new Color(0.2f, 0.2f, 0.2f);

	[Tooltip("Prompt text that displays while listening but prior to completion")]
	[SerializeField]
	private string _promptDefault = "Press activate to begin listening";

	[Tooltip("Prompt text that displays while listening but prior to completion")]
	[SerializeField]
	private string _promptListening = "Listening...";

	[Header("Error Settings")]
	[Tooltip("Color to be used for error text")]
	[SerializeField]
	private Color _errorColor = new Color(0.8f, 0.2f, 0.2f);

	public Text Label
	{
		get
		{
			if (_label == null)
			{
				_label = base.gameObject.GetComponent<Text>();
			}
			return _label;
		}
	}

	private void Awake()
	{
		if (_voiceServices == null || _voiceServices.Length == 0)
		{
			_voiceServices = Object.FindObjectsByType<VoiceService>(FindObjectsSortMode.None);
		}
	}

	private void OnEnable()
	{
		if (_voiceServices != null)
		{
			VoiceService[] voiceServices = _voiceServices;
			foreach (VoiceService obj in voiceServices)
			{
				obj.VoiceEvents.OnStartListening.AddListener(OnStartListening);
				obj.VoiceEvents.OnPartialTranscription.AddListener(OnTranscriptionChange);
				obj.VoiceEvents.OnFullTranscription.AddListener(OnTranscriptionChange);
				obj.VoiceEvents.OnError.AddListener(OnError);
				obj.VoiceEvents.OnComplete.AddListener(OnComplete);
			}
		}
	}

	private void OnDisable()
	{
		if (_voiceServices != null)
		{
			VoiceService[] voiceServices = _voiceServices;
			foreach (VoiceService obj in voiceServices)
			{
				obj.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
				obj.VoiceEvents.OnPartialTranscription.RemoveListener(OnTranscriptionChange);
				obj.VoiceEvents.OnFullTranscription.RemoveListener(OnTranscriptionChange);
				obj.VoiceEvents.OnError.RemoveListener(OnError);
				obj.VoiceEvents.OnComplete.RemoveListener(OnComplete);
			}
		}
	}

	private void OnStartListening()
	{
		SetText(_promptListening, _promptColor);
	}

	private void OnTranscriptionChange(string text)
	{
		SetText(text, _transcriptionColor);
	}

	private void OnError(string status, string error)
	{
		SetText("[" + status + "] " + error, _errorColor);
	}

	private void OnComplete(VoiceServiceRequest request)
	{
		if (Label != null && string.Equals(Label?.text, _promptListening))
		{
			SetText(_promptDefault, _promptColor);
		}
	}

	private void SetText(string newText, Color newColor)
	{
		if (!(Label == null) && (!string.Equals(newText, Label.text) || !(newColor == Label.color)))
		{
			_label.text = newText;
			_label.color = newColor;
		}
	}
}

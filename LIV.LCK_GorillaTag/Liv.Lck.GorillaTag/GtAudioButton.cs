using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

public class GtAudioButton : MonoBehaviour
{
	public UnityEvent<UnityAction<bool>> onTap;

	[SerializeField]
	private GtUiSettings _settings;

	[Space(10f)]
	[Header("Parameters")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _progress;

	[Space(10f)]
	[Header("Elements")]
	[SerializeField]
	private SpriteRenderer _iconRenderer;

	[SerializeField]
	private Sprite _onIcon;

	[SerializeField]
	private Sprite _offIcon;

	[SerializeField]
	private MeshRenderer _bodyRenderer;

	[SerializeField]
	private Transform _visualsTrans;

	[SerializeField]
	private LckDiscreetAudioController _audioController;

	[SerializeField]
	private bool _isActive = true;

	private string PROGRESS = "_Progress";

	private string IS_ON = "_Is_On";

	private Vector3 _defaultLocalPosition;

	private MaterialPropertyBlock _propertyBlock;

	private void OnValidate()
	{
		SetUp();
		SetProgress(_progress);
	}

	private void Start()
	{
		SetUp();
		_defaultLocalPosition = _visualsTrans.localPosition;
	}

	public void TapStarted()
	{
		onTap.Invoke(ProcessState);
		_visualsTrans.localPosition = _defaultLocalPosition + Vector3.forward * (0f - _settings.ActiveButtonOffset);
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickDown);
	}

	public void TapEnded()
	{
		_visualsTrans.localPosition = _defaultLocalPosition;
		_audioController.PlayDiscreetAudioClip(LckDiscreetAudioController.AudioClip.ClickUp);
	}

	public void SetProgress(float progress)
	{
		_propertyBlock.SetFloat(PROGRESS, progress);
		_bodyRenderer.SetPropertyBlock(_propertyBlock);
	}

	public void SetActiveState(bool isActive)
	{
		_isActive = isActive;
		if (!_isActive)
		{
			_iconRenderer.color = _settings.InactiveIconColor;
		}
	}

	private void SetUp()
	{
		_propertyBlock = new MaterialPropertyBlock();
		ProcessState(isOn: true);
	}

	private void ProcessState(bool isOn)
	{
		if (_isActive)
		{
			_iconRenderer.sprite = (isOn ? _onIcon : _offIcon);
			_iconRenderer.color = (isOn ? _settings.PrimaryIconColor : _settings.SecondaryIconColor);
			_propertyBlock.SetFloat(IS_ON, isOn ? 1 : 0);
			_bodyRenderer.SetPropertyBlock(_propertyBlock);
		}
	}
}

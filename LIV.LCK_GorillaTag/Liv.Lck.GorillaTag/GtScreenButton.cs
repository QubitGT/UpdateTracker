using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

public class GtScreenButton : MonoBehaviour
{
	[SerializeField]
	private Color _defaultColor;

	[SerializeField]
	private Color _activeColor;

	[SerializeField]
	private SpriteRenderer _iconRenderer;

	[SerializeField]
	private GtColliderTriggerProcessor _triggerProcessor;

	[Header("Events")]
	public UnityEvent onTapStarted;

	public UnityEvent onTapEnded;

	private Color _currentDefaultColor;

	private Color _currentActiveColor;

	private bool _isDisabled;

	private bool _isActive;

	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			_isActive = value;
			_currentDefaultColor = (value ? _activeColor : _defaultColor);
			_currentActiveColor = (value ? _activeColor : _defaultColor);
			_iconRenderer.color = _currentDefaultColor;
		}
	}

	private void Start()
	{
		_currentDefaultColor = _defaultColor;
		_currentActiveColor = _activeColor;
	}

	public void OnTapStarted()
	{
		if (!_isDisabled)
		{
			onTapStarted?.Invoke();
			_iconRenderer.color = _currentActiveColor;
		}
	}

	public void OnTapEnded()
	{
		if (!_isDisabled)
		{
			onTapEnded?.Invoke();
			_iconRenderer.color = _currentDefaultColor;
		}
	}

	public void DisableForDuration(float duration)
	{
		_isDisabled = true;
		_iconRenderer.enabled = false;
		_triggerProcessor.BlockTapping();
		Invoke("ReEnableButton", duration);
	}

	private void ReEnableButton()
	{
		_iconRenderer.color = _currentDefaultColor;
		_iconRenderer.enabled = true;
		_triggerProcessor.ResetToDefault();
		_isDisabled = false;
	}
}

using System.Collections;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtTabletFollower : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private float _heightOffsetForPlayerHead;

	[SerializeField]
	private float _rotationSpeed = 17f;

	[SerializeField]
	private bool _smoothRepositioning;

	[SerializeField]
	private bool _repositionInFrontOfPlayer;

	[SerializeField]
	private float _repositioningDuration;

	[SerializeField]
	private AnimationCurve _repositioningCurve;

	[Header("Elements")]
	[SerializeField]
	private GtToggle _isFollowingToggle;

	[SerializeField]
	private GtCounter _minDistanceCounter;

	[SerializeField]
	private GtCounter _smoothingCounter;

	[SerializeField]
	private GTLckController _controller;

	private bool _canUpdate;

	private bool _isEnabled;

	private bool _isFollowing;

	private float _minDistance;

	private float _smoothing;

	private Coroutine _repositioningAnimation;

	private Vector3 _followVelocity;

	private Vector3 _targetPosition;

	private Camera _playerCamera;

	private Transform _playerHead;

	private CameraMode _currentCameraMode;

	private Vector3 _playerSizeOffset = Vector3.down;

	private float _playerSizeModifier = 1f;

	private void OnEnable()
	{
		_isFollowingToggle.onValueChanged.AddListener(SetIsFollowing);
		_minDistanceCounter.onValueChanged.AddListener(SetMinDistance);
		_smoothingCounter.onValueChanged.AddListener(SetSmoothing);
		if (_controller != null)
		{
			_controller.OnCameraModeChanged += OnCameraModeChanged;
		}
	}

	private void OnCameraModeChanged(CameraMode mode, ILckCamera camera)
	{
		_currentCameraMode = mode;
		if (mode == CameraMode.Selfie)
		{
			_isEnabled = true;
		}
		else
		{
			_isEnabled = false;
		}
	}

	private void OnDisable()
	{
		_isFollowingToggle?.onValueChanged.RemoveListener(SetIsFollowing);
		_minDistanceCounter?.onValueChanged.RemoveListener(SetMinDistance);
		_smoothingCounter?.onValueChanged.RemoveListener(SetSmoothing);
		if (_controller != null)
		{
			_controller.OnCameraModeChanged -= OnCameraModeChanged;
		}
	}

	private void Start()
	{
		_isEnabled = true;
		_canUpdate = true;
		_targetPosition = base.transform.position;
		_playerHead = FindPlayerHeadTransform();
	}

	private void Update()
	{
		ProcessTabletFollowing();
	}

	public void SetPlayerSizeModifier(bool isDefaultScale, float modifier)
	{
		if (isDefaultScale)
		{
			_playerSizeOffset = Vector3.down;
			_playerSizeModifier = 1f;
		}
		else
		{
			_playerSizeOffset = Vector3.down * modifier;
			_playerSizeModifier = modifier;
		}
	}

	public float GetPlayerSizeModifier()
	{
		return _playerSizeModifier;
	}

	public void SetCanUpdate(bool value)
	{
		_canUpdate = value;
	}

	public void RepositionNearPlayer()
	{
		if (!_isEnabled)
		{
			return;
		}
		if (!_playerHead)
		{
			_playerHead = FindPlayerHeadTransform();
		}
		if (!_playerHead)
		{
			return;
		}
		Vector3 vector;
		if (_repositionInFrontOfPlayer)
		{
			Vector3 forward = _playerHead.forward;
			forward.y = 0f;
			forward.Normalize();
			vector = _playerHead.position + forward + Vector3.down * _heightOffsetForPlayerHead;
		}
		else
		{
			Vector3 vector2 = _playerHead.position + Vector3.down * _heightOffsetForPlayerHead;
			Vector3 vector3 = base.transform.position - vector2;
			vector3.y = 0f;
			vector3.Normalize();
			vector = vector2 + vector3;
		}
		if (_smoothRepositioning)
		{
			if (_repositioningAnimation != null)
			{
				StopCoroutine(_repositioningAnimation);
			}
			_repositioningAnimation = StartCoroutine(RepositioningAnimation(vector));
		}
		else
		{
			base.transform.position = vector;
		}
	}

	public void ResetFollowTarget()
	{
		if (_isEnabled)
		{
			_targetPosition = base.transform.position;
			if (_isFollowing && (bool)_playerHead)
			{
				base.transform.LookAt(_playerHead.position);
			}
			_followVelocity = Vector3.zero;
		}
	}

	public void IsEnabled(bool value)
	{
		if (_currentCameraMode == CameraMode.Selfie)
		{
			_isEnabled = value;
		}
	}

	private void ProcessTabletFollowing()
	{
		if (_isEnabled && _canUpdate && (bool)_playerHead && _isFollowing)
		{
			Vector3 vector = _playerHead.position + _playerSizeOffset * _heightOffsetForPlayerHead;
			Vector3 position = base.transform.position;
			Vector3 vector2 = position - vector;
			float num = _minDistance;
			if (_playerSizeModifier != 1f)
			{
				num *= _playerSizeModifier;
			}
			bool flag = vector2.magnitude < num;
			_targetPosition = vector + vector2.normalized * num;
			Vector3 position2 = Vector3.SmoothDamp(position, flag ? position : _targetPosition, ref _followVelocity, _smoothing);
			if (_smoothing > 0f)
			{
				Quaternion b = Quaternion.LookRotation(vector - base.transform.position);
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * (_rotationSpeed * InvertSmoothingValue(_smoothing)));
			}
			else
			{
				base.transform.LookAt(vector);
			}
			base.transform.position = position2;
		}
	}

	private float InvertSmoothingValue(float originalValue)
	{
		return Mathf.Clamp(1.1f - originalValue, 0.1f, 1f);
	}

	private Transform FindPlayerHeadTransform()
	{
		Transform result = null;
		_playerCamera = Camera.main;
		if (_playerCamera != null)
		{
			result = (GtTag.TryGetTransform(GtTagType.Player, out var transform) ? transform : _playerCamera.transform);
		}
		return result;
	}

	private IEnumerator RepositioningAnimation(Vector3 targetPosition)
	{
		float time = 0f;
		Vector3 startPosition = base.transform.position;
		while (time < _repositioningDuration)
		{
			time += Time.deltaTime;
			float t = _repositioningCurve.Evaluate(time / _repositioningDuration);
			base.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
			yield return null;
		}
	}

	private void SetIsFollowing(bool value)
	{
		_isFollowing = value;
	}

	private void SetMinDistance(int value)
	{
		_minDistance = value;
	}

	private void SetSmoothing(int value)
	{
		_smoothing = (float)value / 1000f;
	}
}

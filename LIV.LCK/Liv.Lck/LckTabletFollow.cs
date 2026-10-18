using System;
using Liv.Lck.Tablet;
using Liv.Lck.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Liv.Lck;

public class LckTabletFollow : MonoBehaviour
{
	[Header("Settings")]
	[Tooltip("An offset applied to the HMD's position to estimate the player's head/neck position. A small downward offset is typical.")]
	[SerializeField]
	private float _heightOffsetForPlayerHead;

	[Tooltip("The minimum smoothing value, preventing the tablet from becoming too rigid even at the lowest user setting.")]
	[SerializeField]
	private float _minFollowSmoothing = 0.2f;

	[Tooltip("A multiplier applied to the value from the follow distance UI button to determine the actual follow distance in world units.")]
	[SerializeField]
	private float _minFollowDistanceMultiplier = 0.75f;

	[Header("References")]
	[Tooltip("A reference to the main camera controller to get access to the HMD transform.")]
	[SerializeField]
	private LCKCameraController _controller;

	[Tooltip("The UI toggle that enables or disables the follow behavior.")]
	[SerializeField]
	private Toggle _isFollowingToggle;

	[Tooltip("A reference to the transform of the virtual selfie camera. The tablet will orient itself based on this camera's position.")]
	[SerializeField]
	private Transform _selfieCamera;

	[Tooltip("An optional, specific transform for the tablet to follow. If this is not set, it will default to following the user's HMD (player head).")]
	[SerializeField]
	private Transform _followTarget;

	[Tooltip("The UI button used to adjust the follow smoothing.")]
	[SerializeField]
	private LckDoubleButton _smoothingDoubleButton;

	[Tooltip("The UI button used to adjust the minimum follow distance.")]
	[SerializeField]
	private LckDoubleButton _followDistanceDoubleButton;

	[Tooltip("The root Rigidbody of the tablet. All movement is applied to this component.")]
	[SerializeField]
	private Rigidbody _rigidbodyRoot;

	private bool _isInCorrectCameraMode = true;

	private bool _isFollowToggleOn;

	private Vector3 _followVelocity;

	private Vector3 _targetPosition;

	private float _minFollowDistance;

	private float _followSmoothing;

	private RigidbodyInterpolation _defaultInterpolation;

	private void OnEnable()
	{
		_isFollowToggleOn = _isFollowingToggle.isOn;
		_isFollowingToggle.onValueChanged.AddListener(OnIsFollowToggled);
		_followDistanceDoubleButton.OnValueChanged += OnFollowDistanceChanged;
		_smoothingDoubleButton.OnValueChanged += OnSmoothingChanged;
		LCKCameraController controller = _controller;
		controller.OnCameraModeChanged = (Action<CameraMode>)Delegate.Combine(controller.OnCameraModeChanged, new Action<CameraMode>(OnCameraModeChanged));
	}

	private void OnDisable()
	{
		_isFollowingToggle.onValueChanged.RemoveListener(OnIsFollowToggled);
		_followDistanceDoubleButton.OnValueChanged -= OnFollowDistanceChanged;
		_smoothingDoubleButton.OnValueChanged -= OnSmoothingChanged;
		LCKCameraController controller = _controller;
		controller.OnCameraModeChanged = (Action<CameraMode>)Delegate.Remove(controller.OnCameraModeChanged, new Action<CameraMode>(OnCameraModeChanged));
	}

	private void Start()
	{
		SetInitialValuesFromDoubleButtons();
		_isInCorrectCameraMode = true;
		_targetPosition = base.transform.position;
		if (_rigidbodyRoot != null)
		{
			_defaultInterpolation = _rigidbodyRoot.interpolation;
		}
	}

	private void SetInitialValuesFromDoubleButtons()
	{
		_minFollowDistance = _followDistanceDoubleButton.Value * _minFollowDistanceMultiplier;
		_followSmoothing = CalculateFollowSmoothing(_smoothingDoubleButton.Value);
	}

	private void FixedUpdate()
	{
		ProcessTabletFollowingWithRigidbody();
	}

	public void SetFollowTarget(Transform target)
	{
		_followTarget = target;
	}

	private void ProcessTabletFollowingWithRigidbody()
	{
		if (_isInCorrectCameraMode && _isFollowToggleOn)
		{
			Vector3 vector = ((!_followTarget) ? (_controller.HmdTransform.position + Vector3.down * _heightOffsetForPlayerHead) : _followTarget.position);
			Vector3 position = _rigidbodyRoot.position;
			Vector3 vector2 = position - vector;
			bool flag = vector2.magnitude < _minFollowDistance;
			_targetPosition = vector + vector2.normalized * _minFollowDistance;
			Vector3 position2 = Vector3.SmoothDamp(position, flag ? position : _targetPosition, ref _followVelocity, _followSmoothing);
			_rigidbodyRoot.MovePosition(position2);
			Vector3 position3 = _selfieCamera.transform.position;
			_rigidbodyRoot.LookAtFromPivotPoint(position3, vector - position3, position2, _rigidbodyRoot.rotation);
		}
	}

	private void OnCameraModeChanged(CameraMode mode)
	{
		_isInCorrectCameraMode = mode == CameraMode.Selfie;
	}

	private void OnIsFollowToggled(bool value)
	{
		_isFollowToggleOn = value;
		if (_rigidbodyRoot != null)
		{
			_rigidbodyRoot.interpolation = (_isFollowToggleOn ? RigidbodyInterpolation.Interpolate : _defaultInterpolation);
		}
	}

	private void OnSmoothingChanged(float value)
	{
		_followSmoothing = CalculateFollowSmoothing(value);
	}

	private float CalculateFollowSmoothing(float value)
	{
		return Mathf.Max(value / 10f, _minFollowSmoothing);
	}

	private void OnFollowDistanceChanged(float value)
	{
		_minFollowDistance = value * _minFollowDistanceMultiplier;
	}
}

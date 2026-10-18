using System;
using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.Input;

[DefaultExecutionOrder(-90)]
[Feature(Feature.Interaction)]
public class OVRCameraRigRef : MonoBehaviour, IOVRCameraRigRef
{
	[Header("Configuration")]
	[SerializeField]
	private OVRCameraRig _ovrCameraRig;

	[SerializeField]
	private OVRHand _leftHand;

	[SerializeField]
	private OVRHand _rightHand;

	[SerializeField]
	private bool _requireOvrHands = true;

	protected bool _started;

	private bool _isLateUpdate;

	public OVRCameraRig CameraRig => _ovrCameraRig;

	public OVRHand LeftHand => GetHandCached(ref _leftHand, _ovrCameraRig.leftHandAnchor);

	public OVRHand RightHand => GetHandCached(ref _rightHand, _ovrCameraRig.rightHandAnchor);

	public Transform LeftController => _ovrCameraRig.leftControllerAnchor;

	public Transform RightController => _ovrCameraRig.rightControllerAnchor;

	public event Action<bool> WhenInputDataDirtied = delegate
	{
	};

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void FixedUpdate()
	{
		_isLateUpdate = false;
	}

	protected virtual void Update()
	{
		_isLateUpdate = false;
	}

	protected virtual void LateUpdate()
	{
		_isLateUpdate = true;
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			CameraRig.UpdatedAnchors += HandleInputDataDirtied;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			CameraRig.UpdatedAnchors -= HandleInputDataDirtied;
		}
	}

	private OVRHand GetHandCached(ref OVRHand cachedValue, Transform handAnchor)
	{
		if (cachedValue != null)
		{
			return cachedValue;
		}
		cachedValue = handAnchor.GetComponentInChildren<OVRHand>(includeInactive: true);
		_ = _requireOvrHands;
		return cachedValue;
	}

	private void HandleInputDataDirtied(OVRCameraRig cameraRig)
	{
		this.WhenInputDataDirtied(_isLateUpdate);
	}

	public void InjectAllOVRCameraRigRef(OVRCameraRig ovrCameraRig, bool requireHands)
	{
		InjectInteractionOVRCameraRig(ovrCameraRig);
		InjectRequireHands(requireHands);
	}

	public void InjectInteractionOVRCameraRig(OVRCameraRig ovrCameraRig)
	{
		_ovrCameraRig = ovrCameraRig;
		_leftHand = null;
		_rightHand = null;
	}

	public void InjectRequireHands(bool requireHands)
	{
		_requireOvrHands = requireHands;
	}
}

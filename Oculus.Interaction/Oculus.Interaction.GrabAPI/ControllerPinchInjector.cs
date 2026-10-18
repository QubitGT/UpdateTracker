using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.GrabAPI;

public class ControllerPinchInjector : MonoBehaviour
{
	private class ControllerPinchAPI : IFingerAPI
	{
		private IController _controller;

		private float _triggerStrength;

		private float _gripStrength;

		private bool _triggerDown;

		private bool _gripDown;

		private bool _prevTriggerDown;

		private bool _prevGripDown;

		private Pose _indexPinchPose = Pose.identity;

		private Pose _middlePinchPose = Pose.identity;

		private Pose _pinchPose = Pose.identity;

		public ControllerPinchAPI(IController controller)
		{
			_controller = controller;
		}

		public float GetFingerGrabScore(HandFinger finger)
		{
			switch (finger)
			{
			case HandFinger.Thumb:
				return _triggerStrength;
			case HandFinger.Index:
				return _triggerStrength;
			case HandFinger.Middle:
			case HandFinger.Ring:
			case HandFinger.Pinky:
				return _gripStrength;
			default:
				return 0f;
			}
		}

		public bool GetFingerIsGrabbing(HandFinger finger)
		{
			switch (finger)
			{
			case HandFinger.Thumb:
				return _triggerDown;
			case HandFinger.Index:
				return _triggerDown;
			case HandFinger.Middle:
			case HandFinger.Ring:
			case HandFinger.Pinky:
				return _gripDown;
			default:
				return false;
			}
		}

		public bool GetFingerIsGrabbingChanged(HandFinger finger, bool targetPinchState)
		{
			switch (finger)
			{
			case HandFinger.Thumb:
				if (_triggerDown != targetPinchState || _triggerDown == _prevTriggerDown)
				{
					if (_gripDown == targetPinchState)
					{
						return _gripDown != _prevGripDown;
					}
					return false;
				}
				return true;
			case HandFinger.Index:
				if (_triggerDown == targetPinchState)
				{
					return _triggerDown != _prevTriggerDown;
				}
				return false;
			case HandFinger.Middle:
			case HandFinger.Ring:
			case HandFinger.Pinky:
				if (_gripDown == targetPinchState)
				{
					return _gripDown != _prevGripDown;
				}
				return false;
			default:
				return false;
			}
		}

		public Vector3 GetWristOffsetLocal()
		{
			return _pinchPose.position;
		}

		public void Update(IHand hand)
		{
			ControllerInput controllerInput = _controller.ControllerInput;
			_prevGripDown = _gripDown;
			_prevTriggerDown = _triggerDown;
			_triggerStrength = controllerInput.Trigger;
			_triggerDown = controllerInput.TriggerButton;
			_gripStrength = controllerInput.Grip;
			_gripDown = controllerInput.GripButton;
			hand.GetJointPoseFromWrist(HandJointId.HandIndexTip, out _indexPinchPose);
			hand.GetJointPoseFromWrist(HandJointId.HandMiddleTip, out _middlePinchPose);
			hand.GetJointPoseFromWrist(HandJointId.HandThumbTip, out var pose);
			_indexPinchPose.Lerp(in pose, 0.5f);
			_middlePinchPose.Lerp(in pose, 0.5f);
			float num = _triggerStrength + _gripStrength;
			float t = ((num > 0f) ? (_gripStrength / num) : 0.5f);
			PoseUtils.Lerp(in _indexPinchPose, in _middlePinchPose, t, ref _pinchPose);
		}
	}

	[SerializeField]
	private HandGrabAPI _handGrabAPI;

	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	protected bool _started;

	private IController Controller { get; set; }

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_handGrabAPI.InjectOptionalFingerPinchAPI(new ControllerPinchAPI(Controller));
		_handGrabAPI.InjectOptionalFingerGrabAPI(new ControllerPinchAPI(Controller));
		this.EndStart(ref _started);
	}

	public void InjectAll(HandGrabAPI handGrabAPI, IController controller)
	{
		InjectHandGrabAPI(handGrabAPI);
		InjectController(controller);
	}

	public void InjectHandGrabAPI(HandGrabAPI handGrabAPI)
	{
		_handGrabAPI = handGrabAPI;
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}
}

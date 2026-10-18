using System;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

namespace Oculus.Interaction;

public class TouchHandGrabInteractor : PointerInteractor<TouchHandGrabInteractor, TouchHandGrabInteractable>, ITimeConsumer
{
	private class FingerStatus
	{
		public bool Locked;

		public bool Selecting;

		public HandJointId[] Joints;

		public Pose[] LocalJoints;

		public float CurlValueAtLock;

		public float Timer;
	}

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _openHand;

	[SerializeField]
	[Interface(typeof(IHandSphereMap), new Type[] { })]
	private UnityEngine.Object _handSphereMap;

	protected IHandSphereMap HandSphereMap;

	[SerializeField]
	private Transform _hoverLocation;

	[SerializeField]
	private Transform _grabLocation;

	[SerializeField]
	private float _minHoverDistance = 0.05f;

	[SerializeField]
	private float _curlDeltaThreshold = 3f;

	[SerializeField]
	private float _curlTimeThreshold = 0.05f;

	[SerializeField]
	[Min(1f)]
	private int _iterations = 10;

	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	[Optional]
	private UnityEngine.Object _grabPrerequisite;

	private Func<float> _timeProvider = () => Time.time;

	private Vector3 _saveOffset = Vector3.zero;

	private Vector3 GrabOffset = Vector3.zero;

	protected IActiveState GrabPrerequisite;

	private FingerStatus[] _fingerStatuses;

	private TouchShadowHand _touchShadowHand;

	private readonly ShadowHand _fromShadow = new ShadowHand();

	private readonly ShadowHand _toShadow = new ShadowHand();

	private readonly ShadowHand _openShadow = new ShadowHand();

	private bool _firstSelect;

	private float _previousTime;

	private float _deltaTime;

	private IHand Hand { get; set; }

	private IHand OpenHand { get; set; }

	private Vector3 GrabPosition => _grabLocation.position;

	private Quaternion GrabRotation => _grabLocation.rotation;

	public event Action WhenFingerLocked = delegate
	{
	};

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected override void Awake()
	{
		base.Awake();
		Hand = _hand as IHand;
		OpenHand = _openHand as IHand;
		HandSphereMap = _handSphereMap as IHandSphereMap;
		GrabPrerequisite = _grabPrerequisite as IActiveState;
		_nativeId = 6084210691412554338uL;
		_fingerStatuses = new FingerStatus[5];
		for (int i = 0; i < 5; i++)
		{
			int[] array = FingersMetadata.FINGER_TO_JOINT_INDEX[i];
			HandJointId[] array2 = new HandJointId[array.Length];
			for (int j = 0; j < array.Length; j++)
			{
				array2[j] = FingersMetadata.HAND_JOINT_IDS[array[j]];
			}
			_fingerStatuses[i] = new FingerStatus
			{
				Joints = array2,
				LocalJoints = new Pose[array2.Length]
			};
		}
	}

	protected override void Start()
	{
		base.Start();
		_touchShadowHand = new TouchShadowHand(HandSphereMap, Hand.Handedness, _iterations);
		_fromShadow.FromHand(Hand);
		_toShadow.FromHand(Hand);
		_previousTime = _timeProvider();
		_deltaTime = 0f;
	}

	public bool IsFingerLocked(HandFinger finger)
	{
		if (base.State == InteractorState.Select && _selectedInteractable == null)
		{
			return false;
		}
		return _fingerStatuses[(int)finger].Locked;
	}

	public Pose[] GetFingerJoints(HandFinger finger)
	{
		return _fingerStatuses[(int)finger].LocalJoints;
	}

	protected override void DoPreprocess()
	{
		base.DoPreprocess();
		_toShadow.FromHand(Hand);
		float previousTime = _timeProvider();
		_deltaTime = _timeProvider() - _previousTime;
		_previousTime = previousTime;
	}

	protected override void DoPostprocess()
	{
		if (base.State != InteractorState.Select && _interactable != null)
		{
			_fromShadow.FromHand(Hand);
		}
		else
		{
			_fromShadow.FromHandRoot(Hand);
			for (int i = 0; i < 5; i++)
			{
				FingerStatus fingerStatus = _fingerStatuses[i];
				if (fingerStatus.Locked)
				{
					continue;
				}
				for (int j = 0; j < fingerStatus.Joints.Length; j++)
				{
					HandJointId handJointId = fingerStatus.Joints[j];
					if (Hand.GetJointPoseLocal(handJointId, out var pose))
					{
						_fromShadow.SetLocalPose(handJointId, pose);
					}
				}
			}
		}
		base.DoPostprocess();
	}

	protected override bool ComputeShouldSelect()
	{
		return HandStatusSelecting();
	}

	protected override bool ComputeShouldUnselect()
	{
		return !HandStatusSelecting();
	}

	protected override void DoHoverUpdate()
	{
		TouchHandGrabInteractable interactable = _interactable;
		if (interactable == null)
		{
			return;
		}
		TouchShadowHand.GrabTouchInfo grabTouchInfo = new TouchShadowHand.GrabTouchInfo();
		_touchShadowHand.GrabTouch(_fromShadow, _toShadow, interactable.ColliderGroup, pushout: false, grabTouchInfo);
		if (!grabTouchInfo.grabbing)
		{
			_touchShadowHand.GrabTouch(_fromShadow, _toShadow, interactable.ColliderGroup, pushout: true, grabTouchInfo);
		}
		if (!grabTouchInfo.grabbing)
		{
			return;
		}
		_touchShadowHand.SetShadowRootFromHands(_fromShadow, _toShadow, grabTouchInfo.grabT);
		for (int i = 0; i < _fingerStatuses.Length; i++)
		{
			FingerStatus fingerStatus = _fingerStatuses[i];
			ComputeNewTouching(i, _interactable.ColliderGroup, grabTouchInfo.offset);
			if (!grabTouchInfo.grabbingFingers[i] || _fingerStatuses[i].Locked)
			{
				continue;
			}
			_openShadow.FromHand(OpenHand, OpenHand.Handedness != Hand.Handedness);
			if (_touchShadowHand.PushoutFinger(i, _fromShadow, _openShadow, _interactable.ColliderGroup, grabTouchInfo.offset))
			{
				for (int j = 0; j < fingerStatus.Joints.Length; j++)
				{
					HandJointId handJointId = fingerStatus.Joints[j];
					_fromShadow.SetLocalPose(handJointId, _touchShadowHand.ShadowHand.GetLocalPose(handJointId));
				}
				ComputeNewTouching(i, _interactable.ColliderGroup, grabTouchInfo.offset);
			}
		}
		if (!HandStatusSelecting())
		{
			ClearFingerLockStatuses();
		}
		else
		{
			GrabOffset = Vector3.zero;
			_saveOffset = Quaternion.Inverse(GrabRotation) * grabTouchInfo.offset;
			_firstSelect = true;
		}
		this.WhenFingerLocked();
	}

	private bool MeetsGrabPrerequisite()
	{
		if (GrabPrerequisite == null || GrabPrerequisite.Active)
		{
			return true;
		}
		return false;
	}

	private bool HandStatusSelecting()
	{
		if (MeetsGrabPrerequisite() && _fingerStatuses[0].Selecting)
		{
			if (!_fingerStatuses[1].Selecting && !_fingerStatuses[2].Selecting && !_fingerStatuses[3].Selecting)
			{
				return _fingerStatuses[4].Selecting;
			}
			return true;
		}
		return false;
	}

	private void ComputeNewTouching(int idx, ColliderGroup colliderGroup, Vector3 offset)
	{
		FingerStatus fingerStatus = _fingerStatuses[idx];
		if (fingerStatus.Locked)
		{
			return;
		}
		_touchShadowHand.SetShadowFingerFrom(idx, _fromShadow);
		if (!_touchShadowHand.CheckFingerTouch(idx, 0, colliderGroup, offset) && _touchShadowHand.GrabConformFinger(idx, _fromShadow, _toShadow, colliderGroup, offset))
		{
			fingerStatus.Locked = true;
			fingerStatus.Selecting = true;
			fingerStatus.Timer = 0f;
			_touchShadowHand.GetJointsFromShadow(fingerStatus.Joints, fingerStatus.LocalJoints, local: true);
			Pose[] array = new Pose[fingerStatus.Joints.Length];
			for (int i = 0; i < fingerStatus.Joints.Length; i++)
			{
				array[i] = _touchShadowHand.ShadowHand.GetWorldPose(fingerStatus.Joints[i]);
			}
			fingerStatus.CurlValueAtLock = FingerShapes.PosesListCurlValue(array);
			for (int j = 0; j < fingerStatus.Joints.Length; j++)
			{
				HandJointId handJointId = fingerStatus.Joints[j];
				_fromShadow.SetLocalPose(handJointId, _touchShadowHand.ShadowHand.GetLocalPose(handJointId));
			}
		}
	}

	private void ComputeNewRelease(int idx, ColliderGroup colliderGroup, Vector3 offset)
	{
		FingerStatus fingerStatus = _fingerStatuses[idx];
		if (!fingerStatus.Locked)
		{
			return;
		}
		Pose[] array = new Pose[fingerStatus.Joints.Length];
		for (int i = 0; i < fingerStatus.Joints.Length; i++)
		{
			array[i] = _toShadow.GetWorldPose(fingerStatus.Joints[i]);
		}
		if (FingerShapes.PosesListCurlValue(array) >= fingerStatus.CurlValueAtLock - _curlDeltaThreshold)
		{
			fingerStatus.Timer = 0f;
			return;
		}
		if (!_touchShadowHand.GrabReleaseFinger(idx, _fromShadow, _toShadow, colliderGroup, offset))
		{
			fingerStatus.Timer = 0f;
			return;
		}
		fingerStatus.Timer += _deltaTime;
		if (!(fingerStatus.Timer < _curlTimeThreshold))
		{
			fingerStatus.Locked = false;
			fingerStatus.Selecting = false;
		}
	}

	protected override void DoSelectUpdate()
	{
		if (_firstSelect)
		{
			GrabOffset = _saveOffset;
			_saveOffset = Vector3.zero;
			_firstSelect = false;
			return;
		}
		TouchHandGrabInteractable selectedInteractable = _selectedInteractable;
		if (selectedInteractable == null)
		{
			for (int i = 0; i < _fingerStatuses.Length; i++)
			{
				FingerStatus fingerStatus = _fingerStatuses[i];
				if (fingerStatus.Locked)
				{
					fingerStatus.Selecting = true;
					fingerStatus.Locked = false;
					fingerStatus.Timer = 0f;
					Pose[] array = new Pose[fingerStatus.Joints.Length];
					for (int j = 0; j < fingerStatus.Joints.Length; j++)
					{
						array[j] = _toShadow.GetWorldPose(fingerStatus.Joints[j]);
					}
					fingerStatus.CurlValueAtLock = FingerShapes.PosesListCurlValue(array);
				}
			}
			for (int k = 0; k < _fingerStatuses.Length; k++)
			{
				FingerStatus fingerStatus2 = _fingerStatuses[k];
				if (!fingerStatus2.Selecting)
				{
					continue;
				}
				Pose[] array2 = new Pose[fingerStatus2.Joints.Length];
				for (int l = 0; l < fingerStatus2.Joints.Length; l++)
				{
					array2[l] = _toShadow.GetWorldPose(fingerStatus2.Joints[l]);
				}
				if (FingerShapes.PosesListCurlValue(array2) >= fingerStatus2.CurlValueAtLock - _curlDeltaThreshold)
				{
					fingerStatus2.Timer = 0f;
					continue;
				}
				fingerStatus2.Timer += _deltaTime;
				if (fingerStatus2.Timer < _curlTimeThreshold)
				{
					break;
				}
				fingerStatus2.Selecting = false;
			}
			return;
		}
		_touchShadowHand.ShadowHand.Copy(_fromShadow);
		_touchShadowHand.SetShadowRootFromHand(_fromShadow);
		if (MeetsGrabPrerequisite())
		{
			for (int m = 0; m < _fingerStatuses.Length; m++)
			{
				if (_fingerStatuses[m].Locked)
				{
					ComputeNewRelease(m, selectedInteractable.ColliderGroup, Vector3.zero);
				}
				else
				{
					ComputeNewTouching(m, selectedInteractable.ColliderGroup, Vector3.zero);
				}
			}
		}
		this.WhenFingerLocked();
	}

	public override void Unselect()
	{
		if (!base.ShouldUnselect)
		{
			base.Unselect();
			return;
		}
		ClearFingerLockStatuses();
		GrabOffset = Vector3.zero;
		this.WhenFingerLocked();
		base.Unselect();
	}

	private void ClearFingerLockStatuses()
	{
		for (int i = 0; i < _fingerStatuses.Length; i++)
		{
			_fingerStatuses[i].Locked = false;
			_fingerStatuses[i].Selecting = false;
		}
	}

	protected override TouchHandGrabInteractable ComputeCandidate()
	{
		TouchHandGrabInteractable result = null;
		float num = float.MaxValue;
		foreach (TouchHandGrabInteractable item in Interactable<TouchHandGrabInteractor, TouchHandGrabInteractable>.Registry.List())
		{
			foreach (Collider collider in item.ColliderGroup.Colliders)
			{
				float sqrMagnitude = (collider.ClosestPoint(_hoverLocation.position) - _hoverLocation.position).sqrMagnitude;
				if (sqrMagnitude < num && sqrMagnitude < _minHoverDistance * _minHoverDistance)
				{
					num = sqrMagnitude;
					result = item;
				}
			}
		}
		return result;
	}

	protected override Pose ComputePointerPose()
	{
		return new Pose(GrabPosition + GrabRotation * GrabOffset, GrabRotation);
	}

	public void InjectAllTouchHandGrabInteractor(IHand hand, IHand openHand, IHandSphereMap handSphereMap, Transform hoverLocation, Transform grabLocation)
	{
		InjectHand(hand);
		InjectOpenHand(openHand);
		InjectHandSphereMap(handSphereMap);
		InjectHoverLocation(hoverLocation);
		InjectGrabLocation(grabLocation);
	}

	public void InjectHand(IHand hand)
	{
		Hand = hand;
		_hand = hand as UnityEngine.Object;
	}

	public void InjectOpenHand(IHand openHand)
	{
		OpenHand = openHand;
		_openHand = openHand as UnityEngine.Object;
	}

	public void InjectHandSphereMap(IHandSphereMap handSphereMap)
	{
		HandSphereMap = handSphereMap;
		_handSphereMap = handSphereMap as UnityEngine.Object;
	}

	public void InjectHoverLocation(Transform hoverLocation)
	{
		_hoverLocation = hoverLocation;
	}

	public void InjectGrabLocation(Transform grabLocation)
	{
		_grabLocation = grabLocation;
	}

	public void InjectOptionalGrabPrerequisite(IActiveState grabPrerequisite)
	{
		GrabPrerequisite = grabPrerequisite;
		_grabPrerequisite = grabPrerequisite as UnityEngine.Object;
	}

	public void InjectOptionalMinHoverDistance(float minHoverDistance)
	{
		_minHoverDistance = minHoverDistance;
	}

	public void InjectOptionalCurlDeltaThreshold(float threshold)
	{
		_curlDeltaThreshold = threshold;
	}

	public void InjectOptionalCurlTimeThreshold(float seconds)
	{
		_curlTimeThreshold = seconds;
	}

	public void InjectOptionalIterations(int iterations)
	{
		_iterations = iterations;
	}

	[Obsolete("Use SetTimeProvide()")]
	public void InjectOptionalTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}
}

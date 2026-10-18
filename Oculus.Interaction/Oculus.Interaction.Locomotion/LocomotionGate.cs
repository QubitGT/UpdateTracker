using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class LocomotionGate : MonoBehaviour
{
	public enum LocomotionMode
	{
		None,
		Teleport,
		Turn
	}

	[Serializable]
	public class GateSection
	{
		public float minAngle = -70f;

		public float maxAngle = 290f;

		public bool canEnterDirectly = true;

		public LocomotionMode locomotionMode;

		public float ScoreToAngle(float angle)
		{
			float num = Mathf.Repeat(angle - minAngle, 360f);
			float num2 = Mathf.Repeat(maxAngle - minAngle, 360f);
			if (num > num2)
			{
				return float.PositiveInfinity;
			}
			float target = (minAngle + maxAngle) / 2f;
			return Mathf.Repeat(Mathf.DeltaAngle(angle, target), 360f);
		}
	}

	public struct LocomotionModeEventArgs
	{
		public LocomotionMode PreviousMode { get; }

		public LocomotionMode NewMode { get; }

		public LocomotionModeEventArgs(LocomotionMode previousMode, LocomotionMode newMode)
		{
			PreviousMode = previousMode;
			NewMode = newMode;
		}
	}

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	private Transform _shoulder;

	[SerializeField]
	private GateSection[] _gateSections = new GateSection[3]
	{
		new GateSection
		{
			locomotionMode = LocomotionMode.Teleport,
			maxAngle = 95f
		},
		new GateSection
		{
			locomotionMode = LocomotionMode.Turn,
			minAngle = 40f,
			maxAngle = 165f
		},
		new GateSection
		{
			locomotionMode = LocomotionMode.Teleport,
			minAngle = 120f,
			canEnterDirectly = false
		}
	};

	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _enableShape;

	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _disableShape;

	[SerializeField]
	private VirtualActiveState _turningState;

	[SerializeField]
	private VirtualActiveState _teleportState;

	protected bool _started;

	private bool _previousShapeEnabled;

	private int _currentGateIndex = -1;

	private LocomotionMode _activeMode;

	private Action<LocomotionModeEventArgs> _whenActiveModeChanged = delegate
	{
	};

	private static readonly GateSection DefaultSection = new GateSection
	{
		locomotionMode = LocomotionMode.None
	};

	private const float _enterPoseThreshold = 0.5f;

	private const float _wristLimit = -70f;

	private bool _cancelled;

	public IHand Hand { get; private set; }

	private IActiveState EnableShape { get; set; }

	private IActiveState DisableShape { get; set; }

	public LocomotionMode ActiveMode
	{
		get
		{
			return _activeMode;
		}
		private set
		{
			if (_activeMode != value)
			{
				LocomotionMode activeMode = _activeMode;
				_activeMode = value;
				_teleportState.Active = _activeMode == LocomotionMode.Teleport;
				_turningState.Active = _activeMode == LocomotionMode.Turn;
				_whenActiveModeChanged(new LocomotionModeEventArgs(activeMode, _activeMode));
			}
		}
	}

	public float CurrentAngle { get; private set; }

	public Vector3 WristDirection { get; private set; }

	public Pose StabilizationPose { get; private set; } = Pose.identity;


	public event Action<LocomotionModeEventArgs> WhenActiveModeChanged
	{
		add
		{
			_whenActiveModeChanged = (Action<LocomotionModeEventArgs>)Delegate.Combine(_whenActiveModeChanged, value);
		}
		remove
		{
			_whenActiveModeChanged = (Action<LocomotionModeEventArgs>)Delegate.Remove(_whenActiveModeChanged, value);
		}
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		EnableShape = _enableShape as IActiveState;
		DisableShape = _disableShape as IActiveState;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated += HandleHandupdated;
			Disable();
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= HandleHandupdated;
			Disable();
		}
	}

	public void Disable()
	{
		ActiveMode = LocomotionMode.None;
		_currentGateIndex = -1;
		_cancelled = false;
	}

	public void Cancel()
	{
		ActiveMode = LocomotionMode.None;
		if (_currentGateIndex >= 0)
		{
			_cancelled = true;
		}
	}

	private void HandleHandupdated()
	{
		if (!Hand.GetJointPose(HandJointId.HandWristRoot, out var pose))
		{
			Disable();
			return;
		}
		bool flag = Hand.Handedness == Handedness.Right;
		Vector3 up = Vector3.up;
		Vector3 normalized = (pose.position - _shoulder.position).normalized;
		Vector3 normalized2 = Vector3.Cross(up, normalized).normalized;
		normalized2 = (flag ? normalized2 : (-normalized2));
		Vector3 vector = pose.rotation * (flag ? Constants.RightThumbSide : Constants.LeftThumbSide);
		bool flag2 = (double)Vector3.Dot(pose.rotation * (flag ? Constants.RightDistal : Constants.LeftDistal), Vector3.ProjectOnPlane(normalized, up).normalized) * 0.5 + 0.5 > 0.5;
		vector = Vector3.ProjectOnPlane(vector, normalized).normalized;
		float num = Vector3.SignedAngle(vector, normalized2, normalized);
		num = ((Hand.Handedness == Handedness.Right) ? (0f - num) : num);
		if (num < -70f)
		{
			num += 360f;
		}
		CurrentAngle = num;
		StabilizationPose = new Pose(_shoulder.position, Quaternion.LookRotation(normalized));
		WristDirection = vector;
		bool flag3 = false;
		if (EnableShape.Active && !_previousShapeEnabled)
		{
			flag3 = true;
		}
		_previousShapeEnabled = EnableShape.Active;
		if (_currentGateIndex < 0 && flag3 && flag2)
		{
			GateSection bestGateSection = GetBestGateSection(CurrentAngle, out _currentGateIndex);
			if (bestGateSection.canEnterDirectly)
			{
				ActiveMode = bestGateSection.locomotionMode;
			}
			else
			{
				_currentGateIndex = -1;
			}
		}
		else if (_currentGateIndex >= 0 && DisableShape.Active)
		{
			Disable();
		}
		else if (_currentGateIndex >= 0 && !_cancelled)
		{
			GateSection gateSection = _gateSections[_currentGateIndex];
			if (CurrentAngle < gateSection.minAngle)
			{
				_currentGateIndex = Mathf.Max(0, _currentGateIndex - 1);
				ActiveMode = _gateSections[_currentGateIndex].locomotionMode;
			}
			else if (CurrentAngle > gateSection.maxAngle)
			{
				_currentGateIndex = Mathf.Min(_gateSections.Length - 1, _currentGateIndex + 1);
				ActiveMode = _gateSections[_currentGateIndex].locomotionMode;
			}
		}
	}

	private GateSection GetBestGateSection(float angle, out int index)
	{
		float num = float.PositiveInfinity;
		index = -1;
		for (int i = 0; i < _gateSections.Length; i++)
		{
			float num2 = _gateSections[i].ScoreToAngle(angle);
			if (num2 < num)
			{
				num = num2;
				index = i;
			}
		}
		if (index == -1)
		{
			return DefaultSection;
		}
		return _gateSections[index];
	}

	public void InjectAllLocomotionGate(IHand hand, Transform shoulder, IActiveState enableShape, IActiveState disableShape, VirtualActiveState turningState, VirtualActiveState teleportState, GateSection[] gateSections)
	{
		InjectHand(hand);
		InjectShoulder(shoulder);
		InjectEnableShape(enableShape);
		InjectDisableShape(disableShape);
		InjectTurningState(turningState);
		InjectTeleportState(teleportState);
		InjectGateSections(gateSections);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectShoulder(Transform shoulder)
	{
		_shoulder = shoulder;
	}

	public void InjectEnableShape(IActiveState enableShape)
	{
		_enableShape = enableShape as UnityEngine.Object;
		EnableShape = enableShape;
	}

	public void InjectDisableShape(IActiveState disableShape)
	{
		_disableShape = disableShape as UnityEngine.Object;
		DisableShape = disableShape;
	}

	public void InjectTurningState(VirtualActiveState turningState)
	{
		_turningState = turningState;
	}

	public void InjectTeleportState(VirtualActiveState teleportState)
	{
		_teleportState = teleportState;
	}

	public void InjectGateSections(GateSection[] gateSections)
	{
		_gateSections = gateSections;
	}
}

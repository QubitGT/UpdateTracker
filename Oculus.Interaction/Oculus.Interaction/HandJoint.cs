using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandJoint : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	private HandJointId _handJointId;

	[SerializeField]
	[InspectorName("Offset")]
	private Vector3 _localPositionOffset;

	[SerializeField]
	[InspectorName("Rotation")]
	private Quaternion _rotationOffset = Quaternion.identity;

	[SerializeField]
	private HandJointId _jointId;

	[SerializeField]
	[InspectorName("Offset")]
	private Vector3 _posOffset;

	[SerializeField]
	[InspectorName("Rotation")]
	private Quaternion _rotOffset = Quaternion.identity;

	[Tooltip("Provided for backwards compatibility. When set, the rotation of the driven transform for this component will match the legacy hand skeleton joint orientation rather than the current OpenXR joint orientation.")]
	[SerializeField]
	private bool _useLegacyOrientation;

	private static readonly Vector3 LEFT_LEGACY_ROT = new Vector3(180f, 90f, 0f);

	private static readonly Vector3 RIGHT_LEGACY_ROT = new Vector3(0f, -90f, 0f);

	[SerializeField]
	[Tooltip("When the attached hand's handedness is set to Left, this property will mirror the offsets. This allows for offset values to be set in Right hand coordinates for both Left and Right hands.")]
	private bool _mirrorOffsetsForLeftHand = true;

	[Header("Freeze rotations")]
	[SerializeField]
	private bool _freezeRotationX;

	[SerializeField]
	private bool _freezeRotationY;

	[SerializeField]
	private bool _freezeRotationZ;

	private Pose _cachedPose = Pose.identity;

	protected bool _started;

	public IHand Hand { get; private set; }

	[Obsolete("This property is provided for backwards compatibility only, and its function will be removed in a future version of Interaction SDK.")]
	public bool UseLegacyOrientation
	{
		get
		{
			return _useLegacyOrientation;
		}
		set
		{
			_useLegacyOrientation = value;
		}
	}

	public bool FreezeRotationX
	{
		get
		{
			return _freezeRotationX;
		}
		set
		{
			_freezeRotationX = value;
		}
	}

	public bool FreezeRotationY
	{
		get
		{
			return _freezeRotationY;
		}
		set
		{
			_freezeRotationY = value;
		}
	}

	public bool FreezeRotationZ
	{
		get
		{
			return _freezeRotationZ;
		}
		set
		{
			_freezeRotationZ = value;
		}
	}

	public bool MirrorOffsetsForLeftHand
	{
		get
		{
			return _mirrorOffsetsForLeftHand;
		}
		set
		{
			_mirrorOffsetsForLeftHand = value;
		}
	}

	public HandJointId HandJointId
	{
		get
		{
			return _jointId;
		}
		set
		{
			_jointId = value;
		}
	}

	public Vector3 LocalPositionOffset
	{
		get
		{
			return _posOffset;
		}
		set
		{
			_posOffset = value;
		}
	}

	public Quaternion RotationOffset
	{
		get
		{
			return _rotOffset;
		}
		set
		{
			_rotOffset = value;
		}
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
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
			Hand.WhenHandUpdated += HandleHandUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= HandleHandUpdated;
		}
	}

	private void HandleHandUpdated()
	{
		if (Hand.GetJointPose(HandJointId, out var pose))
		{
			GetOffset(ref _cachedPose, Hand.Handedness, Hand.Scale);
			_cachedPose.Postmultiply(in pose);
			if (UseLegacyOrientation)
			{
				_cachedPose.rotation = pose.rotation * ((Hand.Handedness == Handedness.Left) ? Quaternion.Euler(LEFT_LEGACY_ROT) : Quaternion.Euler(RIGHT_LEGACY_ROT));
			}
			_cachedPose.rotation = FreezeRotation(_cachedPose.rotation);
			base.transform.SetPose(in _cachedPose);
		}
	}

	private Quaternion FreezeRotation(Quaternion rotation)
	{
		if (_freezeRotationX || _freezeRotationY || _freezeRotationZ)
		{
			Vector3 eulerAngles = rotation.eulerAngles;
			Quaternion quaternion = Quaternion.Euler(new Vector3(eulerAngles.x, 0f, 0f));
			Quaternion quaternion2 = Quaternion.Euler(new Vector3(0f, eulerAngles.y, 0f));
			Quaternion quaternion3 = Quaternion.Euler(new Vector3(0f, 0f, eulerAngles.z));
			Quaternion identity = Quaternion.identity;
			if (!_freezeRotationY)
			{
				identity *= quaternion2;
			}
			if (!_freezeRotationX)
			{
				identity *= quaternion;
			}
			if (!_freezeRotationZ)
			{
				identity *= quaternion3;
			}
			rotation = identity;
		}
		return rotation;
	}

	private void GetOffset(ref Pose pose, Handedness handedness, float scale)
	{
		if (_mirrorOffsetsForLeftHand && handedness == Handedness.Left)
		{
			Vector3 position = LocalPositionOffset * scale;
			pose.position = HandMirroring.Mirror(in position);
			Quaternion rotation = RotationOffset;
			pose.rotation = HandMirroring.Mirror(in rotation);
		}
		else
		{
			pose.position = LocalPositionOffset * scale;
			pose.rotation = RotationOffset;
		}
	}

	public void InjectAllHandJoint(IHand hand)
	{
		InjectHand(hand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}
}

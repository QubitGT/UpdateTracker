using System;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction;

public class HandRootOffset : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	[InspectorName("Offset")]
	private Vector3 _offset;

	[SerializeField]
	[InspectorName("Rotation")]
	private Quaternion _rotation = Quaternion.identity;

	[SerializeField]
	[InspectorName("Offset")]
	private Vector3 _posOffset;

	[SerializeField]
	[InspectorName("Rotation")]
	private Quaternion _rotOffset = Quaternion.identity;

	[SerializeField]
	[FormerlySerializedAs("_mirrorLeftRotation")]
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

	public Vector3 Offset
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

	public Quaternion Rotation
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

	[Obsolete("Use MirrorOffsetsForLeftHand instead.")]
	public bool MirrorLeftRotation
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
		if (Hand.GetRootPose(out var pose))
		{
			GetOffset(ref _cachedPose);
			_cachedPose.Postmultiply(in pose);
			_cachedPose.rotation = FreezeRotation(_cachedPose.rotation);
			base.transform.SetPose(in _cachedPose);
		}
	}

	public void GetOffset(ref Pose pose)
	{
		if (_started)
		{
			GetOffset(ref pose, Hand.Handedness, Hand.Scale);
		}
	}

	public void GetOffset(ref Pose pose, Handedness handedness, float scale)
	{
		if (_mirrorOffsetsForLeftHand && handedness == Handedness.Left)
		{
			Vector3 position = Offset;
			pose.position = HandMirroring.Mirror(in position) * scale;
			Quaternion rotation = Rotation;
			pose.rotation = HandMirroring.Mirror(in rotation);
		}
		else
		{
			pose.position = Offset * scale;
			pose.rotation = Rotation;
		}
	}

	public void GetWorldPose(ref Pose pose)
	{
		pose.position = base.transform.position;
		pose.rotation = base.transform.rotation;
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

	public void InjectAllHandRootOffset(IHand hand)
	{
		InjectHand(hand);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	[Obsolete("Use the Offset setter instead")]
	public void InjectOffset(Vector3 offset)
	{
		Offset = offset;
	}

	[Obsolete("Use the Rotation setter instead")]
	public void InjectRotation(Quaternion rotation)
	{
		Rotation = rotation;
	}

	[Obsolete("Use InjectAllHandRootOffset instead")]
	public void InjectAllHandWristOffset(IHand hand, Vector3 offset, Quaternion rotation)
	{
		InjectHand(hand);
		InjectOffset(offset);
		InjectRotation(rotation);
	}
}

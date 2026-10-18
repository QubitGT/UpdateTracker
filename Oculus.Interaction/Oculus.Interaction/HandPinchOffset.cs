using System;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandPinchOffset : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	private HandGrabAPI _handGrabApi;

	[SerializeField]
	[Optional]
	private Collider _collider;

	[SerializeField]
	[InspectorName("Offset")]
	private Vector3 _localPositionOffset;

	[SerializeField]
	[InspectorName("Rotation")]
	private Quaternion _rotationOffset = Quaternion.identity;

	[SerializeField]
	[InspectorName("Offset")]
	private Vector3 _posOffset;

	[SerializeField]
	[InspectorName("Rotation")]
	private Quaternion _rotOffset = Quaternion.identity;

	[SerializeField]
	[Tooltip("When the attached hand's handedness is set to Left, this property will mirror the offsets. This allows for offset values to be set in Right hand coordinates for both Left and Right hands.")]
	private bool _mirrorOffsetsForLeftHand = true;

	protected bool _started;

	private Pose _cachedPose = Pose.identity;

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
		Vector3 position = _handGrabApi.GetPinchCenter();
		if (_collider != null)
		{
			position = _collider.ClosestPoint(position);
		}
		Hand.GetRootPose(out var pose);
		Pose b = new Pose(position, pose.rotation);
		GetOffset(ref _cachedPose, Hand.Handedness, Hand.Scale);
		_cachedPose.Postmultiply(in b);
		base.transform.SetPose(in _cachedPose);
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

	public void InjectAllHandPinchOffset(IHand hand, HandGrabAPI handGrabApi)
	{
		InjectHand(hand);
		InjectHandGrabAPI(handGrabApi);
	}

	public void InjectHand(IHand hand)
	{
		Hand = hand;
		_hand = hand as UnityEngine.Object;
	}

	public void InjectHandGrabAPI(HandGrabAPI handGrabApi)
	{
		_handGrabApi = handGrabApi;
	}

	public void InjectOptionalCollider(Collider collider)
	{
		_collider = collider;
	}
}

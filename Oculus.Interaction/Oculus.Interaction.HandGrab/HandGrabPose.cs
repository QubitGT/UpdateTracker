using System;
using Oculus.Interaction.Grab;
using Oculus.Interaction.Grab.GrabSurfaces;
using Oculus.Interaction.HandGrab.Visuals;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public class HandGrabPose : MonoBehaviour
{
	internal enum OVROffsetMode
	{
		None,
		Apply,
		Ignore
	}

	private static readonly Pose OVR_OFFSET_LH = new Pose(Vector3.zero, Quaternion.Euler(0f, 90f, 180f));

	private static readonly Pose OVR_OFFSET_RH = new Pose(Vector3.zero, Quaternion.Euler(0f, 90f, 0f));

	[SerializeField]
	[Optional]
	[Interface(typeof(IGrabSurface), new Type[] { })]
	private UnityEngine.Object _surface;

	private IGrabSurface _snapSurface;

	[SerializeField]
	[Tooltip("Transform used as a reference to measure the local data of the HandGrabPose")]
	private Transform _relativeTo;

	[SerializeField]
	private bool _usesHandPose = true;

	[SerializeField]
	[Optional]
	[HideInInspector]
	[InspectorName("Hand Pose")]
	private HandPose _handPose = new HandPose();

	[SerializeField]
	[Optional]
	[HideInInspector]
	private HandPose _targetHandPose = new HandPose();

	[SerializeField]
	[HideInInspector]
	private HandGhostProvider _ghostProvider;

	[SerializeField]
	[HideInInspector]
	private HandGhostProvider _handGhostProvider;

	[SerializeField]
	[HideInInspector]
	private OVROffsetMode _ovrOffsetMode;

	public IGrabSurface SnapSurface
	{
		get
		{
			return _snapSurface ?? (_surface as IGrabSurface);
		}
		private set
		{
			_snapSurface = value;
		}
	}

	public HandPose HandPose
	{
		get
		{
			if (!_usesHandPose)
			{
				return null;
			}
			return _targetHandPose;
		}
	}

	public float RelativeScale => base.transform.lossyScale.x / _relativeTo.lossyScale.x;

	public Pose RelativePose
	{
		get
		{
			if (_relativeTo != null)
			{
				return PoseUtils.DeltaScaled(_relativeTo, WorldPose);
			}
			return LocalPose;
		}
	}

	public Transform RelativeTo => _relativeTo;

	private Pose LocalPose => base.transform.GetPose(Space.Self);

	private Pose WorldPose => base.transform.GetPose();

	internal static Pose GetOVROffset(Handedness handedness)
	{
		if (handedness != 0)
		{
			return OVR_OFFSET_RH;
		}
		return OVR_OFFSET_LH;
	}

	protected virtual void Awake()
	{
	}

	protected virtual void Reset()
	{
		_relativeTo = GetComponentInParent<IRelativeToRef>()?.RelativeTo;
	}

	public bool UsesHandPose()
	{
		return _usesHandPose;
	}

	[Obsolete("Use CalculateBestPose with offset instead")]
	public virtual bool CalculateBestPose(Pose userPose, Handedness handedness, PoseMeasureParameters scoringModifier, Transform relativeTo, ref HandGrabResult result)
	{
		Pose offset = Pose.identity;
		CalculateBestPose(in userPose, in offset, relativeTo, handedness, scoringModifier, ref result);
		return true;
	}

	public virtual void CalculateBestPose(in Pose userPose, in Pose offset, Transform relativeTo, Handedness handedness, PoseMeasureParameters scoringModifier, ref HandGrabResult result)
	{
		result.HasHandPose = false;
		result.Score = CompareNearPoses(in userPose, in offset, relativeTo, scoringModifier, out var bestWorldPose);
		result.RelativePose = relativeTo.Delta(in bestWorldPose);
		if (HandPose != null)
		{
			result.HasHandPose = true;
			result.HandPose.CopyFrom(HandPose);
		}
	}

	private GrabPoseScore CompareNearPoses(in Pose worldPoint, in Pose offset, Transform relativeTo, PoseMeasureParameters scoringModifier, out Pose bestWorldPose)
	{
		if (SnapSurface != null)
		{
			return SnapSurface.CalculateBestPoseAtSurface(in worldPoint, in offset, out bestWorldPose, in scoringModifier, relativeTo);
		}
		bestWorldPose = PoseUtils.GlobalPoseScaled(relativeTo, RelativePose);
		return new GrabPoseScore(in worldPoint, in bestWorldPose, in offset, scoringModifier);
	}

	public void InjectAllHandGrabPose(Transform relativeTo)
	{
		InjectRelativeTo(relativeTo);
	}

	public void InjectRelativeTo(Transform relativeTo)
	{
		_relativeTo = relativeTo;
	}

	public void InjectOptionalSurface(IGrabSurface surface)
	{
		_surface = surface as UnityEngine.Object;
		SnapSurface = surface;
	}

	public void InjectOptionalHandPose(HandPose handPose)
	{
		_targetHandPose = handPose;
		_usesHandPose = _targetHandPose != null;
	}
}

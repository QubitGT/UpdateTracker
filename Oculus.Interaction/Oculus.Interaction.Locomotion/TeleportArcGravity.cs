using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class TeleportArcGravity : MonoBehaviour, IPolyline
{
	[SerializeField]
	[Tooltip("The transform from which the arc will be casted")]
	private Transform _origin;

	[SerializeField]
	[Tooltip("A point behind the origin used to stabilize the aiming direction.")]
	private Transform _stabilizationPoint;

	[SerializeField]
	[Tooltip("Increases the range of the arc based on the distance from the origin to the stabilization point.")]
	private AnimationCurve _rangeCurve = new AnimationCurve(new Keyframe(0f, 5f), new Keyframe(1f, 20f));

	[SerializeField]
	[Tooltip("Mixes the direction of the origin with the stabilized direction based on the pitch.")]
	private AnimationCurve _stabilizationMixCurve = AnimationCurve.Constant(0f, 1f, 1f);

	[SerializeField]
	[Tooltip("Alters the pitch of the origin based on the entry pitch")]
	private AnimationCurve _pitchCurve = new AnimationCurve(new Keyframe(-90f, -90f), new Keyframe(90f, 90f));

	[SerializeField]
	[Tooltip("Multiplier for the gravity force")]
	private float _gravityModifier = 2.3f;

	[SerializeField]
	[Min(2f)]
	private int _arcPointsCount = 30;

	private static readonly Vector3 GRAVITY = new Vector3(0f, -9.81f, 0f);

	private static readonly float GROUND_MARGIN = 2f;

	private Pose _pose = Pose.identity;

	private float _speed;

	protected bool _started;

	public AnimationCurve RangeCurve
	{
		get
		{
			return _rangeCurve;
		}
		set
		{
			_rangeCurve = value;
		}
	}

	public AnimationCurve StabilizationMixCurve
	{
		get
		{
			return _stabilizationMixCurve;
		}
		set
		{
			_stabilizationMixCurve = value;
		}
	}

	public AnimationCurve PitchCurve
	{
		get
		{
			return _pitchCurve;
		}
		set
		{
			_pitchCurve = value;
		}
	}

	public float GravityModifier
	{
		get
		{
			return _gravityModifier;
		}
		set
		{
			_gravityModifier = value;
		}
	}

	public int PointsCount
	{
		get
		{
			return _arcPointsCount;
		}
		set
		{
			_arcPointsCount = value;
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		UpdateArcParameters();
		this.EndStart(ref _started);
	}

	protected virtual void Update()
	{
		UpdateArcParameters();
	}

	public Vector3 PointAtIndex(int index)
	{
		float t = (float)index / ((float)_arcPointsCount - 1f);
		return EvaluateGravityArc(_pose, _speed, t);
	}

	private Vector3 EvaluateGravityArc(Pose origin, float speed, float t)
	{
		Vector3 result = origin.position + origin.forward * speed * t + 0.5f * t * t * GRAVITY * _gravityModifier;
		if (t >= 1f && result.y > origin.position.y - GROUND_MARGIN)
		{
			result.y = origin.position.y - GROUND_MARGIN;
		}
		return result;
	}

	public void UpdateArcParameters()
	{
		_pose = CalculatePose();
		_speed = CalculateSpeed(_pose);
	}

	private Pose CalculatePose()
	{
		Pose pose = _origin.GetPose();
		StabilizeDirection(ref pose);
		RemapPitch(ref pose);
		return pose;
	}

	private float CalculateSpeed(Pose pose)
	{
		Vector3 vector = pose.position - _stabilizationPoint.position;
		vector.y = 0f;
		float magnitude = vector.magnitude;
		return _rangeCurve.Evaluate(magnitude);
	}

	private void StabilizeDirection(ref Pose pose)
	{
		Vector3 up = _stabilizationPoint.up;
		Vector3 vector = (pose.position - _stabilizationPoint.position).normalized;
		if (vector.sqrMagnitude == 0f)
		{
			vector = _stabilizationPoint.forward;
		}
		Quaternion b = Quaternion.LookRotation(vector);
		float time = Vector3.Dot(vector, up) * 0.5f + 0.5f;
		time = _stabilizationMixCurve.Evaluate(time);
		Quaternion rotation = Quaternion.Lerp(pose.rotation, b, time);
		pose.rotation = rotation;
	}

	private void RemapPitch(ref Pose pose)
	{
		Vector3 up = _stabilizationPoint.up;
		Vector3 forward = pose.forward;
		Vector3 normalized = Vector3.ProjectOnPlane(forward, up).normalized;
		Vector3 normalized2 = Vector3.Cross(normalized, up).normalized;
		float time = Vector3.SignedAngle(normalized, forward, normalized2);
		time = _pitchCurve.Evaluate(time);
		Vector3 forward2 = Quaternion.AngleAxis(time, normalized2) * normalized;
		if (forward2.sqrMagnitude != 0f)
		{
			pose.rotation = Quaternion.LookRotation(forward2, pose.up);
		}
	}

	public void InjectAllTeleportArcGravity(Transform origin, Transform stabilizationPoint)
	{
		InjectOrigin(origin);
		InjectStabilizationPoint(stabilizationPoint);
	}

	public void InjectOrigin(Transform origin)
	{
		_origin = origin;
	}

	public void InjectStabilizationPoint(Transform stabilizationPoint)
	{
		_stabilizationPoint = stabilizationPoint;
	}
}

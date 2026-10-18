using UnityEngine;
using UnityEngine.Serialization;

namespace Liv.Lck.Smoothing;

[DefaultExecutionOrder(1000)]
public class LckStabilizer : MonoBehaviour
{
	[Header("Transform References")]
	[SerializeField]
	[FormerlySerializedAs("StabilizationTarget")]
	private Transform _stabilizationTarget;

	[SerializeField]
	[FormerlySerializedAs("TargetToFollow")]
	private Transform _targetToFollow;

	[Header("Stabilization Settings")]
	[SerializeField]
	[FormerlySerializedAs("PositionalSmoothing")]
	private float _positionalSmoothing = 0.1f;

	[SerializeField]
	[FormerlySerializedAs("RotationalSmoothing")]
	private float _rotationalSmoothing = 0.1f;

	[SerializeField]
	[FormerlySerializedAs("AffectPosition")]
	private bool _affectPosition = true;

	[SerializeField]
	[FormerlySerializedAs("AffectRotation")]
	private bool _affectRotation = true;

	[SerializeField]
	private UpdateTimingMode _stabilizationUpdateTimingMode = UpdateTimingMode.LateUpdate;

	[Header("Optional References")]
	[SerializeField]
	[Tooltip("(Optional) Follow target movement relative to this transform will be stabilized. If left unspecified, will stabilize follow target movement in world space.")]
	private Transform _stabilizationSpaceOrigin;

	private KalmanFilterVector3 _positionFilter;

	private KalmanFilterQuaternion _rotationFilter;

	public UpdateTimingMode StabilizationUpdateTimingMode
	{
		get
		{
			return _stabilizationUpdateTimingMode;
		}
		set
		{
			_stabilizationUpdateTimingMode = value;
		}
	}

	public Transform StabilizationTarget
	{
		get
		{
			return _stabilizationTarget;
		}
		set
		{
			_stabilizationTarget = value;
		}
	}

	public Transform TargetToFollow
	{
		get
		{
			return _targetToFollow;
		}
		set
		{
			_targetToFollow = value;
		}
	}

	public Transform StabilizationSpaceOrigin
	{
		get
		{
			return _stabilizationSpaceOrigin;
		}
		set
		{
			if (!(_stabilizationSpaceOrigin == value))
			{
				_stabilizationSpaceOrigin = value;
				HandleStabilizationSpaceChanged();
			}
		}
	}

	public float PositionalSmoothing
	{
		get
		{
			return _positionalSmoothing;
		}
		set
		{
			_positionalSmoothing = value;
		}
	}

	public float RotationalSmoothing
	{
		get
		{
			return _rotationalSmoothing;
		}
		set
		{
			_rotationalSmoothing = value;
		}
	}

	public bool AffectPosition
	{
		get
		{
			return _affectPosition;
		}
		set
		{
			_affectPosition = value;
		}
	}

	public bool AffectRotation
	{
		get
		{
			return _affectRotation;
		}
		set
		{
			_affectRotation = value;
		}
	}

	private KalmanFilterVector3 PositionFilter => _positionFilter ?? (_positionFilter = new KalmanFilterVector3(GetStabilizationSpacePosition(TargetToFollow.position)));

	private KalmanFilterQuaternion RotationFilter => _rotationFilter ?? (_rotationFilter = new KalmanFilterQuaternion(GetStabilizationSpaceRotation(TargetToFollow.rotation)));

	private bool HasCustomStabilizationSpace => _stabilizationSpaceOrigin;

	private void LateUpdate()
	{
		if (StabilizationUpdateTimingMode == UpdateTimingMode.LateUpdate)
		{
			DoStabilizationUpdate(PositionalSmoothing, RotationalSmoothing);
		}
	}

	private void Update()
	{
		if (StabilizationUpdateTimingMode == UpdateTimingMode.Update)
		{
			DoStabilizationUpdate(PositionalSmoothing, RotationalSmoothing);
		}
	}

	private void FixedUpdate()
	{
		if (StabilizationUpdateTimingMode == UpdateTimingMode.FixedUpdate)
		{
			DoStabilizationUpdate(PositionalSmoothing, RotationalSmoothing);
		}
	}

	public void ReachTargetInstantly()
	{
		DoStabilizationUpdate(0f, 0f);
	}

	private void DoStabilizationUpdate(float positionalSmoothing, float rotationalSmoothing)
	{
		if (AffectPosition)
		{
			Vector3 stabilizationSpacePosition = GetStabilizationSpacePosition(TargetToFollow.position);
			Vector3 stabilizationSpacePosition2 = PositionFilter.Update(stabilizationSpacePosition, Time.deltaTime, positionalSmoothing);
			StabilizationTarget.position = GetWorldPosition(stabilizationSpacePosition2);
		}
		if (AffectRotation)
		{
			Quaternion stabilizationSpaceRotation = GetStabilizationSpaceRotation(TargetToFollow.rotation);
			Quaternion stabilizationSpaceRotation2 = RotationFilter.Update(stabilizationSpaceRotation, Time.deltaTime, rotationalSmoothing);
			StabilizationTarget.rotation = GetWorldRotation(stabilizationSpaceRotation2);
		}
	}

	private void HandleStabilizationSpaceChanged()
	{
		if (AffectPosition)
		{
			Vector3 stabilizationSpacePosition = GetStabilizationSpacePosition(StabilizationTarget.position);
			PositionFilter.Update(stabilizationSpacePosition, Time.deltaTime, 0f);
		}
		if (AffectRotation)
		{
			Quaternion stabilizationSpaceRotation = GetStabilizationSpaceRotation(StabilizationTarget.rotation);
			RotationFilter.Update(stabilizationSpaceRotation, Time.deltaTime, 0f);
		}
	}

	private Vector3 GetWorldPosition(Vector3 stabilizationSpacePosition)
	{
		if (!HasCustomStabilizationSpace)
		{
			return stabilizationSpacePosition;
		}
		return StabilizationSpaceOrigin.TransformPoint(stabilizationSpacePosition);
	}

	private Quaternion GetWorldRotation(Quaternion stabilizationSpaceRotation)
	{
		if (!HasCustomStabilizationSpace)
		{
			return stabilizationSpaceRotation;
		}
		return StabilizationSpaceOrigin.rotation * stabilizationSpaceRotation;
	}

	private Vector3 GetStabilizationSpacePosition(Vector3 worldPosition)
	{
		if (!HasCustomStabilizationSpace)
		{
			return worldPosition;
		}
		return StabilizationSpaceOrigin.InverseTransformPoint(worldPosition);
	}

	private Quaternion GetStabilizationSpaceRotation(Quaternion worldRotation)
	{
		if (!HasCustomStabilizationSpace)
		{
			return worldRotation;
		}
		return Quaternion.Inverse(StabilizationSpaceOrigin.rotation) * worldRotation;
	}
}

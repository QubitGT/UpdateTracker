using System;
using Oculus.Interaction.Surfaces;
using UnityEngine;

namespace Oculus.Interaction;

public class RayInteractor : PointerInteractor<RayInteractor, RayInteractable>
{
	public class RayCandidateProperties : ICandidatePosition
	{
		public RayInteractable ClosestInteractable { get; }

		public Vector3 CandidatePosition { get; }

		public RayCandidateProperties(RayInteractable closestInteractable, Vector3 candidatePosition)
		{
			ClosestInteractable = closestInteractable;
			CandidatePosition = candidatePosition;
		}
	}

	[Tooltip("A selector indicating when the Interactor should select or unselect the best available interactable.")]
	[SerializeField]
	[Interface(typeof(ISelector), new Type[] { })]
	private UnityEngine.Object _selector;

	[Tooltip("The origin of the ray.")]
	[SerializeField]
	private Transform _rayOrigin;

	[Tooltip("The maximum length of the ray.")]
	[SerializeField]
	private float _maxRayLength = 5f;

	[SerializeField]
	[Tooltip("(Meters, World) The threshold below which distances to a surface are treated as equal for the purposes of ranking.")]
	private float _equalDistanceThreshold = 0.001f;

	private RayCandidateProperties _rayCandidateProperties;

	private IMovement _movement;

	private SurfaceHit _movedHit;

	private Pose _movementHitDelta = Pose.identity;

	public Vector3 Origin { get; protected set; }

	public Quaternion Rotation { get; protected set; }

	public Vector3 Forward { get; protected set; }

	public Vector3 End { get; set; }

	public float MaxRayLength
	{
		get
		{
			return _maxRayLength;
		}
		set
		{
			_maxRayLength = value;
		}
	}

	public SurfaceHit? CollisionInfo { get; protected set; }

	public Ray Ray { get; protected set; }

	public override object CandidateProperties => _rayCandidateProperties;

	protected override void Awake()
	{
		base.Awake();
		base.Selector = _selector as ISelector;
		_nativeId = 5936159140244058656uL;
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void DoPreprocess()
	{
		Transform transform = _rayOrigin.transform;
		Origin = transform.position;
		Rotation = transform.rotation;
		Forward = transform.forward;
		End = Origin + MaxRayLength * Forward;
		Ray = new Ray(Origin, Forward);
	}

	protected override RayInteractable ComputeCandidate()
	{
		CollisionInfo = null;
		RayInteractable rayInteractable = null;
		float num = float.MaxValue;
		Vector3 candidatePosition = Vector3.zero;
		foreach (RayInteractable item in Interactable<RayInteractor, RayInteractable>.Registry.List(this))
		{
			Ray ray = Ray;
			float maxDistance = MaxRayLength;
			if (item.Raycast(ray, out var hit, in maxDistance, selectSurface: false))
			{
				bool flag = Mathf.Abs(hit.Distance - num) < _equalDistanceThreshold;
				if ((!flag && hit.Distance < num) || (flag && ComputeCandidateTiebreaker(item, rayInteractable) > 0))
				{
					num = hit.Distance;
					rayInteractable = item;
					CollisionInfo = hit;
					candidatePosition = hit.Point;
				}
			}
		}
		float num2 = ((rayInteractable != null) ? num : MaxRayLength);
		End = Origin + num2 * Forward;
		_rayCandidateProperties = new RayCandidateProperties(rayInteractable, candidatePosition);
		return rayInteractable;
	}

	protected override int ComputeCandidateTiebreaker(RayInteractable a, RayInteractable b)
	{
		int num = base.ComputeCandidateTiebreaker(a, b);
		if (num != 0)
		{
			return num;
		}
		return a.TiebreakerScore.CompareTo(b.TiebreakerScore);
	}

	protected override void InteractableSelected(RayInteractable interactable)
	{
		if (interactable != null)
		{
			_movedHit = CollisionInfo.Value;
			Pose to = new Pose(_movedHit.Point, Quaternion.LookRotation(_movedHit.Normal));
			Pose source = new Pose(_movedHit.Point, Quaternion.LookRotation(-_movedHit.Normal));
			Pose to2 = _rayOrigin.GetPose();
			_movement = interactable.GenerateMovement(in to2, in source);
			if (_movement != null)
			{
				to2 = _movement.Pose;
				_movementHitDelta = PoseUtils.Delta(in to2, in to);
			}
		}
		base.InteractableSelected(interactable);
	}

	protected override void InteractableUnselected(RayInteractable interactable)
	{
		if (_movement != null)
		{
			_movement.StopAndSetPose(_movement.Pose);
		}
		base.InteractableUnselected(interactable);
		_movement = null;
	}

	protected override void DoSelectUpdate()
	{
		RayInteractable selectedInteractable = _selectedInteractable;
		if (_movement != null)
		{
			_movement.UpdateTarget(_rayOrigin.GetPose());
			_movement.Tick();
			Pose a = _movement.Pose;
			Pose pose = PoseUtils.Multiply(in a, in _movementHitDelta);
			_movedHit.Point = pose.position;
			_movedHit.Normal = pose.forward;
			CollisionInfo = _movedHit;
			End = _movedHit.Point;
			return;
		}
		CollisionInfo = null;
		if (selectedInteractable != null)
		{
			Ray ray = Ray;
			float maxDistance = MaxRayLength;
			if (selectedInteractable.Raycast(ray, out var hit, in maxDistance, selectSurface: true))
			{
				End = hit.Point;
				CollisionInfo = hit;
				return;
			}
		}
		End = Origin + MaxRayLength * Forward;
	}

	protected override Pose ComputePointerPose()
	{
		if (_movement != null)
		{
			return _movement.Pose;
		}
		if (CollisionInfo.HasValue)
		{
			Vector3 point = CollisionInfo.Value.Point;
			Quaternion rotation = Quaternion.LookRotation(CollisionInfo.Value.Normal);
			return new Pose(point, rotation);
		}
		return Pose.identity;
	}

	public void InjectAllRayInteractor(ISelector selector, Transform rayOrigin)
	{
		InjectSelector(selector);
		InjectRayOrigin(rayOrigin);
	}

	public void InjectSelector(ISelector selector)
	{
		_selector = selector as UnityEngine.Object;
		base.Selector = selector;
	}

	public void InjectRayOrigin(Transform rayOrigin)
	{
		_rayOrigin = rayOrigin;
	}

	public void InjectOptionalEqualDistanceThreshold(float equalDistanceThreshold)
	{
		_equalDistanceThreshold = equalDistanceThreshold;
	}
}

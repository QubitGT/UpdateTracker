using System;
using Oculus.Interaction.Surfaces;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class TeleportInteractable : Interactable<TeleportInteractor, TeleportInteractable>
{
	[SerializeField]
	[Tooltip("Indicates if the interactable is valid for teleport. Setting it to false can be convenient to block the arc.")]
	private bool _allowTeleport = true;

	[SerializeField]
	[Optional]
	[ConditionalHide("_allowTeleport", true)]
	[Tooltip("An override for the Interactor EqualDistanceThreshold used when comparing the interactable against other interactables that does not allow teleport.")]
	private float _equalDistanceToBlockerOverride;

	[SerializeField]
	[Optional]
	[Tooltip("Establishes the priority when several interactables are hit at the same time (EqualDistanceThreshold) by the arc.")]
	private int _tieBreakerScore;

	[SerializeField]
	[Interface(typeof(ISurface), new Type[] { })]
	[Tooltip("Surface against which the interactor will check collision with the arc.")]
	private UnityEngine.Object _surface;

	[Header("Target", order = -1)]
	[SerializeField]
	[Optional]
	[Tooltip("A specific point in space where the player should teleport to.")]
	private Transform _targetPoint;

	[SerializeField]
	[Optional]
	[Tooltip("When true, the player will also face the direction specified by the target point.")]
	private bool _faceTargetDirection;

	[SerializeField]
	[Optional]
	[Tooltip("When true, instead of aligning the players feet to the TargetPoint it will align the head.")]
	private bool _eyeLevel;

	public bool AllowTeleport
	{
		get
		{
			return _allowTeleport;
		}
		set
		{
			_allowTeleport = value;
		}
	}

	public float EqualDistanceToBlockerOverride
	{
		get
		{
			return _equalDistanceToBlockerOverride;
		}
		set
		{
			_equalDistanceToBlockerOverride = value;
		}
	}

	public int TieBreakerScore
	{
		get
		{
			return _tieBreakerScore;
		}
		set
		{
			_tieBreakerScore = value;
		}
	}

	public ISurface Surface { get; private set; }

	public IBounds SurfaceBounds { get; private set; }

	public bool FaceTargetDirection
	{
		get
		{
			return _faceTargetDirection;
		}
		set
		{
			_faceTargetDirection = value;
		}
	}

	public bool EyeLevel
	{
		get
		{
			return _eyeLevel;
		}
		set
		{
			_eyeLevel = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Surface = _surface as ISurface;
		SurfaceBounds = _surface as IBounds;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	public bool IsInRange(in Pose origin, float maxSqrDistance)
	{
		if (SurfaceBounds == null)
		{
			return true;
		}
		Bounds bounds = SurfaceBounds.Bounds;
		Vector3 center = bounds.center;
		center.y = origin.position.y;
		Vector3 vector = center - origin.position;
		float num = bounds.extents.x * bounds.extents.x + bounds.extents.z * bounds.extents.z;
		if (vector.sqrMagnitude <= num)
		{
			return true;
		}
		if (!CheckSquaredDistances(vector.sqrMagnitude, num, maxSqrDistance))
		{
			return false;
		}
		Vector3 forward = origin.forward;
		float num2 = 1f / Mathf.Sqrt(1f - forward.y * forward.y);
		forward.y = 0f;
		forward.x *= num2;
		forward.z *= num2;
		if (SqrDistanceToSegment(center, origin.position, forward, maxSqrDistance) <= num)
		{
			return true;
		}
		return false;
		static bool CheckSquaredDistances(float x, float y, float threshold)
		{
			float num4 = x - y - threshold;
			if (x > y + threshold && num4 * num4 > 4f * y * threshold)
			{
				return false;
			}
			return true;
		}
		static float SqrDistanceToSegment(Vector3 point, Vector3 origin, Vector3 dir, float sqrLength)
		{
			float num3 = Vector3.Dot(point - origin, dir);
			if (num3 < 0f)
			{
				num3 = 0f;
			}
			else if (num3 * num3 > sqrLength)
			{
				num3 = Mathf.Sqrt(sqrLength);
			}
			Vector3 vector2 = origin + dir * num3;
			return (point - vector2).sqrMagnitude;
		}
	}

	public bool DetectHit(Vector3 from, Vector3 to, out TeleportHit hit)
	{
		Vector3 direction = to - from;
		Ray ray = new Ray(from, direction);
		if (Surface.Raycast(in ray, out var hit2, direction.magnitude))
		{
			hit = new TeleportHit(base.transform, hit2.Point, hit2.Normal);
			return true;
		}
		hit = TeleportHit.DEFAULT;
		return false;
	}

	public Pose TargetPose(Pose hitPose)
	{
		Pose result = hitPose;
		if (_targetPoint != null)
		{
			result.position = _targetPoint.position;
			result.rotation = _targetPoint.rotation;
		}
		return result;
	}

	public void InjectAllTeleportInteractable(ISurface surface)
	{
		InjectSurface(surface);
	}

	public void InjectSurface(ISurface surface)
	{
		_surface = surface as UnityEngine.Object;
		Surface = surface;
		SurfaceBounds = surface as IBounds;
	}

	public void InjectOptionalTargetPoint(Transform targetPoint)
	{
		_targetPoint = targetPoint;
	}
}

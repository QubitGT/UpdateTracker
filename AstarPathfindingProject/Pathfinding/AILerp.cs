using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pathfinding;

[RequireComponent(typeof(Seeker))]
[AddComponentMenu("Pathfinding/AI/AILerp (2D,3D)")]
[HelpURL("http://arongranberg.com/astar/documentation/stable/class_pathfinding_1_1_a_i_lerp.php")]
public class AILerp : VersionedMonoBehaviour, IAstarAI
{
	public AutoRepathPolicy autoRepath = new AutoRepathPolicy();

	public bool canMove = true;

	public float speed = 3f;

	[FormerlySerializedAs("rotationIn2D")]
	public OrientationMode orientation;

	public bool enableRotation = true;

	public float rotationSpeed = 10f;

	public bool interpolatePathSwitches = true;

	public float switchPathInterpolationSpeed = 5f;

	[NonSerialized]
	public bool updatePosition = true;

	[NonSerialized]
	public bool updateRotation = true;

	protected Seeker seeker;

	protected Transform tr;

	protected ABPath path;

	protected bool canSearchAgain = true;

	protected Vector3 previousMovementOrigin;

	protected Vector3 previousMovementDirection;

	protected float pathSwitchInterpolationTime;

	protected PathInterpolator interpolator = new PathInterpolator();

	private bool startHasRun;

	private Vector3 previousPosition1;

	private Vector3 previousPosition2;

	private Vector3 simulatedPosition;

	private Quaternion simulatedRotation;

	[FormerlySerializedAs("target")]
	[SerializeField]
	[HideInInspector]
	private Transform targetCompatibility;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("repathRate")]
	private float repathRateCompatibility = float.NaN;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("canSearch")]
	private bool canSearchCompability;

	public float repathRate
	{
		get
		{
			return autoRepath.period;
		}
		set
		{
			autoRepath.period = value;
		}
	}

	public bool canSearch
	{
		get
		{
			return autoRepath.mode != AutoRepathPolicy.Mode.Never;
		}
		set
		{
			autoRepath.mode = (value ? AutoRepathPolicy.Mode.EveryNSeconds : AutoRepathPolicy.Mode.Never);
		}
	}

	[Obsolete("Use orientation instead")]
	public bool rotationIn2D
	{
		get
		{
			return orientation == OrientationMode.YAxisForward;
		}
		set
		{
			orientation = (value ? OrientationMode.YAxisForward : OrientationMode.ZAxisForward);
		}
	}

	public bool reachedEndOfPath { get; private set; }

	public bool reachedDestination
	{
		get
		{
			if (!reachedEndOfPath || !interpolator.valid)
			{
				return false;
			}
			Vector3 vector = destination - interpolator.endPoint;
			if (orientation == OrientationMode.YAxisForward)
			{
				vector.z = 0f;
			}
			else
			{
				vector.y = 0f;
			}
			if (remainingDistance + vector.magnitude >= 0.05f)
			{
				return false;
			}
			return true;
		}
	}

	public Vector3 destination { get; set; }

	[Obsolete("Use the destination property or the AIDestinationSetter component instead")]
	public Transform target
	{
		get
		{
			AIDestinationSetter component = GetComponent<AIDestinationSetter>();
			if (!(component != null))
			{
				return null;
			}
			return component.target;
		}
		set
		{
			targetCompatibility = null;
			AIDestinationSetter aIDestinationSetter = GetComponent<AIDestinationSetter>();
			if (aIDestinationSetter == null)
			{
				aIDestinationSetter = base.gameObject.AddComponent<AIDestinationSetter>();
			}
			aIDestinationSetter.target = value;
			destination = ((value != null) ? value.position : new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
		}
	}

	public Vector3 position
	{
		get
		{
			if (!updatePosition)
			{
				return simulatedPosition;
			}
			return tr.position;
		}
	}

	public Quaternion rotation
	{
		get
		{
			if (!updateRotation)
			{
				return simulatedRotation;
			}
			return tr.rotation;
		}
		set
		{
			if (updateRotation)
			{
				tr.rotation = value;
			}
			else
			{
				simulatedRotation = value;
			}
		}
	}

	float IAstarAI.radius
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	float IAstarAI.height
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	float IAstarAI.maxSpeed
	{
		get
		{
			return speed;
		}
		set
		{
			speed = value;
		}
	}

	bool IAstarAI.canSearch
	{
		get
		{
			return canSearch;
		}
		set
		{
			canSearch = value;
		}
	}

	bool IAstarAI.canMove
	{
		get
		{
			return canMove;
		}
		set
		{
			canMove = value;
		}
	}

	public Vector3 velocity
	{
		get
		{
			if (!(Time.deltaTime > 1E-05f))
			{
				return Vector3.zero;
			}
			return (previousPosition1 - previousPosition2) / Time.deltaTime;
		}
	}

	Vector3 IAstarAI.desiredVelocity => ((IAstarAI)this).velocity;

	Vector3 IAstarAI.steeringTarget
	{
		get
		{
			if (!interpolator.valid)
			{
				return simulatedPosition;
			}
			return interpolator.position + interpolator.tangent;
		}
	}

	public float remainingDistance
	{
		get
		{
			return Mathf.Max(interpolator.remainingDistance, 0f);
		}
		set
		{
			interpolator.remainingDistance = Mathf.Max(value, 0f);
		}
	}

	public bool hasPath => interpolator.valid;

	public bool pathPending => !canSearchAgain;

	public bool isStopped { get; set; }

	public Action onSearchPath { get; set; }

	protected virtual bool shouldRecalculatePath
	{
		get
		{
			if (canSearchAgain)
			{
				return autoRepath.ShouldRecalculatePath(position, 0f, destination);
			}
			return false;
		}
	}

	void IAstarAI.Move(Vector3 deltaPosition)
	{
	}

	protected AILerp()
	{
		destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
	}

	protected override void Awake()
	{
		base.Awake();
		tr = base.transform;
		seeker = GetComponent<Seeker>();
		seeker.startEndModifier.adjustStartPoint = () => simulatedPosition;
	}

	protected virtual void Start()
	{
		startHasRun = true;
		Init();
	}

	protected virtual void OnEnable()
	{
		Seeker obj = seeker;
		obj.pathCallback = (OnPathDelegate)Delegate.Combine(obj.pathCallback, new OnPathDelegate(OnPathComplete));
		Init();
	}

	private void Init()
	{
		if (startHasRun)
		{
			Teleport(position, clearPath: false);
			autoRepath.Reset();
			if (shouldRecalculatePath)
			{
				SearchPath();
			}
		}
	}

	public void OnDisable()
	{
		ClearPath();
		Seeker obj = seeker;
		obj.pathCallback = (OnPathDelegate)Delegate.Remove(obj.pathCallback, new OnPathDelegate(OnPathComplete));
	}

	public void GetRemainingPath(List<Vector3> buffer, out bool stale)
	{
		buffer.Clear();
		if (!interpolator.valid)
		{
			buffer.Add(position);
			stale = true;
		}
		else
		{
			stale = false;
			interpolator.GetRemainingPath(buffer);
			buffer[0] = position;
		}
	}

	public void Teleport(Vector3 position, bool clearPath = true)
	{
		if (clearPath)
		{
			ClearPath();
		}
		simulatedPosition = (previousPosition1 = (previousPosition2 = position));
		if (updatePosition)
		{
			tr.position = position;
		}
		reachedEndOfPath = false;
		if (clearPath)
		{
			SearchPath();
		}
	}

	[Obsolete("Use SearchPath instead")]
	public virtual void ForceSearchPath()
	{
		SearchPath();
	}

	public virtual void SearchPath()
	{
		if (!float.IsPositiveInfinity(destination.x))
		{
			if (onSearchPath != null)
			{
				onSearchPath();
			}
			Vector3 feetPosition = GetFeetPosition();
			canSearchAgain = false;
			SetPath(ABPath.Construct(feetPosition, destination), updateDestinationFromPath: false);
		}
	}

	public virtual void OnTargetReached()
	{
	}

	protected virtual void OnPathComplete(Path _p)
	{
		if (!(_p is ABPath aBPath))
		{
			throw new Exception("This function only handles ABPaths, do not use special path types");
		}
		canSearchAgain = true;
		aBPath.Claim(this);
		if (aBPath.error)
		{
			aBPath.Release(this);
			return;
		}
		if (interpolatePathSwitches)
		{
			ConfigurePathSwitchInterpolation();
		}
		ABPath aBPath2 = path;
		path = aBPath;
		reachedEndOfPath = false;
		if (path is RandomPath randomPath)
		{
			destination = randomPath.originalEndPoint;
		}
		else if (path is MultiTargetPath multiTargetPath)
		{
			destination = multiTargetPath.originalEndPoint;
		}
		if (path.vectorPath != null && path.vectorPath.Count == 1)
		{
			path.vectorPath.Insert(0, GetFeetPosition());
		}
		ConfigureNewPath();
		aBPath2?.Release(this);
		if (interpolator.remainingDistance < 0.0001f && !reachedEndOfPath)
		{
			reachedEndOfPath = true;
			OnTargetReached();
		}
	}

	protected virtual void ClearPath()
	{
		if (seeker != null)
		{
			seeker.CancelCurrentPathRequest();
		}
		canSearchAgain = true;
		reachedEndOfPath = false;
		if (path != null)
		{
			path.Release(this);
		}
		path = null;
		interpolator.SetPath(null);
	}

	public void SetPath(Path path, bool updateDestinationFromPath = true)
	{
		if (updateDestinationFromPath && path is ABPath aBPath && !(path is RandomPath))
		{
			destination = aBPath.originalEndPoint;
		}
		if (path == null)
		{
			ClearPath();
			return;
		}
		if (path.PipelineState == PathState.Created)
		{
			canSearchAgain = false;
			seeker.CancelCurrentPathRequest();
			seeker.StartPath(path);
			autoRepath.DidRecalculatePath(destination);
			return;
		}
		if (path.PipelineState == PathState.Returned)
		{
			if (seeker.GetCurrentPath() != path)
			{
				seeker.CancelCurrentPathRequest();
				OnPathComplete(path);
				return;
			}
			throw new ArgumentException("If you calculate the path using seeker.StartPath then this script will pick up the calculated path anyway as it listens for all paths the Seeker finishes calculating. You should not call SetPath in that case.");
		}
		throw new ArgumentException("You must call the SetPath method with a path that either has been completely calculated or one whose path calculation has not been started at all. It looks like the path calculation for the path you tried to use has been started, but is not yet finished.");
	}

	protected virtual void ConfigurePathSwitchInterpolation()
	{
		bool flag = interpolator.valid && interpolator.remainingDistance < 0.0001f;
		if (interpolator.valid && !flag)
		{
			previousMovementOrigin = interpolator.position;
			previousMovementDirection = interpolator.tangent.normalized * interpolator.remainingDistance;
			pathSwitchInterpolationTime = 0f;
		}
		else
		{
			previousMovementOrigin = Vector3.zero;
			previousMovementDirection = Vector3.zero;
			pathSwitchInterpolationTime = float.PositiveInfinity;
		}
	}

	public virtual Vector3 GetFeetPosition()
	{
		return position;
	}

	protected virtual void ConfigureNewPath()
	{
		bool valid = interpolator.valid;
		Vector3 vector = (valid ? interpolator.tangent : Vector3.zero);
		interpolator.SetPath(path.vectorPath);
		interpolator.MoveToClosestPoint(GetFeetPosition());
		if (interpolatePathSwitches && switchPathInterpolationSpeed > 0.01f && valid)
		{
			float num = Mathf.Max(0f - Vector3.Dot(vector.normalized, interpolator.tangent.normalized), 0f);
			interpolator.distance -= speed * num * (1f / switchPathInterpolationSpeed);
		}
	}

	protected virtual void Update()
	{
		if (shouldRecalculatePath)
		{
			SearchPath();
		}
		if (canMove)
		{
			MovementUpdate(Time.deltaTime, out var nextPosition, out var nextRotation);
			FinalizeMovement(nextPosition, nextRotation);
		}
	}

	public void MovementUpdate(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
	{
		if (updatePosition)
		{
			simulatedPosition = tr.position;
		}
		if (updateRotation)
		{
			simulatedRotation = tr.rotation;
		}
		nextPosition = CalculateNextPosition(out var direction, isStopped ? 0f : deltaTime);
		if (enableRotation)
		{
			nextRotation = SimulateRotationTowards(direction, deltaTime);
		}
		else
		{
			nextRotation = simulatedRotation;
		}
	}

	public void FinalizeMovement(Vector3 nextPosition, Quaternion nextRotation)
	{
		previousPosition2 = previousPosition1;
		previousPosition1 = (simulatedPosition = nextPosition);
		simulatedRotation = nextRotation;
		if (updatePosition)
		{
			tr.position = nextPosition;
		}
		if (updateRotation)
		{
			tr.rotation = nextRotation;
		}
	}

	private Quaternion SimulateRotationTowards(Vector3 direction, float deltaTime)
	{
		if (direction != Vector3.zero)
		{
			Quaternion b = Quaternion.LookRotation(direction, (orientation == OrientationMode.YAxisForward) ? Vector3.back : Vector3.up);
			if (orientation == OrientationMode.YAxisForward)
			{
				b *= Quaternion.Euler(90f, 0f, 0f);
			}
			return Quaternion.Slerp(simulatedRotation, b, deltaTime * rotationSpeed);
		}
		return simulatedRotation;
	}

	protected virtual Vector3 CalculateNextPosition(out Vector3 direction, float deltaTime)
	{
		if (!interpolator.valid)
		{
			direction = Vector3.zero;
			return simulatedPosition;
		}
		interpolator.distance += deltaTime * speed;
		if (interpolator.remainingDistance < 0.0001f && !reachedEndOfPath)
		{
			reachedEndOfPath = true;
			OnTargetReached();
		}
		direction = interpolator.tangent;
		pathSwitchInterpolationTime += deltaTime;
		float num = switchPathInterpolationSpeed * pathSwitchInterpolationTime;
		if (interpolatePathSwitches && num < 1f)
		{
			return Vector3.Lerp(previousMovementOrigin + Vector3.ClampMagnitude(previousMovementDirection, speed * pathSwitchInterpolationTime), interpolator.position, num);
		}
		return interpolator.position;
	}

	protected override int OnUpgradeSerializedData(int version, bool unityThread)
	{
		if (unityThread && targetCompatibility != null)
		{
			target = targetCompatibility;
		}
		if (version <= 3)
		{
			repathRate = repathRateCompatibility;
			canSearch = canSearchCompability;
		}
		return 4;
	}

	public virtual void OnDrawGizmos()
	{
		tr = base.transform;
		autoRepath.DrawGizmos(position, 0f);
	}
}

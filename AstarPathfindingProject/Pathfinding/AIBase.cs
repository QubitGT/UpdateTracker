using System;
using Pathfinding.RVO;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pathfinding;

[RequireComponent(typeof(Seeker))]
public abstract class AIBase : VersionedMonoBehaviour
{
	public float radius = 0.5f;

	public float height = 2f;

	public bool canMove = true;

	[FormerlySerializedAs("speed")]
	public float maxSpeed = 1f;

	public Vector3 gravity = new Vector3(float.NaN, float.NaN, float.NaN);

	public LayerMask groundMask = -1;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("centerOffset")]
	private float centerOffsetCompatibility = float.NaN;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("repathRate")]
	private float repathRateCompatibility = float.NaN;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("canSearch")]
	[FormerlySerializedAs("repeatedlySearchPaths")]
	private bool canSearchCompability;

	[FormerlySerializedAs("rotationIn2D")]
	public OrientationMode orientation;

	public bool enableRotation = true;

	protected Vector3 simulatedPosition;

	protected Quaternion simulatedRotation;

	private Vector3 accumulatedMovementDelta = Vector3.zero;

	protected Vector2 velocity2D;

	protected float verticalVelocity;

	protected Seeker seeker;

	protected Transform tr;

	protected Rigidbody rigid;

	protected Rigidbody2D rigid2D;

	protected CharacterController controller;

	protected RVOController rvoController;

	public IMovementPlane movementPlane = GraphTransform.identityTransform;

	[NonSerialized]
	public bool updatePosition = true;

	[NonSerialized]
	public bool updateRotation = true;

	public AutoRepathPolicy autoRepath = new AutoRepathPolicy();

	protected float lastDeltaTime;

	protected int prevFrame;

	protected Vector3 prevPosition1;

	protected Vector3 prevPosition2;

	protected Vector2 lastDeltaPosition;

	protected bool waitingForPathCalculation;

	[FormerlySerializedAs("target")]
	[SerializeField]
	[HideInInspector]
	private Transform targetCompatibility;

	private bool startHasRun;

	public static readonly Color ShapeGizmoColor = new Color(0.9411765f, 71f / 85f, 0.11764706f);

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
			if (value)
			{
				if (autoRepath.mode == AutoRepathPolicy.Mode.Never)
				{
					autoRepath.mode = AutoRepathPolicy.Mode.EveryNSeconds;
				}
			}
			else
			{
				autoRepath.mode = AutoRepathPolicy.Mode.Never;
			}
		}
	}

	[Obsolete("Use the height property instead (2x this value)")]
	public float centerOffset
	{
		get
		{
			return height * 0.5f;
		}
		set
		{
			height = value * 2f;
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

	protected bool usingGravity { get; set; }

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

	public Vector3 destination { get; set; }

	public Vector3 velocity
	{
		get
		{
			if (!(lastDeltaTime > 1E-06f))
			{
				return Vector3.zero;
			}
			return (prevPosition1 - prevPosition2) / lastDeltaTime;
		}
	}

	public Vector3 desiredVelocity
	{
		get
		{
			if (!(lastDeltaTime > 1E-05f))
			{
				return Vector3.zero;
			}
			return movementPlane.ToWorld(lastDeltaPosition / lastDeltaTime, verticalVelocity);
		}
	}

	public bool isStopped { get; set; }

	public Action onSearchPath { get; set; }

	protected virtual bool shouldRecalculatePath
	{
		get
		{
			if (!waitingForPathCalculation)
			{
				return autoRepath.ShouldRecalculatePath(position, radius, destination);
			}
			return false;
		}
	}

	protected AIBase()
	{
		destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
	}

	public virtual void FindComponents()
	{
		tr = base.transform;
		seeker = GetComponent<Seeker>();
		rvoController = GetComponent<RVOController>();
		controller = GetComponent<CharacterController>();
		rigid = GetComponent<Rigidbody>();
		rigid2D = GetComponent<Rigidbody2D>();
	}

	protected virtual void OnEnable()
	{
		FindComponents();
		Seeker obj = seeker;
		obj.pathCallback = (OnPathDelegate)Delegate.Combine(obj.pathCallback, new OnPathDelegate(OnPathComplete));
		Init();
	}

	protected virtual void Start()
	{
		startHasRun = true;
		Init();
	}

	private void Init()
	{
		if (startHasRun)
		{
			if (canMove)
			{
				Teleport(position, clearPath: false);
			}
			autoRepath.Reset();
			if (shouldRecalculatePath)
			{
				SearchPath();
			}
		}
	}

	public virtual void Teleport(Vector3 newPosition, bool clearPath = true)
	{
		if (clearPath)
		{
			ClearPath();
		}
		prevPosition1 = (prevPosition2 = (simulatedPosition = newPosition));
		if (updatePosition)
		{
			tr.position = newPosition;
		}
		if (rvoController != null)
		{
			rvoController.Move(Vector3.zero);
		}
		if (clearPath)
		{
			SearchPath();
		}
	}

	protected void CancelCurrentPathRequest()
	{
		waitingForPathCalculation = false;
		if (seeker != null)
		{
			seeker.CancelCurrentPathRequest();
		}
	}

	protected virtual void OnDisable()
	{
		ClearPath();
		Seeker obj = seeker;
		obj.pathCallback = (OnPathDelegate)Delegate.Remove(obj.pathCallback, new OnPathDelegate(OnPathComplete));
		velocity2D = Vector3.zero;
		accumulatedMovementDelta = Vector3.zero;
		verticalVelocity = 0f;
		lastDeltaTime = 0f;
	}

	protected virtual void Update()
	{
		if (shouldRecalculatePath)
		{
			SearchPath();
		}
		usingGravity = !(gravity == Vector3.zero) && (!updatePosition || ((rigid == null || rigid.isKinematic) && (rigid2D == null || rigid2D.isKinematic)));
		if (rigid == null && rigid2D == null && canMove)
		{
			MovementUpdate(Time.deltaTime, out var nextPosition, out var nextRotation);
			FinalizeMovement(nextPosition, nextRotation);
		}
	}

	protected virtual void FixedUpdate()
	{
		if ((!(rigid == null) || !(rigid2D == null)) && canMove)
		{
			MovementUpdate(Time.fixedDeltaTime, out var nextPosition, out var nextRotation);
			FinalizeMovement(nextPosition, nextRotation);
		}
	}

	public void MovementUpdate(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
	{
		lastDeltaTime = deltaTime;
		MovementUpdateInternal(deltaTime, out nextPosition, out nextRotation);
	}

	protected abstract void MovementUpdateInternal(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation);

	protected virtual void CalculatePathRequestEndpoints(out Vector3 start, out Vector3 end)
	{
		start = GetFeetPosition();
		end = destination;
	}

	public virtual void SearchPath()
	{
		if (!float.IsPositiveInfinity(destination.x))
		{
			if (onSearchPath != null)
			{
				onSearchPath();
			}
			CalculatePathRequestEndpoints(out var start, out var end);
			ABPath path = ABPath.Construct(start, end);
			SetPath(path, updateDestinationFromPath: false);
		}
	}

	public virtual Vector3 GetFeetPosition()
	{
		return position;
	}

	protected abstract void OnPathComplete(Path newPath);

	protected abstract void ClearPath();

	public void SetPath(Path path, bool updateDestinationFromPath = true)
	{
		if (updateDestinationFromPath && path is ABPath aBPath && !(path is RandomPath))
		{
			destination = aBPath.originalEndPoint;
		}
		if (path == null)
		{
			CancelCurrentPathRequest();
			ClearPath();
			return;
		}
		if (path.PipelineState == PathState.Created)
		{
			waitingForPathCalculation = true;
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

	protected void ApplyGravity(float deltaTime)
	{
		if (usingGravity)
		{
			velocity2D += movementPlane.ToPlane(deltaTime * (float.IsNaN(gravity.x) ? Physics.gravity : gravity), out var elevation);
			verticalVelocity += elevation;
		}
		else
		{
			verticalVelocity = 0f;
		}
	}

	protected Vector2 CalculateDeltaToMoveThisFrame(Vector2 position, float distanceToEndOfPath, float deltaTime)
	{
		if (rvoController != null && rvoController.enabled)
		{
			return movementPlane.ToPlane(rvoController.CalculateMovementDelta(movementPlane.ToWorld(position), deltaTime));
		}
		return Vector2.ClampMagnitude(velocity2D * deltaTime, distanceToEndOfPath);
	}

	public Quaternion SimulateRotationTowards(Vector3 direction, float maxDegrees)
	{
		return SimulateRotationTowards(movementPlane.ToPlane(direction), maxDegrees);
	}

	protected Quaternion SimulateRotationTowards(Vector2 direction, float maxDegrees)
	{
		if (direction != Vector2.zero)
		{
			Quaternion to = Quaternion.LookRotation(movementPlane.ToWorld(direction), movementPlane.ToWorld(Vector2.zero, 1f));
			if (orientation == OrientationMode.YAxisForward)
			{
				to *= Quaternion.Euler(90f, 0f, 0f);
			}
			return Quaternion.RotateTowards(simulatedRotation, to, maxDegrees);
		}
		return simulatedRotation;
	}

	public virtual void Move(Vector3 deltaPosition)
	{
		accumulatedMovementDelta += deltaPosition;
	}

	public virtual void FinalizeMovement(Vector3 nextPosition, Quaternion nextRotation)
	{
		if (enableRotation)
		{
			FinalizeRotation(nextRotation);
		}
		FinalizePosition(nextPosition);
	}

	private void FinalizeRotation(Quaternion nextRotation)
	{
		simulatedRotation = nextRotation;
		if (updateRotation)
		{
			if (rigid != null)
			{
				rigid.MoveRotation(nextRotation);
			}
			else if (rigid2D != null)
			{
				rigid2D.MoveRotation(nextRotation.eulerAngles.z);
			}
			else
			{
				tr.rotation = nextRotation;
			}
		}
	}

	private void FinalizePosition(Vector3 nextPosition)
	{
		Vector3 vector = simulatedPosition;
		bool flag = false;
		if (controller != null && controller.enabled && updatePosition)
		{
			tr.position = vector;
			controller.Move(nextPosition - vector + accumulatedMovementDelta);
			vector = tr.position;
			if (controller.isGrounded)
			{
				verticalVelocity = 0f;
			}
		}
		else
		{
			movementPlane.ToPlane(vector, out var elevation);
			vector = nextPosition + accumulatedMovementDelta;
			if (usingGravity)
			{
				vector = RaycastPosition(vector, elevation);
			}
			flag = true;
		}
		bool positionChanged = false;
		vector = ClampToNavmesh(vector, out positionChanged);
		if ((flag || positionChanged) && updatePosition)
		{
			if (rigid != null)
			{
				rigid.MovePosition(vector);
			}
			else if (rigid2D != null)
			{
				rigid2D.MovePosition(vector);
			}
			else
			{
				tr.position = vector;
			}
		}
		accumulatedMovementDelta = Vector3.zero;
		simulatedPosition = vector;
		UpdateVelocity();
	}

	protected void UpdateVelocity()
	{
		int frameCount = Time.frameCount;
		if (frameCount != prevFrame)
		{
			prevPosition2 = prevPosition1;
		}
		prevPosition1 = position;
		prevFrame = frameCount;
	}

	protected virtual Vector3 ClampToNavmesh(Vector3 position, out bool positionChanged)
	{
		positionChanged = false;
		return position;
	}

	protected Vector3 RaycastPosition(Vector3 position, float lastElevation)
	{
		movementPlane.ToPlane(position, out var elevation);
		float num = tr.localScale.y * height * 0.5f + Mathf.Max(0f, lastElevation - elevation);
		Vector3 vector = movementPlane.ToWorld(Vector2.zero, num);
		if (Physics.Raycast(position + vector, -vector, out var hitInfo, num, groundMask, QueryTriggerInteraction.Ignore))
		{
			verticalVelocity *= Math.Max(0f, 1f - 5f * lastDeltaTime);
			return hitInfo.point;
		}
		return position;
	}

	protected virtual void OnDrawGizmosSelected()
	{
		if (Application.isPlaying)
		{
			FindComponents();
		}
	}

	protected virtual void OnDrawGizmos()
	{
		if (!Application.isPlaying || !base.enabled)
		{
			FindComponents();
		}
		Color shapeGizmoColor = ShapeGizmoColor;
		if (rvoController != null && rvoController.locked)
		{
			shapeGizmoColor *= 0.5f;
		}
		if (orientation == OrientationMode.YAxisForward)
		{
			Draw.Gizmos.Cylinder(position, Vector3.forward, 0f, radius * tr.localScale.x, shapeGizmoColor);
		}
		else
		{
			Draw.Gizmos.Cylinder(position, rotation * Vector3.up, tr.localScale.y * height, radius * tr.localScale.x, shapeGizmoColor);
		}
		if (!float.IsPositiveInfinity(destination.x) && Application.isPlaying)
		{
			Draw.Gizmos.CircleXZ(destination, 0.2f, Color.blue);
		}
		autoRepath.DrawGizmos(position, radius);
	}

	protected override void Reset()
	{
		ResetShape();
		base.Reset();
	}

	private void ResetShape()
	{
		CharacterController component = GetComponent<CharacterController>();
		if (component != null)
		{
			radius = component.radius;
			height = Mathf.Max(radius * 2f, component.height);
		}
	}

	protected override int OnUpgradeSerializedData(int version, bool unityThread)
	{
		if (unityThread && !float.IsNaN(centerOffsetCompatibility))
		{
			height = centerOffsetCompatibility * 2f;
			ResetShape();
			RVOController component = GetComponent<RVOController>();
			if (component != null)
			{
				radius = component.radiusBackingField;
			}
			centerOffsetCompatibility = float.NaN;
		}
		if (unityThread && targetCompatibility != null)
		{
			target = targetCompatibility;
		}
		if (version <= 3)
		{
			repathRate = repathRateCompatibility;
			canSearch = canSearchCompability;
		}
		return 5;
	}
}

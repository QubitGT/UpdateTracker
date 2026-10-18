using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pathfinding;

[AddComponentMenu("Pathfinding/AI/AIPath (2D,3D)")]
public class AIPath : AIBase, IAstarAI
{
	public float maxAcceleration = -2.5f;

	[FormerlySerializedAs("turningSpeed")]
	public float rotationSpeed = 360f;

	public float slowdownDistance = 0.6f;

	public float pickNextWaypointDist = 2f;

	public float endReachedDistance = 0.2f;

	public bool alwaysDrawGizmos;

	public bool slowWhenNotFacingTarget = true;

	public CloseToDestinationMode whenCloseToDestination;

	public bool constrainInsideGraph;

	protected Path path;

	protected PathInterpolator interpolator = new PathInterpolator();

	private static NNConstraint cachedNNConstraint = NNConstraint.Default;

	public float remainingDistance
	{
		get
		{
			if (!interpolator.valid)
			{
				return float.PositiveInfinity;
			}
			return interpolator.remainingDistance + movementPlane.ToPlane(interpolator.position - base.position).magnitude;
		}
	}

	public bool reachedDestination
	{
		get
		{
			if (!reachedEndOfPath)
			{
				return false;
			}
			if (!interpolator.valid || remainingDistance + movementPlane.ToPlane(base.destination - interpolator.endPoint).magnitude > endReachedDistance)
			{
				return false;
			}
			if (orientation != OrientationMode.YAxisForward)
			{
				movementPlane.ToPlane(base.destination - base.position, out var elevation);
				float num = tr.localScale.y * height;
				if (elevation > num || (double)elevation < (double)(0f - num) * 0.5)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool reachedEndOfPath { get; protected set; }

	public bool hasPath => interpolator.valid;

	public bool pathPending => waitingForPathCalculation;

	public Vector3 steeringTarget
	{
		get
		{
			if (!interpolator.valid)
			{
				return base.position;
			}
			return interpolator.position;
		}
	}

	float IAstarAI.radius
	{
		get
		{
			return radius;
		}
		set
		{
			radius = value;
		}
	}

	float IAstarAI.height
	{
		get
		{
			return height;
		}
		set
		{
			height = value;
		}
	}

	float IAstarAI.maxSpeed
	{
		get
		{
			return maxSpeed;
		}
		set
		{
			maxSpeed = value;
		}
	}

	bool IAstarAI.canSearch
	{
		get
		{
			return base.canSearch;
		}
		set
		{
			base.canSearch = value;
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

	[Obsolete("When unifying the interfaces for different movement scripts, this property has been renamed to reachedEndOfPath.  [AstarUpgradable: 'TargetReached' -> 'reachedEndOfPath']")]
	public bool TargetReached => reachedEndOfPath;

	[Obsolete("This field has been renamed to #rotationSpeed and is now in degrees per second instead of a damping factor")]
	public float turningSpeed
	{
		get
		{
			return rotationSpeed / 90f;
		}
		set
		{
			rotationSpeed = value * 90f;
		}
	}

	[Obsolete("This member has been deprecated. Use 'maxSpeed' instead. [AstarUpgradable: 'speed' -> 'maxSpeed']")]
	public float speed
	{
		get
		{
			return maxSpeed;
		}
		set
		{
			maxSpeed = value;
		}
	}

	[Obsolete("Only exists for compatibility reasons. Use desiredVelocity or steeringTarget instead.")]
	public Vector3 targetDirection => (steeringTarget - tr.position).normalized;

	public override void Teleport(Vector3 newPosition, bool clearPath = true)
	{
		reachedEndOfPath = false;
		base.Teleport(newPosition, clearPath);
	}

	public void GetRemainingPath(List<Vector3> buffer, out bool stale)
	{
		buffer.Clear();
		buffer.Add(base.position);
		if (!interpolator.valid)
		{
			stale = true;
			return;
		}
		stale = false;
		interpolator.GetRemainingPath(buffer);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (path != null)
		{
			path.Release(this);
		}
		path = null;
		interpolator.SetPath(null);
		reachedEndOfPath = false;
	}

	public virtual void OnTargetReached()
	{
	}

	protected override void OnPathComplete(Path newPath)
	{
		if (!(newPath is ABPath aBPath))
		{
			throw new Exception("This function only handles ABPaths, do not use special path types");
		}
		waitingForPathCalculation = false;
		aBPath.Claim(this);
		if (aBPath.error)
		{
			aBPath.Release(this);
			SetPath(null);
			return;
		}
		if (path != null)
		{
			path.Release(this);
		}
		path = aBPath;
		if (path is RandomPath randomPath)
		{
			base.destination = randomPath.originalEndPoint;
		}
		else if (path is MultiTargetPath multiTargetPath)
		{
			base.destination = multiTargetPath.originalEndPoint;
		}
		if (path.vectorPath.Count == 1)
		{
			path.vectorPath.Add(path.vectorPath[0]);
		}
		interpolator.SetPath(path.vectorPath);
		ITransformedGraph transformedGraph = ((path.path.Count > 0) ? (AstarData.GetGraph(path.path[0]) as ITransformedGraph) : null);
		movementPlane = ((transformedGraph != null) ? transformedGraph.transform : ((orientation == OrientationMode.YAxisForward) ? new GraphTransform(Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90f, 270f, 90f), Vector3.one)) : GraphTransform.identityTransform));
		reachedEndOfPath = false;
		interpolator.MoveToLocallyClosestPoint((GetFeetPosition() + aBPath.originalStartPoint) * 0.5f);
		interpolator.MoveToLocallyClosestPoint(GetFeetPosition());
		interpolator.MoveToCircleIntersection2D(base.position, pickNextWaypointDist, movementPlane);
		if (remainingDistance <= endReachedDistance)
		{
			reachedEndOfPath = true;
			OnTargetReached();
		}
	}

	protected override void ClearPath()
	{
		CancelCurrentPathRequest();
		if (path != null)
		{
			path.Release(this);
		}
		path = null;
		interpolator.SetPath(null);
		reachedEndOfPath = false;
	}

	protected override void MovementUpdateInternal(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation)
	{
		float num = maxAcceleration;
		if (num < 0f)
		{
			num *= 0f - maxSpeed;
		}
		if (updatePosition)
		{
			simulatedPosition = tr.position;
		}
		if (updateRotation)
		{
			simulatedRotation = tr.rotation;
		}
		Vector3 vector = simulatedPosition;
		interpolator.MoveToCircleIntersection2D(vector, pickNextWaypointDist, movementPlane);
		Vector2 deltaPosition = movementPlane.ToPlane(steeringTarget - vector);
		float num2 = deltaPosition.magnitude + Mathf.Max(0f, interpolator.remainingDistance);
		bool num3 = reachedEndOfPath;
		reachedEndOfPath = num2 <= endReachedDistance && interpolator.valid;
		if (!num3 && reachedEndOfPath)
		{
			OnTargetReached();
		}
		Vector2 vector2 = movementPlane.ToPlane(simulatedRotation * ((orientation == OrientationMode.YAxisForward) ? Vector3.up : Vector3.forward));
		bool flag = base.isStopped || (reachedDestination && whenCloseToDestination == CloseToDestinationMode.Stop);
		float num4;
		if (interpolator.valid && !flag)
		{
			num4 = ((num2 < slowdownDistance) ? Mathf.Sqrt(num2 / slowdownDistance) : 1f);
			if (reachedEndOfPath && whenCloseToDestination == CloseToDestinationMode.Stop)
			{
				velocity2D -= Vector2.ClampMagnitude(velocity2D, num * deltaTime);
			}
			else
			{
				velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(deltaPosition, deltaPosition.normalized * maxSpeed, velocity2D, num, rotationSpeed, maxSpeed, vector2) * deltaTime;
			}
		}
		else
		{
			num4 = 1f;
			velocity2D -= Vector2.ClampMagnitude(velocity2D, num * deltaTime);
		}
		velocity2D = MovementUtilities.ClampVelocity(velocity2D, maxSpeed, num4, slowWhenNotFacingTarget && enableRotation, vector2);
		ApplyGravity(deltaTime);
		if (rvoController != null && rvoController.enabled)
		{
			Vector3 pos = vector + movementPlane.ToWorld(Vector2.ClampMagnitude(velocity2D, num2));
			rvoController.SetTarget(pos, velocity2D.magnitude, maxSpeed);
		}
		Vector2 p = (lastDeltaPosition = CalculateDeltaToMoveThisFrame(movementPlane.ToPlane(vector), num2, deltaTime));
		nextPosition = vector + movementPlane.ToWorld(p, verticalVelocity * lastDeltaTime);
		CalculateNextRotation(num4, out nextRotation);
	}

	protected virtual void CalculateNextRotation(float slowdown, out Quaternion nextRotation)
	{
		if (lastDeltaTime > 1E-05f && enableRotation)
		{
			Vector2 direction;
			if (rvoController != null && rvoController.enabled)
			{
				Vector2 b = lastDeltaPosition / lastDeltaTime;
				direction = Vector2.Lerp(velocity2D, b, 4f * b.magnitude / (maxSpeed + 0.0001f));
			}
			else
			{
				direction = velocity2D;
			}
			float num = rotationSpeed * Mathf.Max(0f, (slowdown - 0.3f) / 0.7f);
			nextRotation = SimulateRotationTowards(direction, num * lastDeltaTime);
		}
		else
		{
			nextRotation = base.rotation;
		}
	}

	protected override Vector3 ClampToNavmesh(Vector3 position, out bool positionChanged)
	{
		if (constrainInsideGraph)
		{
			cachedNNConstraint.tags = seeker.traversableTags;
			cachedNNConstraint.graphMask = seeker.graphMask;
			cachedNNConstraint.distanceXZ = true;
			Vector3 vector = AstarPath.active.GetNearest(position, cachedNNConstraint).position;
			Vector2 vector2 = movementPlane.ToPlane(vector - position);
			float sqrMagnitude = vector2.sqrMagnitude;
			if (sqrMagnitude > 1.0000001E-06f)
			{
				velocity2D -= vector2 * Vector2.Dot(vector2, velocity2D) / sqrMagnitude;
				if (rvoController != null && rvoController.enabled)
				{
					rvoController.SetCollisionNormal(vector2);
				}
				positionChanged = true;
				return position + movementPlane.ToWorld(vector2);
			}
		}
		positionChanged = false;
		return position;
	}

	protected override int OnUpgradeSerializedData(int version, bool unityThread)
	{
		if (version < 1)
		{
			rotationSpeed *= 90f;
		}
		return base.OnUpgradeSerializedData(version, unityThread);
	}

	[Obsolete("This method no longer calculates the velocity. Use the desiredVelocity property instead")]
	public Vector3 CalculateVelocity(Vector3 position)
	{
		return base.desiredVelocity;
	}
}

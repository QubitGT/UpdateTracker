using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding;

public interface IAstarAI
{
	float radius { get; set; }

	float height { get; set; }

	Vector3 position { get; }

	Quaternion rotation { get; set; }

	float maxSpeed { get; set; }

	Vector3 velocity { get; }

	Vector3 desiredVelocity { get; }

	float remainingDistance { get; }

	bool reachedDestination { get; }

	bool reachedEndOfPath { get; }

	Vector3 destination { get; set; }

	bool canSearch { get; set; }

	bool canMove { get; set; }

	bool hasPath { get; }

	bool pathPending { get; }

	bool isStopped { get; set; }

	Vector3 steeringTarget { get; }

	Action onSearchPath { get; set; }

	void GetRemainingPath(List<Vector3> buffer, out bool stale);

	void SearchPath();

	void SetPath(Path path, bool updateDestinationFromPath = true);

	void Teleport(Vector3 newPosition, bool clearPath = true);

	void Move(Vector3 deltaPosition);

	void MovementUpdate(float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation);

	void FinalizeMovement(Vector3 nextPosition, Quaternion nextRotation);
}

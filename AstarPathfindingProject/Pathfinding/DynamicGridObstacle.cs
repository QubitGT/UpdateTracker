using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding;

[HelpURL("http://arongranberg.com/astar/documentation/stable/class_pathfinding_1_1_dynamic_grid_obstacle.php")]
public class DynamicGridObstacle : GraphModifier
{
	private Collider coll;

	private Collider2D coll2D;

	private Transform tr;

	public float updateError = 1f;

	public float checkTime = 0.2f;

	private Bounds prevBounds;

	private Quaternion prevRotation;

	private bool prevEnabled;

	private float lastCheckTime = -9999f;

	private Queue<GraphUpdateObject> pendingGraphUpdates = new Queue<GraphUpdateObject>();

	private Bounds bounds
	{
		get
		{
			if (coll != null)
			{
				return coll.bounds;
			}
			Bounds result = coll2D.bounds;
			result.extents += new Vector3(0f, 0f, 10000f);
			return result;
		}
	}

	private bool colliderEnabled
	{
		get
		{
			if (!(coll != null))
			{
				return coll2D.enabled;
			}
			return coll.enabled;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		coll = GetComponent<Collider>();
		coll2D = GetComponent<Collider2D>();
		tr = base.transform;
		if (coll == null && coll2D == null && Application.isPlaying)
		{
			throw new Exception("A collider or 2D collider must be attached to the GameObject(" + base.gameObject.name + ") for the DynamicGridObstacle to work");
		}
		prevBounds = bounds;
		prevRotation = tr.rotation;
		prevEnabled = false;
	}

	public override void OnPostScan()
	{
		if (coll == null)
		{
			Awake();
		}
		if (coll != null)
		{
			prevEnabled = colliderEnabled;
		}
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (coll == null && coll2D == null)
		{
			Debug.LogError("Removed collider from DynamicGridObstacle", this);
			base.enabled = false;
			return;
		}
		while (pendingGraphUpdates.Count > 0 && pendingGraphUpdates.Peek().stage != GraphUpdateStage.Pending)
		{
			pendingGraphUpdates.Dequeue();
		}
		if (AstarPath.active == null || AstarPath.active.isScanning || Time.realtimeSinceStartup - lastCheckTime < checkTime || !Application.isPlaying || pendingGraphUpdates.Count > 0)
		{
			return;
		}
		lastCheckTime = Time.realtimeSinceStartup;
		if (colliderEnabled)
		{
			Bounds bounds = this.bounds;
			Quaternion rotation = tr.rotation;
			Vector3 vector = prevBounds.min - bounds.min;
			Vector3 vector2 = prevBounds.max - bounds.max;
			float num = bounds.extents.magnitude * Quaternion.Angle(prevRotation, rotation) * (MathF.PI / 180f);
			if (vector.sqrMagnitude > updateError * updateError || vector2.sqrMagnitude > updateError * updateError || num > updateError || !prevEnabled)
			{
				DoUpdateGraphs();
			}
		}
		else if (prevEnabled)
		{
			DoUpdateGraphs();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (AstarPath.active != null && Application.isPlaying)
		{
			GraphUpdateObject graphUpdateObject = new GraphUpdateObject(prevBounds);
			pendingGraphUpdates.Enqueue(graphUpdateObject);
			AstarPath.active.UpdateGraphs(graphUpdateObject);
			prevEnabled = false;
		}
		pendingGraphUpdates.Clear();
	}

	public void DoUpdateGraphs()
	{
		if (coll == null && coll2D == null)
		{
			return;
		}
		Physics.SyncTransforms();
		Physics2D.SyncTransforms();
		if (!colliderEnabled)
		{
			GraphUpdateObject graphUpdateObject = new GraphUpdateObject(prevBounds);
			pendingGraphUpdates.Enqueue(graphUpdateObject);
			AstarPath.active.UpdateGraphs(graphUpdateObject);
		}
		else
		{
			Bounds bounds = this.bounds;
			Bounds b = bounds;
			b.Encapsulate(prevBounds);
			if (BoundsVolume(b) < BoundsVolume(bounds) + BoundsVolume(prevBounds))
			{
				GraphUpdateObject graphUpdateObject2 = new GraphUpdateObject(b);
				pendingGraphUpdates.Enqueue(graphUpdateObject2);
				AstarPath.active.UpdateGraphs(graphUpdateObject2);
			}
			else
			{
				GraphUpdateObject graphUpdateObject3 = new GraphUpdateObject(prevBounds);
				GraphUpdateObject graphUpdateObject4 = new GraphUpdateObject(bounds);
				pendingGraphUpdates.Enqueue(graphUpdateObject3);
				pendingGraphUpdates.Enqueue(graphUpdateObject4);
				AstarPath.active.UpdateGraphs(graphUpdateObject3);
				AstarPath.active.UpdateGraphs(graphUpdateObject4);
			}
			prevBounds = bounds;
		}
		prevEnabled = colliderEnabled;
		prevRotation = tr.rotation;
		lastCheckTime = Time.realtimeSinceStartup;
	}

	private static float BoundsVolume(Bounds b)
	{
		return Math.Abs(b.size.x * b.size.y * b.size.z);
	}
}

using System;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pathfinding;

[Serializable]
public class AutoRepathPolicy
{
	public enum Mode
	{
		Never,
		EveryNSeconds,
		Dynamic
	}

	public Mode mode = Mode.Dynamic;

	[FormerlySerializedAs("interval")]
	public float period = 0.5f;

	public float sensitivity = 10f;

	[FormerlySerializedAs("maximumInterval")]
	public float maximumPeriod = 2f;

	public bool visualizeSensitivity;

	private Vector3 lastDestination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

	private float lastRepathTime = float.NegativeInfinity;

	public virtual bool ShouldRecalculatePath(Vector3 position, float radius, Vector3 destination)
	{
		if (mode == Mode.Never || float.IsPositiveInfinity(destination.x))
		{
			return false;
		}
		float num = Time.time - lastRepathTime;
		if (mode == Mode.EveryNSeconds)
		{
			return num >= period;
		}
		float num2 = (destination - lastDestination).sqrMagnitude / Mathf.Max((position - lastDestination).sqrMagnitude, radius * radius) * (sensitivity * sensitivity);
		if (num2 > 1f || float.IsNaN(num2))
		{
			return true;
		}
		if (num >= maximumPeriod * (1f - Mathf.Sqrt(num2)))
		{
			return true;
		}
		return false;
	}

	public virtual void Reset()
	{
		lastRepathTime = float.NegativeInfinity;
	}

	public virtual void DidRecalculatePath(Vector3 destination)
	{
		lastRepathTime = Time.time;
		lastDestination = destination;
	}

	public void DrawGizmos(Vector3 position, float radius)
	{
		if (visualizeSensitivity && !float.IsPositiveInfinity(lastDestination.x))
		{
			float radius2 = Mathf.Sqrt(Mathf.Max((position - lastDestination).sqrMagnitude, radius * radius) / (sensitivity * sensitivity));
			Draw.Gizmos.CircleXZ(lastDestination, radius2, Color.magenta);
		}
	}
}

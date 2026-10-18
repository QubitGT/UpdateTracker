using System;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pathfinding.RVO;

[AddComponentMenu("Pathfinding/Local Avoidance/RVO Controller")]
[HelpURL("http://arongranberg.com/astar/documentation/stable/class_pathfinding_1_1_r_v_o_1_1_r_v_o_controller.php")]
public class RVOController : VersionedMonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("radius")]
	internal float radiusBackingField = 0.5f;

	[SerializeField]
	[FormerlySerializedAs("height")]
	private float heightBackingField = 2f;

	[SerializeField]
	[FormerlySerializedAs("center")]
	private float centerBackingField = 1f;

	[Tooltip("A locked unit cannot move. Other units will still avoid it. But avoidance quality is not the best")]
	public bool locked;

	[Tooltip("Automatically set #locked to true when desired velocity is approximately zero")]
	public bool lockWhenNotMoving;

	[Tooltip("How far into the future to look for collisions with other agents (in seconds)")]
	public float agentTimeHorizon = 2f;

	[Tooltip("How far into the future to look for collisions with obstacles (in seconds)")]
	public float obstacleTimeHorizon = 2f;

	[Tooltip("Max number of other agents to take into account.\nA smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.")]
	public int maxNeighbours = 10;

	public RVOLayer layer = RVOLayer.DefaultAgent;

	[EnumFlag]
	public RVOLayer collidesWith = (RVOLayer)(-1);

	[HideInInspector]
	[Obsolete]
	public float wallAvoidForce = 1f;

	[HideInInspector]
	[Obsolete]
	public float wallAvoidFalloff = 1f;

	[Tooltip("How strongly other agents will avoid this agent")]
	[Range(0f, 1f)]
	public float priority = 0.5f;

	protected Transform tr;

	[SerializeField]
	[FormerlySerializedAs("ai")]
	private IAstarAI aiBackingField;

	public bool debug;

	public float radius
	{
		get
		{
			if (ai != null)
			{
				return ai.radius;
			}
			return radiusBackingField;
		}
		set
		{
			if (ai != null)
			{
				ai.radius = value;
			}
			radiusBackingField = value;
		}
	}

	public float height
	{
		get
		{
			if (ai != null)
			{
				return ai.height;
			}
			return heightBackingField;
		}
		set
		{
			if (ai != null)
			{
				ai.height = value;
			}
			heightBackingField = value;
		}
	}

	public float center
	{
		get
		{
			if (ai != null)
			{
				return ai.height / 2f;
			}
			return centerBackingField;
		}
		set
		{
			centerBackingField = value;
		}
	}

	[Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
	public LayerMask mask
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	[Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
	public bool enableRotation
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	[Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
	public float rotationSpeed
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	[Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
	public float maxSpeed
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public MovementPlane movementPlane
	{
		get
		{
			if (simulator != null)
			{
				return simulator.movementPlane;
			}
			if ((bool)RVOSimulator.active)
			{
				return RVOSimulator.active.movementPlane;
			}
			return MovementPlane.XZ;
		}
	}

	public IAgent rvoAgent { get; private set; }

	public Simulator simulator { get; private set; }

	protected IAstarAI ai
	{
		get
		{
			if (aiBackingField as MonoBehaviour == null)
			{
				aiBackingField = null;
			}
			return aiBackingField;
		}
		set
		{
			aiBackingField = value;
		}
	}

	public Vector3 position => To3D(rvoAgent.Position, rvoAgent.ElevationCoordinate);

	public Vector3 velocity
	{
		get
		{
			float num = ((Time.deltaTime > 0.0001f) ? Time.deltaTime : 0.02f);
			return CalculateMovementDelta(num) / num;
		}
		set
		{
			rvoAgent.ForceSetVelocity(To2D(value));
		}
	}

	public Vector3 CalculateMovementDelta(float deltaTime)
	{
		if (rvoAgent == null)
		{
			return Vector3.zero;
		}
		return To3D(Vector2.ClampMagnitude(rvoAgent.CalculatedTargetPoint - To2D((ai != null) ? ai.position : tr.position), rvoAgent.CalculatedSpeed * deltaTime), 0f);
	}

	public Vector3 CalculateMovementDelta(Vector3 position, float deltaTime)
	{
		return To3D(Vector2.ClampMagnitude(rvoAgent.CalculatedTargetPoint - To2D(position), rvoAgent.CalculatedSpeed * deltaTime), 0f);
	}

	public void SetCollisionNormal(Vector3 normal)
	{
		rvoAgent.SetCollisionNormal(To2D(normal));
	}

	[Obsolete("Set the 'velocity' property instead")]
	public void ForceSetVelocity(Vector3 velocity)
	{
		this.velocity = velocity;
	}

	public Vector2 To2D(Vector3 p)
	{
		float elevation;
		return To2D(p, out elevation);
	}

	public Vector2 To2D(Vector3 p, out float elevation)
	{
		if (movementPlane == MovementPlane.XY)
		{
			elevation = 0f - p.z;
			return new Vector2(p.x, p.y);
		}
		elevation = p.y;
		return new Vector2(p.x, p.z);
	}

	public Vector3 To3D(Vector2 p, float elevationCoordinate)
	{
		if (movementPlane == MovementPlane.XY)
		{
			return new Vector3(p.x, p.y, 0f - elevationCoordinate);
		}
		return new Vector3(p.x, elevationCoordinate, p.y);
	}

	private void OnDisable()
	{
		if (simulator != null)
		{
			simulator.RemoveAgent(rvoAgent);
		}
	}

	private void OnEnable()
	{
		tr = base.transform;
		ai = GetComponent<IAstarAI>();
		AIBase aIBase = ai as AIBase;
		if (aIBase != null)
		{
			aIBase.FindComponents();
		}
		if (RVOSimulator.active == null)
		{
			Debug.LogError("No RVOSimulator component found in the scene. Please add one.");
			base.enabled = false;
			return;
		}
		simulator = RVOSimulator.active.GetSimulator();
		if (rvoAgent != null)
		{
			simulator.AddAgent(rvoAgent);
			return;
		}
		rvoAgent = simulator.AddAgent(Vector2.zero, 0f);
		rvoAgent.PreCalculationCallback = UpdateAgentProperties;
	}

	protected void UpdateAgentProperties()
	{
		Vector3 localScale = tr.localScale;
		rvoAgent.Radius = Mathf.Max(0.001f, radius * localScale.x);
		rvoAgent.AgentTimeHorizon = agentTimeHorizon;
		rvoAgent.ObstacleTimeHorizon = obstacleTimeHorizon;
		rvoAgent.Locked = locked;
		rvoAgent.MaxNeighbours = maxNeighbours;
		rvoAgent.DebugDraw = debug;
		rvoAgent.Layer = layer;
		rvoAgent.CollidesWith = collidesWith;
		rvoAgent.Priority = priority;
		rvoAgent.Position = To2D((ai != null) ? ai.position : tr.position, out var elevation);
		if (movementPlane == MovementPlane.XZ)
		{
			rvoAgent.Height = height * localScale.y;
			rvoAgent.ElevationCoordinate = elevation + (center - 0.5f * height) * localScale.y;
		}
		else
		{
			rvoAgent.Height = 1f;
			rvoAgent.ElevationCoordinate = 0f;
		}
	}

	public void SetTarget(Vector3 pos, float speed, float maxSpeed)
	{
		if (simulator != null)
		{
			rvoAgent.SetTarget(To2D(pos), speed, maxSpeed);
			if (lockWhenNotMoving)
			{
				locked = speed < 0.001f;
			}
		}
	}

	public void Move(Vector3 vel)
	{
		if (simulator != null)
		{
			Vector2 vector = To2D(vel);
			float magnitude = vector.magnitude;
			rvoAgent.SetTarget(To2D((ai != null) ? ai.position : tr.position) + vector, magnitude, magnitude);
			if (lockWhenNotMoving)
			{
				locked = magnitude < 0.001f;
			}
		}
	}

	[Obsolete("Use transform.position instead, the RVOController can now handle that without any issues.")]
	public void Teleport(Vector3 pos)
	{
		tr.position = pos;
	}

	private void OnDrawGizmos()
	{
		tr = base.transform;
		if (ai == null)
		{
			Color color = AIBase.ShapeGizmoColor * (locked ? 0.5f : 1f);
			Vector3 vector = base.transform.position;
			Vector3 localScale = tr.localScale;
			if (movementPlane == MovementPlane.XY)
			{
				Draw.Gizmos.Cylinder(vector, Vector3.forward, 0f, radius * localScale.x, color);
			}
			else
			{
				Draw.Gizmos.Cylinder(vector + To3D(Vector2.zero, center - height * 0.5f) * localScale.y, To3D(Vector2.zero, 1f), height * localScale.y, radius * localScale.x, color);
			}
		}
	}

	protected override int OnUpgradeSerializedData(int version, bool unityThread)
	{
		if (version <= 1)
		{
			if (!unityThread)
			{
				return -1;
			}
			if (base.transform.localScale.y != 0f)
			{
				centerBackingField /= Mathf.Abs(base.transform.localScale.y);
			}
			if (base.transform.localScale.y != 0f)
			{
				heightBackingField /= Mathf.Abs(base.transform.localScale.y);
			}
			if (base.transform.localScale.x != 0f)
			{
				radiusBackingField /= Mathf.Abs(base.transform.localScale.x);
			}
		}
		return 2;
	}
}

using System;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[Serializable]
public class GraphCollision
{
	public ColliderType type = ColliderType.Capsule;

	public float diameter = 1f;

	public float height = 2f;

	public float collisionOffset;

	public RayDirection rayDirection = RayDirection.Both;

	public LayerMask mask;

	public LayerMask heightMask = -1;

	public float fromHeight = 100f;

	public bool thickRaycast;

	public float thickRaycastDiameter = 1f;

	public bool unwalkableWhenNoGround = true;

	public bool use2D;

	public bool collisionCheck = true;

	public bool heightCheck = true;

	public Vector3 up;

	private Vector3 upheight;

	private ContactFilter2D contactFilter;

	private static Collider2D[] dummyArray = new Collider2D[1];

	private float finalRadius;

	private float finalRaycastRadius;

	public const float RaycastErrorMargin = 0.005f;

	private RaycastHit[] hitBuffer = new RaycastHit[8];

	public void Initialize(GraphTransform transform, float scale)
	{
		up = (transform.Transform(Vector3.up) - transform.Transform(Vector3.zero)).normalized;
		upheight = up * height;
		finalRadius = diameter * scale * 0.5f;
		finalRaycastRadius = thickRaycastDiameter * scale * 0.5f;
		contactFilter = new ContactFilter2D
		{
			layerMask = mask,
			useDepth = false,
			useLayerMask = true,
			useNormalAngle = false,
			useTriggers = false
		};
	}

	public bool Check(Vector3 position)
	{
		if (!collisionCheck)
		{
			return true;
		}
		if (use2D)
		{
			ColliderType colliderType = type;
			if ((uint)colliderType <= 1u)
			{
				return Physics2D.OverlapCircle(position, finalRadius, contactFilter, dummyArray) == 0;
			}
			return Physics2D.OverlapPoint(position, contactFilter, dummyArray) == 0;
		}
		position += up * collisionOffset;
		switch (type)
		{
		case ColliderType.Capsule:
			return !Physics.CheckCapsule(position, position + upheight, finalRadius, mask, QueryTriggerInteraction.Ignore);
		case ColliderType.Sphere:
			return !Physics.CheckSphere(position, finalRadius, mask, QueryTriggerInteraction.Ignore);
		default:
			switch (rayDirection)
			{
			case RayDirection.Both:
				if (!Physics.Raycast(position, up, height, mask, QueryTriggerInteraction.Ignore))
				{
					return !Physics.Raycast(position + upheight, -up, height, mask, QueryTriggerInteraction.Ignore);
				}
				return false;
			case RayDirection.Up:
				return !Physics.Raycast(position, up, height, mask, QueryTriggerInteraction.Ignore);
			default:
				return !Physics.Raycast(position + upheight, -up, height, mask, QueryTriggerInteraction.Ignore);
			}
		}
	}

	public Vector3 CheckHeight(Vector3 position)
	{
		RaycastHit hit;
		bool walkable;
		return CheckHeight(position, out hit, out walkable);
	}

	public Vector3 CheckHeight(Vector3 position, out RaycastHit hit, out bool walkable)
	{
		walkable = true;
		if (!heightCheck || use2D)
		{
			hit = default(RaycastHit);
			return position;
		}
		if (thickRaycast)
		{
			Ray ray = new Ray(position + up * fromHeight, -up);
			if (Physics.SphereCast(ray, finalRaycastRadius, out hit, fromHeight + 0.005f, heightMask, QueryTriggerInteraction.Ignore))
			{
				return VectorMath.ClosestPointOnLine(ray.origin, ray.origin + ray.direction, hit.point);
			}
			walkable &= !unwalkableWhenNoGround;
		}
		else
		{
			if (Physics.Raycast(position + up * fromHeight, -up, out hit, fromHeight + 0.005f, heightMask, QueryTriggerInteraction.Ignore))
			{
				return hit.point;
			}
			walkable &= !unwalkableWhenNoGround;
		}
		return position;
	}

	public RaycastHit[] CheckHeightAll(Vector3 position, out int numHits)
	{
		if (!heightCheck || use2D)
		{
			hitBuffer[0] = new RaycastHit
			{
				point = position,
				distance = 0f
			};
			numHits = 1;
			return hitBuffer;
		}
		numHits = Physics.RaycastNonAlloc(position + up * fromHeight, -up, hitBuffer, fromHeight + 0.005f, heightMask, QueryTriggerInteraction.Ignore);
		if (numHits == hitBuffer.Length)
		{
			hitBuffer = new RaycastHit[hitBuffer.Length * 2];
			return CheckHeightAll(position, out numHits);
		}
		return hitBuffer;
	}

	public void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
	{
		type = (ColliderType)ctx.reader.ReadInt32();
		diameter = ctx.reader.ReadSingle();
		height = ctx.reader.ReadSingle();
		collisionOffset = ctx.reader.ReadSingle();
		rayDirection = (RayDirection)ctx.reader.ReadInt32();
		mask = ctx.reader.ReadInt32();
		heightMask = ctx.reader.ReadInt32();
		fromHeight = ctx.reader.ReadSingle();
		thickRaycast = ctx.reader.ReadBoolean();
		thickRaycastDiameter = ctx.reader.ReadSingle();
		unwalkableWhenNoGround = ctx.reader.ReadBoolean();
		use2D = ctx.reader.ReadBoolean();
		collisionCheck = ctx.reader.ReadBoolean();
		heightCheck = ctx.reader.ReadBoolean();
	}
}

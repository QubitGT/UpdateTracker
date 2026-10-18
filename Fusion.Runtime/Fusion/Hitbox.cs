#define DEBUG
using System.Runtime.CompilerServices;
using Fusion.LagCompensation;
using UnityEngine;

namespace Fusion;

[AddComponentMenu("Fusion/Lag Compensation/Hitbox")]
public class Hitbox : Behaviour
{
	[InlineHelp]
	public HitboxTypes Type;

	[InlineHelp]
	[DrawIf("Type", 2L, CompareOperator.Equal, DrawIfMode.ReadOnly, Hide = true)]
	[Unit(Units.Units)]
	public float SphereRadius;

	[InlineHelp]
	[DrawIf("Type", 3L, CompareOperator.Equal, DrawIfMode.ReadOnly, Hide = true)]
	[Unit(Units.Units)]
	public float CapsuleRadius;

	[InlineHelp]
	[DrawIf("Type", 1L, CompareOperator.Equal, DrawIfMode.ReadOnly, Hide = true)]
	public Vector3 BoxExtents;

	[InlineHelp]
	[DrawIf("Type", 3L, CompareOperator.Equal, DrawIfMode.ReadOnly, Hide = true)]
	[Unit(Units.Units)]
	public float CapsuleExtents;

	[DrawIf("Type", Hide = true)]
	public Vector3 Offset;

	[HideInInspector]
	public HitboxRoot Root;

	internal int _hitboxIndex;

	[InlineHelp]
	public Color GizmosColor = Color.yellow;

	private int _cachedLayerMask;

	private Transform _cachedTransform;

	internal float AbsSphereRadius => Mathf.Abs(SphereRadius);

	internal float AbsCapsuleRadius => Mathf.Abs(CapsuleRadius);

	internal Vector3 CapsuleTopCenter => Offset + Vector3.up * (Mathf.Max(CapsuleExtents * 0.5f, AbsCapsuleRadius) - AbsCapsuleRadius);

	internal Vector3 CapsuleBottomCenter => Offset + Vector3.down * (Mathf.Max(CapsuleExtents * 0.5f, AbsCapsuleRadius) - AbsCapsuleRadius);

	internal Vector3 AbsBoxExtents
	{
		get
		{
			Vector3 result = default(Vector3);
			result.x = Mathf.Abs(BoxExtents.x);
			result.y = Mathf.Abs(BoxExtents.y);
			result.z = Mathf.Abs(BoxExtents.z);
			return result;
		}
	}

	public int HitboxIndex
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _hitboxIndex;
		}
	}

	internal uint HitboxMask
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			Assert.Check((uint)_hitboxIndex < 31u);
			return (uint)(1 << _hitboxIndex + 1);
		}
	}

	public bool HitboxActive
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return BehaviourUtils.IsAlive(Root) && Root.IsHitboxActive(this);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if (BehaviourUtils.IsAlive(Root))
			{
				Root.SetHitboxActive(this, value);
			}
		}
	}

	public int ColliderIndex { get; internal set; }

	public Vector3 Position => base.transform.position + base.transform.rotation * Offset;

	private void Awake()
	{
		CacheInfo();
	}

	internal void CacheInfo()
	{
		_cachedLayerMask = 1 << base.gameObject.layer;
		_cachedTransform = base.transform;
	}

	internal void SetColliderData(ref HitboxCollider c, int tick)
	{
		Assert.Check(BehaviourUtils.IsAlive(Root));
		c.Type = Type;
		c.Offset = Offset;
		_cachedTransform.GetPositionAndRotation(out c.Position, out c.Rotation);
		c.ResetCachedMatrix();
		c.BoxExtents = AbsBoxExtents;
		c.Radius = ((Type == HitboxTypes.Sphere) ? AbsSphereRadius : AbsCapsuleRadius);
		c.CapsuleExtents = CapsuleExtents;
		c.Hitbox = this;
		c.DebugTick = tick;
		c.layerMask = _cachedLayerMask;
		c.Active = Root.IsHitboxActiveFastUnchecked(this);
		c.IsBoxNarrowDataInitialized = false;
	}

	public void SetLayer(int layer)
	{
		base.gameObject.layer = layer;
		_cachedLayerMask = 1 << layer;
	}

	public void OnDrawGizmos()
	{
		Color gizmosColor = GizmosColor;
		if (BehaviourUtils.IsAlive(Root) && Root.StateBufferIsValid && (!Root.HitboxRootActive || !Root.IsHitboxActiveFastUnchecked(this)))
		{
			gizmosColor.a *= 0.1f;
		}
		Matrix4x4 localToWorldMatrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
		DrawGizmos(gizmosColor, ref localToWorldMatrix);
	}

	protected virtual void DrawGizmos(Color color, ref Matrix4x4 localToWorldMatrix)
	{
		Gizmos.matrix = localToWorldMatrix;
		Gizmos.color = color;
		switch (Type)
		{
		case HitboxTypes.Box:
			Gizmos.DrawWireCube(Offset, AbsBoxExtents * 2f);
			break;
		case HitboxTypes.Sphere:
			Gizmos.DrawWireSphere(Offset, AbsSphereRadius);
			break;
		case HitboxTypes.Capsule:
			LagCompensationDraw.GizmosDrawWireCapsule(CapsuleTopCenter, CapsuleBottomCenter, AbsCapsuleRadius);
			break;
		}
		Gizmos.color = Color.white;
		Gizmos.matrix = Matrix4x4.identity;
	}
}

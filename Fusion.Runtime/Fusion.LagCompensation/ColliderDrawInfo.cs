using UnityEngine;

namespace Fusion.LagCompensation;

public class ColliderDrawInfo
{
	internal int Index;

	internal IHitboxColliderContainer Container;

	public HitboxTypes Type => Container.GetCollider(Index).Type;

	public Vector3 BoxExtents => Container.GetCollider(Index).BoxExtents;

	public Vector3 Offset => Container.GetCollider(Index).Offset;

	public float Radius => Container.GetCollider(Index).Radius;

	public float CapsuleExtents => Container.GetCollider(Index).CapsuleExtents;

	public Vector3 CapsuleTopCenter => Container.GetCollider(Index).CapsuleLocalTopCenter;

	public Vector3 CapsuleBottomCenter => Container.GetCollider(Index).CapsuleLocalBottomCenter;

	public Matrix4x4 LocalToWorldMatrix => Container.GetCollider(Index).LocalToWorld;

	internal ColliderDrawInfo FromHitboxCollider(int colliderIndex)
	{
		Index = colliderIndex;
		return this;
	}

	internal void SetContainer(IHitboxColliderContainer container)
	{
		Container = container;
	}
}

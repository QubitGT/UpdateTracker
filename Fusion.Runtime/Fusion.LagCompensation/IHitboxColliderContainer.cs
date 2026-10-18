namespace Fusion.LagCompensation;

internal interface IHitboxColliderContainer
{
	ref HitboxCollider GetNextCollider(out int index);

	ref HitboxCollider GetNextTempCollider(out int tmpIndex);

	ref HitboxCollider GetCollider(int index);

	void ReleaseCollider(int index);

	void ReleaseTempColliders();
}

using UnityEngine;

namespace Fusion.LagCompensation;

public class LagCompensationDraw
{
	public SnapshotHistoryDraw SnapshotHistoryDraw;

	public BVHDraw BVHDraw;

	internal LagCompensationDraw(HitboxBuffer _buffer)
	{
		SnapshotHistoryDraw = new SnapshotHistoryDraw(_buffer);
		BVHDraw = new BVHDraw(_buffer);
	}

	public static void GizmosDrawWireCapsule(Vector3 topCenter, Vector3 bottomCenter, float capsuleRadius)
	{
		Gizmos.DrawWireSphere(topCenter, capsuleRadius);
		Gizmos.DrawWireSphere(bottomCenter, capsuleRadius);
		Gizmos.DrawLine(topCenter + Vector3.left * capsuleRadius, bottomCenter + Vector3.left * capsuleRadius);
		Gizmos.DrawLine(topCenter + Vector3.right * capsuleRadius, bottomCenter + Vector3.right * capsuleRadius);
		Gizmos.DrawLine(topCenter + Vector3.forward * capsuleRadius, bottomCenter + Vector3.forward * capsuleRadius);
		Gizmos.DrawLine(topCenter + Vector3.back * capsuleRadius, bottomCenter + Vector3.back * capsuleRadius);
	}
}

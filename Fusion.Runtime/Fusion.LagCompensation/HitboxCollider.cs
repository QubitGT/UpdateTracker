using UnityEngine;

namespace Fusion.LagCompensation;

internal struct HitboxCollider
{
	internal HitboxTypes Type;

	private Matrix4x4 _cachedMatrix;

	private bool _matrixCalculated;

	internal Vector3 Offset;

	internal Vector3 BoxExtents;

	internal float Radius;

	internal float CapsuleExtents;

	internal bool Active;

	internal Hitbox Hitbox;

	internal int layerMask;

	internal int DebugTick;

	internal bool Used;

	internal int Next;

	internal LagCompensationUtils.BoxNarrowData BoxNarrowData;

	internal Vector3 Position;

	internal Quaternion Rotation;

	internal Matrix4x4 LocalToWorld
	{
		get
		{
			if (_matrixCalculated)
			{
				return _cachedMatrix;
			}
			CustomTRS(ref _cachedMatrix, Position, Rotation, Vector3.one);
			_matrixCalculated = true;
			return _cachedMatrix;
		}
	}

	internal bool IsBoxNarrowDataInitialized { get; set; }

	internal Vector3 CapsuleLocalTopCenter => Offset + Vector3.up * (Mathf.Max(CapsuleExtents * 0.5f, Radius) - Radius);

	internal Vector3 CapsuleLocalBottomCenter => Offset + Vector3.down * (Mathf.Max(CapsuleExtents * 0.5f, Radius) - Radius);

	internal static void Lerp(ref HitboxCollider from, ref HitboxCollider to, float alpha, ref HitboxCollider result)
	{
		result = from;
		result.Offset = Vector3.Lerp(from.Offset, to.Offset, alpha);
		result.Radius = Mathf.Lerp(from.Radius, to.Radius, alpha);
		result.BoxExtents = Vector3.Lerp(from.BoxExtents, to.BoxExtents, alpha);
		result.CapsuleExtents = Mathf.Lerp(from.CapsuleExtents, to.CapsuleExtents, alpha);
		result.Position = Vector3.Lerp(from.Position, to.Position, alpha);
		result.Rotation = Quaternion.Lerp(from.Rotation, to.Rotation, alpha);
		result.layerMask = ((alpha > 0.5f) ? to.layerMask : from.layerMask);
		result.IsBoxNarrowDataInitialized = false;
	}

	internal void InitNarrowData()
	{
		if (Type == HitboxTypes.Box && !IsBoxNarrowDataInitialized)
		{
			BoxNarrowData = new LagCompensationUtils.BoxNarrowData(Position + Offset, LocalToWorld.rotation, BoxExtents);
			IsBoxNarrowDataInitialized = true;
		}
	}

	internal void ResetCachedMatrix()
	{
		_matrixCalculated = false;
	}

	private static void CustomTRS(ref Matrix4x4 res, Vector3 t, Quaternion r, Vector3 s)
	{
		res.m00 = (1f - 2f * (r.y * r.y + r.z * r.z)) * s.x;
		res.m10 = (r.x * r.y + r.z * r.w) * s.x * 2f;
		res.m20 = (r.x * r.z - r.y * r.w) * s.x * 2f;
		res.m30 = 0f;
		res.m01 = (r.x * r.y - r.z * r.w) * s.y * 2f;
		res.m11 = (1f - 2f * (r.x * r.x + r.z * r.z)) * s.y;
		res.m21 = (r.y * r.z + r.x * r.w) * s.y * 2f;
		res.m31 = 0f;
		res.m02 = (r.x * r.z + r.y * r.w) * s.z * 2f;
		res.m12 = (r.y * r.z - r.x * r.w) * s.z * 2f;
		res.m22 = (1f - 2f * (r.x * r.x + r.y * r.y)) * s.z;
		res.m32 = 0f;
		res.m03 = t.x;
		res.m13 = t.y;
		res.m23 = t.z;
		res.m33 = 1f;
	}
}

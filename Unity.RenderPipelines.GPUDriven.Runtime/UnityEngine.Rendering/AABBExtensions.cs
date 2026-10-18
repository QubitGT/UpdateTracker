namespace UnityEngine.Rendering;

internal static class AABBExtensions
{
	public static AABB ToAABB(this Bounds bounds)
	{
		AABB result = default(AABB);
		result.center = bounds.center;
		result.extents = bounds.extents;
		return result;
	}

	public static Bounds ToBounds(this AABB aabb)
	{
		Bounds result = default(Bounds);
		result.center = aabb.center;
		result.extents = aabb.extents;
		return result;
	}
}

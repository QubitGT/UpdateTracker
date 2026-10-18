namespace g3;

public static class Distance
{
	public static float ClosestPointOnLineT(Vector3f p0, Vector3f dir, Vector3f pt)
	{
		return (pt - p0).Dot(dir);
	}
}

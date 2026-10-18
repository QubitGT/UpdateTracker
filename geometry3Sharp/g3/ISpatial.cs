namespace g3;

public interface ISpatial
{
	bool SupportsNearestTriangle { get; }

	bool SupportsTriangleRayIntersection { get; }

	bool SupportsPointContainment { get; }

	int FindNearestTriangle(Vector3d p, double fMaxDist = double.MaxValue);

	int FindNearestHitTriangle(Ray3d ray, double fMaxDist = double.MaxValue);

	bool IsInside(Vector3d p);
}

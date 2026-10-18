namespace g3;

public class DMeshIntersectionTarget : IIntersectionTarget
{
	public bool UseFaceNormal = true;

	public DMesh3 Mesh { get; set; }

	public ISpatial Spatial { get; set; }

	public bool HasNormal => true;

	public DMeshIntersectionTarget()
	{
	}

	public DMeshIntersectionTarget(DMesh3 mesh, ISpatial spatial)
	{
		Mesh = mesh;
		Spatial = spatial;
	}

	public bool RayIntersect(Ray3d ray, out Vector3d vHit, out Vector3d vHitNormal)
	{
		vHit = Vector3d.Zero;
		vHitNormal = Vector3d.AxisX;
		int num = Spatial.FindNearestHitTriangle(ray);
		if (num == -1)
		{
			return false;
		}
		IntrRay3Triangle3 intrRay3Triangle = MeshQueries.TriangleIntersection(Mesh, num, ray);
		vHit = ray.PointAt(intrRay3Triangle.RayParameter);
		if (!UseFaceNormal && Mesh.HasVertexNormals)
		{
			vHitNormal = Mesh.GetTriBaryNormal(num, intrRay3Triangle.TriangleBaryCoords.x, intrRay3Triangle.TriangleBaryCoords.y, intrRay3Triangle.TriangleBaryCoords.z);
		}
		else
		{
			vHitNormal = Mesh.GetTriNormal(num);
		}
		return true;
	}
}

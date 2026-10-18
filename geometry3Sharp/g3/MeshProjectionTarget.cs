using System.Collections.Generic;

namespace g3;

public class MeshProjectionTarget : IOrientedProjectionTarget, IProjectionTarget
{
	public DMesh3 Mesh { get; set; }

	public ISpatial Spatial { get; set; }

	public MeshProjectionTarget()
	{
	}

	public MeshProjectionTarget(DMesh3 mesh, ISpatial spatial)
	{
		Mesh = mesh;
		Spatial = spatial;
		if (Spatial == null)
		{
			Spatial = new DMeshAABBTree3(mesh, autoBuild: true);
		}
	}

	public MeshProjectionTarget(DMesh3 mesh)
	{
		Mesh = mesh;
		Spatial = new DMeshAABBTree3(mesh, autoBuild: true);
	}

	public virtual Vector3d Project(Vector3d vPoint, int identifier = -1)
	{
		int tID = Spatial.FindNearestTriangle(vPoint);
		Triangle3d triangle = default(Triangle3d);
		Mesh.GetTriVertices(tID, ref triangle.V0, ref triangle.V1, ref triangle.V2);
		DistPoint3Triangle3.DistanceSqr(ref vPoint, ref triangle, out var closestPoint, out var _);
		return closestPoint;
	}

	public virtual Vector3d Project(Vector3d vPoint, out Vector3d vProjectNormal, int identifier = -1)
	{
		int tID = Spatial.FindNearestTriangle(vPoint);
		Triangle3d triangle = default(Triangle3d);
		Mesh.GetTriVertices(tID, ref triangle.V0, ref triangle.V1, ref triangle.V2);
		DistPoint3Triangle3.DistanceSqr(ref vPoint, ref triangle, out var closestPoint, out var _);
		vProjectNormal = triangle.Normal;
		return closestPoint;
	}

	public static MeshProjectionTarget Auto(DMesh3 mesh, bool bForceCopy = true)
	{
		if (bForceCopy)
		{
			return new MeshProjectionTarget(new DMesh3(mesh, bCompact: false, MeshComponents.None));
		}
		return new MeshProjectionTarget(mesh);
	}

	public static MeshProjectionTarget Auto(DMesh3 mesh, IEnumerable<int> triangles, int nExpandRings = 5)
	{
		MeshFaceSelection meshFaceSelection = new MeshFaceSelection(mesh);
		meshFaceSelection.Select(triangles);
		meshFaceSelection.ExpandToOneRingNeighbours(nExpandRings);
		return new MeshProjectionTarget(new DSubmesh3(mesh, meshFaceSelection).SubMesh);
	}
}

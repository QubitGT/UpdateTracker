using System;

namespace g3;

public static class SpatialFunctions
{
	[Obsolete("NormalOffset is deprecated - is anybody using it? please lmk.")]
	public class NormalOffset
	{
		public DMesh3 Mesh;

		public ISpatial Spatial;

		public double Distance = 0.01;

		public bool UseFaceNormal = true;

		public Vector3d FindNearestAndOffset(Vector3d pos)
		{
			int num = Spatial.FindNearestTriangle(pos);
			DistPoint3Triangle3 distPoint3Triangle = MeshQueries.TriangleDistance(Mesh, num, pos);
			Vector3d vector3d = ((!UseFaceNormal && Mesh.HasVertexNormals) ? Mesh.GetTriBaryNormal(num, distPoint3Triangle.TriangleBaryCoords.x, distPoint3Triangle.TriangleBaryCoords.y, distPoint3Triangle.TriangleBaryCoords.z) : Mesh.GetTriNormal(num));
			return distPoint3Triangle.TriangleClosest + Distance * vector3d;
		}
	}
}

namespace g3;

public class MeshTriInfoCache
{
	public DVector<Vector3d> Centroids;

	public DVector<Vector3d> Normals;

	public DVector<double> Areas;

	public MeshTriInfoCache(DMesh3 mesh)
	{
		MeshTriInfoCache meshTriInfoCache = this;
		int triangleCount = mesh.TriangleCount;
		Centroids = new DVector<Vector3d>();
		Centroids.resize(triangleCount);
		Normals = new DVector<Vector3d>();
		Normals.resize(triangleCount);
		Areas = new DVector<double>();
		Areas.resize(triangleCount);
		gParallel.ForEach(mesh.TriangleIndices(), delegate(int tid)
		{
			mesh.GetTriInfo(tid, out var normal, out var fArea, out var vCentroid);
			meshTriInfoCache.Centroids[tid] = vCentroid;
			meshTriInfoCache.Normals[tid] = normal;
			meshTriInfoCache.Areas[tid] = fArea;
		});
	}

	public void GetTriInfo(int tid, ref Vector3d n, ref double a, ref Vector3d c)
	{
		c = Centroids[tid];
		n = Normals[tid];
		a = Areas[tid];
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using g3;

namespace gs;

public class MeshRepairOrientation
{
	private class Component
	{
		public List<int> triangles;

		public double outFacing;

		public double inFacing;
	}

	public DMesh3 Mesh;

	private DMeshAABBTree3 spatial;

	private List<Component> Components = new List<Component>();

	protected DMeshAABBTree3 Spatial
	{
		get
		{
			if (spatial == null)
			{
				spatial = new DMeshAABBTree3(Mesh, autoBuild: true);
			}
			return spatial;
		}
	}

	public MeshRepairOrientation(DMesh3 mesh3, DMeshAABBTree3 spatial = null)
	{
		Mesh = mesh3;
		this.spatial = spatial;
	}

	public void OrientComponents()
	{
		Components = new List<Component>();
		HashSet<int> hashSet = new HashSet<int>(Mesh.TriangleIndices());
		List<int> list = new List<int>();
		while (hashSet.Count > 0)
		{
			Component component = new Component();
			component.triangles = new List<int>();
			list.Clear();
			int item = hashSet.First();
			hashSet.Remove(item);
			component.triangles.Add(item);
			list.Add(item);
			while (list.Count > 0)
			{
				int tID = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				Index3i triangle = Mesh.GetTriangle(tID);
				Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(tID);
				for (int i = 0; i < 3; i++)
				{
					int num = triNeighbourTris[i];
					if (hashSet.Contains(num))
					{
						int b = triangle[i];
						int a = triangle[(i + 1) % 3];
						Index3i tri_verts = Mesh.GetTriangle(num);
						if (IndexUtil.find_tri_ordered_edge(a, b, ref tri_verts) == -1)
						{
							Mesh.ReverseTriOrientation(num);
						}
						list.Add(num);
						hashSet.Remove(num);
						component.triangles.Add(num);
					}
				}
			}
			Components.Add(component);
		}
	}

	public void ComputeStatistics()
	{
		_ = Spatial;
		foreach (Component component in Components)
		{
			compute_statistics(component);
		}
	}

	private void compute_statistics(Component c)
	{
		int count = c.triangles.Count;
		c.inFacing = (c.outFacing = 0.0);
		double dist = 2.0 * Mesh.CachedBounds.DiagonalLength;
		HashSet<int> hashSet = new HashSet<int>(c.triangles);
		spatial.TriangleFilterF = hashSet.Contains;
		SpinLock count_lock = default(SpinLock);
		gParallel.BlockStartEnd(0, count - 1, delegate(int a, int b)
		{
			for (int i = a; i <= b; i++)
			{
				int num = c.triangles[i];
				Mesh.GetTriInfo(num, out var normal, out var fArea, out var vCentroid);
				if (!(fArea < 9.999999974752427E-07))
				{
					Vector3d origin = vCentroid + dist * normal;
					Vector3d origin2 = vCentroid - dist * normal;
					int num2 = spatial.FindNearestHitTriangle(new Ray3d(origin, -normal));
					int num3 = spatial.FindNearestHitTriangle(new Ray3d(origin2, normal));
					if ((num2 == num || num3 == num) && (num2 != num || num3 != num))
					{
						bool lockTaken = false;
						count_lock.Enter(ref lockTaken);
						if (num3 == num)
						{
							c.inFacing += fArea;
						}
						else if (num2 == num)
						{
							c.outFacing += fArea;
						}
						count_lock.Exit();
					}
				}
			}
		});
		spatial.TriangleFilterF = null;
	}

	public void SolveGlobalOrientation()
	{
		ComputeStatistics();
		MeshEditor meshEditor = new MeshEditor(Mesh);
		foreach (Component component in Components)
		{
			if (component.inFacing > component.outFacing)
			{
				meshEditor.ReverseTriangles(component.triangles);
			}
		}
	}
}

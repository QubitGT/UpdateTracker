using System;
using System.Collections.Generic;
using System.Threading;
using g3;

namespace gs;

public class MeshSpatialSort
{
	public class ComponentMesh
	{
		public object Identifier;

		public DMesh3 Mesh;

		public bool IsClosed;

		public DMeshAABBTree3 Spatial;

		public AxisAlignedBox3d Bounds;

		public List<ComponentMesh> InsideOf = new List<ComponentMesh>();

		public List<ComponentMesh> InsideSet = new List<ComponentMesh>();

		public ComponentMesh(DMesh3 mesh, object identifier, DMeshAABBTree3 spatial)
		{
			Mesh = mesh;
			Identifier = identifier;
			IsClosed = mesh.IsClosed();
			Spatial = spatial;
			Bounds = mesh.CachedBounds;
		}

		public bool Contains(ComponentMesh mesh2, double fIso = 0.5)
		{
			if (Spatial == null)
			{
				return false;
			}
			Spatial.FastWindingNumber(Vector3d.Zero);
			int vertexCount = mesh2.Mesh.VertexCount;
			bool contained = true;
			gParallel.BlockStartEnd(0, vertexCount - 1, delegate(int a, int b)
			{
				if (contained)
				{
					for (int i = a; i <= b && contained; i++)
					{
						Vector3d vertex = mesh2.Mesh.GetVertex(i);
						if (Math.Abs(Spatial.FastWindingNumber(vertex)) < fIso)
						{
							contained = false;
							break;
						}
					}
				}
			}, 100);
			return contained;
		}
	}

	public class MeshSolid
	{
		public ComponentMesh Outer;

		public List<ComponentMesh> Cavities = new List<ComponentMesh>();
	}

	public List<ComponentMesh> Components;

	public List<MeshSolid> Solids;

	public bool AllowOpenContainers;

	public double FastWindingIso = 0.5;

	public MeshSpatialSort()
	{
		Components = new List<ComponentMesh>();
	}

	public void AddMesh(DMesh3 mesh, object identifier, DMeshAABBTree3 spatial = null)
	{
		ComponentMesh componentMesh = new ComponentMesh(mesh, identifier, spatial);
		if (spatial == null && (componentMesh.IsClosed || AllowOpenContainers))
		{
			componentMesh.Spatial = new DMeshAABBTree3(mesh, autoBuild: true);
		}
		Components.Add(componentMesh);
	}

	public void Sort()
	{
		int N = Components.Count;
		ComponentMesh[] comps = Components.ToArray();
		Array.Sort(comps, (ComponentMesh i, ComponentMesh j) => (!i.Bounds.Contains(j.Bounds)) ? 1 : (-1));
		bool[] bIsContained = new bool[N];
		Dictionary<int, List<int>> ContainSets = new Dictionary<int, List<int>>();
		Dictionary<int, List<int>> ContainedParents = new Dictionary<int, List<int>>();
		SpinLock dataLock = default(SpinLock);
		gParallel.ForEach(Interval1i.Range(N), delegate(int i)
		{
			ComponentMesh componentMesh4 = comps[i];
			if (componentMesh4.IsClosed || AllowOpenContainers)
			{
				for (int n = 0; n < N; n++)
				{
					if (i != n)
					{
						ComponentMesh componentMesh5 = comps[n];
						if (componentMesh4.Bounds.Contains(componentMesh5.Bounds) && componentMesh4.Contains(componentMesh5))
						{
							bool lockTaken = false;
							dataLock.Enter(ref lockTaken);
							componentMesh5.InsideOf.Add(componentMesh4);
							componentMesh4.InsideSet.Add(componentMesh5);
							if (!ContainSets.ContainsKey(i))
							{
								ContainSets.Add(i, new List<int>());
							}
							ContainSets[i].Add(n);
							bIsContained[n] = true;
							if (!ContainedParents.ContainsKey(n))
							{
								ContainedParents.Add(n, new List<int>());
							}
							ContainedParents[n].Add(i);
							dataLock.Exit();
						}
					}
				}
			}
		});
		List<MeshSolid> list = new List<MeshSolid>();
		HashSet<ComponentMesh> hashSet = new HashSet<ComponentMesh>();
		Dictionary<ComponentMesh, int> dictionary = new Dictionary<ComponentMesh, int>();
		List<int> list2 = new List<int>();
		for (int k = 0; k < N; k++)
		{
			ComponentMesh componentMesh = comps[k];
			if (!bIsContained[k])
			{
				MeshSolid item = new MeshSolid
				{
					Outer = componentMesh
				};
				int count = list.Count;
				dictionary[componentMesh] = count;
				hashSet.Add(componentMesh);
				if (ContainSets.ContainsKey(k))
				{
					list2.Add(k);
				}
				list.Add(item);
			}
		}
		while (list2.Count > 0)
		{
			List<int> list3 = new List<int>();
			foreach (int item5 in list2)
			{
				ComponentMesh key = comps[item5];
				int index = dictionary[key];
				foreach (int item6 in ContainSets[item5])
				{
					ComponentMesh item2 = comps[item6];
					if (ContainedParents[item6].Count <= 1)
					{
						list[index].Cavities.Add(item2);
						hashSet.Add(item2);
						if (ContainSets.ContainsKey(item6))
						{
							list3.Add(item6);
						}
					}
				}
				list3.Add(item5);
			}
			foreach (int item7 in list3)
			{
				ContainSets.Remove(item7);
				foreach (int item8 in new List<int>(ContainedParents.Keys))
				{
					if (ContainedParents[item8].Contains(item7))
					{
						ContainedParents[item8].Remove(item7);
					}
				}
			}
			list2.Clear();
			for (int l = 0; l < N; l++)
			{
				ComponentMesh componentMesh2 = comps[l];
				if (!hashSet.Contains(componentMesh2) && ContainSets.ContainsKey(l) && ContainedParents[l].Count <= 0)
				{
					MeshSolid item3 = new MeshSolid
					{
						Outer = componentMesh2
					};
					int count2 = list.Count;
					dictionary[componentMesh2] = count2;
					hashSet.Add(componentMesh2);
					if (ContainSets.ContainsKey(l))
					{
						list2.Add(l);
					}
					list.Add(item3);
				}
			}
		}
		for (int m = 0; m < N; m++)
		{
			ComponentMesh componentMesh3 = comps[m];
			if (!hashSet.Contains(componentMesh3))
			{
				MeshSolid item4 = new MeshSolid
				{
					Outer = componentMesh3
				};
				list.Add(item4);
			}
		}
		Solids = list;
	}
}

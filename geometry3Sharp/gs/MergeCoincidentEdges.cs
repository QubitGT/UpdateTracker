using System;
using System.Collections.Generic;
using g3;

namespace gs;

public class MergeCoincidentEdges
{
	private class DuplicateEdge : DynamicPriorityQueueNode
	{
		public int eid;
	}

	public DMesh3 Mesh;

	public double MergeDistance = 9.999999974752427E-07;

	public bool OnlyUniquePairs;

	private double merge_r2;

	public MergeCoincidentEdges(DMesh3 mesh)
	{
		Mesh = mesh;
	}

	public virtual bool Apply()
	{
		merge_r2 = MergeDistance * MergeDistance;
		PointSetHashtable pointSetHashtable = new PointSetHashtable(new MeshBoundaryEdgeMidpoints(Mesh));
		int maxAxisSubdivs = 64;
		if (Mesh.TriangleCount > 100000)
		{
			maxAxisSubdivs = 128;
		}
		if (Mesh.TriangleCount > 1000000)
		{
			maxAxisSubdivs = 256;
		}
		pointSetHashtable.Build(maxAxisSubdivs);
		Vector3d a = Vector3d.Zero;
		Vector3d b = Vector3d.Zero;
		Vector3d a2 = Vector3d.Zero;
		Vector3d b2 = Vector3d.Zero;
		int[] array = new int[1024];
		List<int>[] array2 = new List<int>[Mesh.MaxEdgeID];
		HashSet<int> hashSet = new HashSet<int>();
		foreach (int item in Mesh.BoundaryEdgeIndices())
		{
			Vector3d edgePoint = Mesh.GetEdgePoint(item, 0.5);
			int buffer_count;
			while (!pointSetHashtable.FindInBall(edgePoint, MergeDistance, array, out buffer_count))
			{
				array = new int[array.Length];
			}
			if (buffer_count == 1 && array[0] != item)
			{
				throw new Exception("MergeCoincidentEdges.Apply: how could this happen?!");
			}
			if (buffer_count <= 1)
			{
				continue;
			}
			Mesh.GetEdgeV(item, ref a, ref b);
			List<int> list = new List<int>(buffer_count - 1);
			for (int i = 0; i < buffer_count; i++)
			{
				if (array[i] != item)
				{
					Mesh.GetEdgeV(array[i], ref a2, ref b2);
					if (is_same_edge(ref a, ref b, ref a2, ref b2))
					{
						list.Add(array[i]);
					}
				}
			}
			if (list.Count > 0)
			{
				array2[item] = list;
				hashSet.Add(item);
			}
		}
		DynamicPriorityQueue<DuplicateEdge> dynamicPriorityQueue = new DynamicPriorityQueue<DuplicateEdge>();
		foreach (int item2 in hashSet)
		{
			if (OnlyUniquePairs)
			{
				if (array2[item2].Count != 1)
				{
					continue;
				}
				foreach (int item3 in array2[item2])
				{
					if (array2[item3].Count == 1)
					{
						_ = array2[item3][0];
					}
				}
			}
			dynamicPriorityQueue.Enqueue(new DuplicateEdge
			{
				eid = item2
			}, array2[item2].Count);
		}
		while (dynamicPriorityQueue.Count > 0)
		{
			DuplicateEdge duplicateEdge = dynamicPriorityQueue.Dequeue();
			if (!Mesh.IsEdge(duplicateEdge.eid) || array2[duplicateEdge.eid] == null || !hashSet.Contains(duplicateEdge.eid) || !Mesh.IsBoundaryEdge(duplicateEdge.eid))
			{
				continue;
			}
			List<int> list2 = array2[duplicateEdge.eid];
			bool flag = false;
			int num = 0;
			for (int j = 0; j < list2.Count; j++)
			{
				if (flag)
				{
					break;
				}
				int num2 = list2[j];
				if (Mesh.IsEdge(num2) && Mesh.IsBoundaryEdge(num2))
				{
					if (Mesh.MergeEdges(duplicateEdge.eid, num2, out var _) != 0)
					{
						list2.RemoveAt(j);
						j--;
						array2[num2].Remove(duplicateEdge.eid);
						num++;
					}
					else
					{
						flag = true;
						array2[num2] = null;
						hashSet.Remove(num2);
					}
				}
			}
			if (flag)
			{
				array2[duplicateEdge.eid] = null;
				hashSet.Remove(duplicateEdge.eid);
			}
			else
			{
				array2[duplicateEdge.eid] = null;
				hashSet.Remove(duplicateEdge.eid);
			}
		}
		return true;
	}

	private bool is_same_edge(ref Vector3d a, ref Vector3d b, ref Vector3d c, ref Vector3d d)
	{
		if (!(a.DistanceSquared(c) < merge_r2) || !(b.DistanceSquared(d) < merge_r2))
		{
			if (a.DistanceSquared(d) < merge_r2)
			{
				return b.DistanceSquared(c) < merge_r2;
			}
			return false;
		}
		return true;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder;

public static class Smoothing
{
	internal const int smoothingGroupNone = 0;

	internal const int smoothRangeMin = 1;

	internal const int smoothRangeMax = 30;

	public static int GetUnusedSmoothingGroup(ProBuilderMesh mesh)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		return GetNextUnusedSmoothingGroup(1, new HashSet<int>(mesh.facesInternal.Select((Face x) => x.smoothingGroup)));
	}

	private static int GetNextUnusedSmoothingGroup(int start, HashSet<int> used)
	{
		while (used.Contains(start) && start < 2147483646)
		{
			start++;
		}
		return start;
	}

	public static bool IsSmooth(int index)
	{
		return index > 0;
	}

	public static void ApplySmoothingGroups(ProBuilderMesh mesh, IEnumerable<Face> faces, float angleThreshold)
	{
		ApplySmoothingGroups(mesh, faces, angleThreshold, null);
	}

	internal static void ApplySmoothingGroups(ProBuilderMesh mesh, IEnumerable<Face> faces, float angleThreshold, Vector3[] normals)
	{
		if (mesh == null || faces == null)
		{
			throw new ArgumentNullException("mesh");
		}
		bool flag = false;
		foreach (Face face in faces)
		{
			if (face.smoothingGroup != 0)
			{
				flag = true;
			}
			face.smoothingGroup = 0;
		}
		if (normals == null)
		{
			if (flag)
			{
				mesh.mesh.normals = null;
			}
			normals = mesh.GetNormals();
		}
		float angleThreshold2 = Mathf.Abs(Mathf.Cos(Mathf.Clamp(angleThreshold, 0f, 89.999f) * (MathF.PI / 180f)));
		HashSet<int> hashSet = new HashSet<int>(mesh.facesInternal.Select((Face x) => x.smoothingGroup));
		int nextUnusedSmoothingGroup = GetNextUnusedSmoothingGroup(1, hashSet);
		HashSet<Face> hashSet2 = new HashSet<Face>();
		List<WingedEdge> wingedEdges = WingedEdge.GetWingedEdges(mesh, faces, oneWingPerFace: true);
		try
		{
			foreach (WingedEdge item in wingedEdges)
			{
				if (hashSet2.Add(item.face))
				{
					item.face.smoothingGroup = nextUnusedSmoothingGroup;
					if (FindSoftEdgesRecursive(normals, item, angleThreshold2, hashSet2))
					{
						hashSet.Add(nextUnusedSmoothingGroup);
						nextUnusedSmoothingGroup = GetNextUnusedSmoothingGroup(nextUnusedSmoothingGroup, hashSet);
					}
					else
					{
						item.face.smoothingGroup = 0;
					}
				}
			}
		}
		catch
		{
			Debug.LogWarning("Smoothing has been aborted: Too many edges in the analyzed mesh");
		}
	}

	private static bool FindSoftEdgesRecursive(Vector3[] normals, WingedEdge wing, float angleThreshold, HashSet<Face> processed)
	{
		bool result = false;
		using WingedEdgeEnumerator wingedEdgeEnumerator = new WingedEdgeEnumerator(wing);
		while (wingedEdgeEnumerator.MoveNext())
		{
			WingedEdge current = wingedEdgeEnumerator.Current;
			if (current.opposite != null && current.opposite.face.smoothingGroup == 0 && IsSoftEdge(normals, current.edge, current.opposite.edge, angleThreshold) && processed.Add(current.opposite.face))
			{
				result = true;
				current.opposite.face.smoothingGroup = wing.face.smoothingGroup;
				FindSoftEdgesRecursive(normals, current.opposite, angleThreshold, processed);
			}
		}
		return result;
	}

	private static bool IsSoftEdge(Vector3[] normals, EdgeLookup left, EdgeLookup right, float threshold)
	{
		Vector3 lhs = normals[left.local.a];
		Vector3 lhs2 = normals[left.local.b];
		Vector3 rhs = normals[(right.common.a == left.common.a) ? right.local.a : right.local.b];
		Vector3 rhs2 = normals[(right.common.b == left.common.b) ? right.local.b : right.local.a];
		lhs.Normalize();
		lhs2.Normalize();
		rhs.Normalize();
		rhs2.Normalize();
		if (Mathf.Abs(Vector3.Dot(lhs, rhs)) > threshold)
		{
			return Mathf.Abs(Vector3.Dot(lhs2, rhs2)) > threshold;
		}
		return false;
	}
}

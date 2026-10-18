using System.Collections.Generic;

namespace g3;

public static class FaceGroupUtil
{
	public static void SetGroupID(DMesh3 mesh, int to)
	{
		if (!mesh.HasTriangleGroups)
		{
			return;
		}
		foreach (int item in mesh.TriangleIndices())
		{
			mesh.SetTriangleGroup(item, to);
		}
	}

	public static void SetGroupID(DMesh3 mesh, IEnumerable<int> triangles, int to)
	{
		if (!mesh.HasTriangleGroups)
		{
			return;
		}
		foreach (int triangle in triangles)
		{
			mesh.SetTriangleGroup(triangle, to);
		}
	}

	public static void SetGroupToGroup(DMesh3 mesh, int from, int to)
	{
		if (!mesh.HasTriangleGroups)
		{
			return;
		}
		int maxTriangleID = mesh.MaxTriangleID;
		for (int i = 0; i < maxTriangleID; i++)
		{
			if (mesh.IsTriangle(i) && mesh.GetTriangleGroup(i) == from)
			{
				mesh.SetTriangleGroup(i, to);
			}
		}
	}

	public static HashSet<int> FindAllGroups(DMesh3 mesh)
	{
		HashSet<int> hashSet = new HashSet<int>();
		if (mesh.HasTriangleGroups)
		{
			int maxTriangleID = mesh.MaxTriangleID;
			for (int i = 0; i < maxTriangleID; i++)
			{
				if (mesh.IsTriangle(i))
				{
					int triangleGroup = mesh.GetTriangleGroup(i);
					hashSet.Add(triangleGroup);
				}
			}
		}
		return hashSet;
	}

	public static SparseList<int> CountAllGroups(DMesh3 mesh)
	{
		SparseList<int> sparseList = new SparseList<int>(mesh.MaxGroupID, 0, 0);
		if (mesh.HasTriangleGroups)
		{
			int maxTriangleID = mesh.MaxTriangleID;
			for (int i = 0; i < maxTriangleID; i++)
			{
				if (mesh.IsTriangle(i))
				{
					sparseList[mesh.GetTriangleGroup(i)]++;
				}
			}
		}
		return sparseList;
	}

	public static int[][] FindTriangleSetsByGroup(DMesh3 mesh, int ignoreGID = int.MinValue)
	{
		if (!mesh.HasTriangleGroups)
		{
			return new int[0][];
		}
		SparseList<int> sparseList = CountAllGroups(mesh);
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, int> item in sparseList.Values())
		{
			if (item.Key != ignoreGID && item.Value > 0)
			{
				list.Add(item.Key);
			}
		}
		list.Sort();
		SparseList<int> sparseList2 = new SparseList<int>(mesh.MaxGroupID, list.Count, -1);
		int[][] array = new int[list.Count][];
		int[] array2 = new int[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			int idx = list[i];
			array[i] = new int[sparseList[idx]];
			array2[i] = 0;
			sparseList2[idx] = i;
		}
		int maxTriangleID = mesh.MaxTriangleID;
		for (int j = 0; j < maxTriangleID; j++)
		{
			if (mesh.IsTriangle(j))
			{
				int triangleGroup = mesh.GetTriangleGroup(j);
				int num = sparseList2[triangleGroup];
				if (num >= 0)
				{
					int num2 = array2[num]++;
					array[num][num2] = j;
				}
			}
		}
		return array;
	}

	public static List<int> FindTrianglesByGroup(IMesh mesh, int findGroupID)
	{
		List<int> list = new List<int>();
		if (!mesh.HasTriangleGroups)
		{
			return list;
		}
		foreach (int item in mesh.TriangleIndices())
		{
			if (mesh.GetTriangleGroup(item) == findGroupID)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static DMesh3[] SeparateMeshByGroups(DMesh3 mesh, out int[] groupIDs)
	{
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		foreach (int item in mesh.TriangleIndices())
		{
			int triangleGroup = mesh.GetTriangleGroup(item);
			if (!dictionary.TryGetValue(triangleGroup, out var value))
			{
				value = (dictionary[triangleGroup] = new List<int>());
			}
			value.Add(item);
		}
		DMesh3[] array = new DMesh3[dictionary.Count];
		groupIDs = new int[dictionary.Count];
		int num = 0;
		foreach (KeyValuePair<int, List<int>> item2 in dictionary)
		{
			groupIDs[num] = item2.Key;
			List<int> value2 = item2.Value;
			array[num++] = DSubmesh3.QuickSubmesh(mesh, value2);
		}
		return array;
	}

	public static DMesh3[] SeparateMeshByGroups(DMesh3 mesh)
	{
		int[] groupIDs;
		return SeparateMeshByGroups(mesh, out groupIDs);
	}
}

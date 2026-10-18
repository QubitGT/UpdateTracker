using System;
using System.Collections.Generic;

namespace g3;

public class FaceGroupOptimizer
{
	private DMesh3 mesh;

	public Func<IEnumerable<int>> GetEnumeratorF;

	public int BackgroundGroupID;

	public bool DontClipEnclosedFins = true;

	public bool NoEarGroupSwaps;

	private List<Index2i> temp = new List<Index2i>();

	public DMesh3 Mesh => mesh;

	public FaceGroupOptimizer(DMesh3 meshIn)
	{
		mesh = meshIn;
		GetEnumeratorF = () => mesh.TriangleIndices();
	}

	public int ClipFins(bool bClipLoners)
	{
		temp.Clear();
		foreach (int item in GetEnumeratorF())
		{
			if (is_fin(item, bClipLoners))
			{
				temp.Add(new Index2i(item, BackgroundGroupID));
			}
		}
		if (temp.Count == 0)
		{
			return 0;
		}
		foreach (Index2i item2 in temp)
		{
			mesh.SetTriangleGroup(item2.a, item2.b);
		}
		return temp.Count;
	}

	public int FillEars(bool bFillTinyHoles)
	{
		int num = 0;
		foreach (int item in GetEnumeratorF())
		{
			int num2 = is_ear(item, bFillTinyHoles, NoEarGroupSwaps);
			if (num2 >= 0)
			{
				mesh.SetTriangleGroup(item, num2);
				num++;
			}
		}
		return num;
	}

	public bool LocalOptimize(bool bClipFins, bool bFillEars, bool bFillTinyHoles = true, bool bClipLoners = true, int max_iters = 100)
	{
		bool result = false;
		bool flag = false;
		int num = 0;
		while (!flag && num++ < max_iters)
		{
			flag = true;
			int num2 = 0;
			int num3 = 0;
			if (bClipFins)
			{
				num2 = ClipFins(bClipLoners);
			}
			if (bFillEars)
			{
				num3 = FillEars(bFillTinyHoles);
			}
			if (num2 > 0 || num3 > 0)
			{
				flag = false;
				result = true;
			}
		}
		return result;
	}

	public int DilateAllGroups(int nRings)
	{
		int num = 0;
		for (int i = 0; i < nRings; i++)
		{
			temp.Clear();
			foreach (int item in GetEnumeratorF())
			{
				if (mesh.GetTriangleGroup(item) != BackgroundGroupID)
				{
					continue;
				}
				Index3i triNeighbourTris = mesh.GetTriNeighbourTris(item);
				for (int j = 0; j < 3; j++)
				{
					if (triNeighbourTris[j] != -1)
					{
						int triangleGroup = mesh.GetTriangleGroup(triNeighbourTris[j]);
						if (triangleGroup != BackgroundGroupID)
						{
							temp.Add(new Index2i(item, triangleGroup));
							break;
						}
					}
				}
			}
			if (temp.Count == 0)
			{
				return num;
			}
			foreach (Index2i item2 in temp)
			{
				if (mesh.GetTriangleGroup(item2.a) == BackgroundGroupID)
				{
					mesh.SetTriangleGroup(item2.a, item2.b);
					num++;
				}
			}
		}
		return num;
	}

	public int ContractAllGroups(int nRings, bool bBackgroundOnly)
	{
		int num = 0;
		for (int i = 0; i < nRings; i++)
		{
			temp.Clear();
			foreach (int item in GetEnumeratorF())
			{
				int triangleGroup = mesh.GetTriangleGroup(item);
				Index3i triNeighbourTris = mesh.GetTriNeighbourTris(item);
				bool flag = false;
				if (bBackgroundOnly)
				{
					for (int j = 0; j < 3; j++)
					{
						if (flag)
						{
							break;
						}
						if (triNeighbourTris[j] != -1 && mesh.GetTriangleGroup(triNeighbourTris[j]) == BackgroundGroupID)
						{
							flag = true;
						}
					}
				}
				else
				{
					for (int k = 0; k < 3; k++)
					{
						if (flag)
						{
							break;
						}
						if (triNeighbourTris[k] != -1 && mesh.GetTriangleGroup(triNeighbourTris[k]) != triangleGroup)
						{
							flag = true;
						}
					}
				}
				if (flag)
				{
					temp.Add(new Index2i(item, BackgroundGroupID));
				}
			}
			if (temp.Count == 0)
			{
				return num;
			}
			foreach (Index2i item2 in temp)
			{
				mesh.SetTriangleGroup(item2.a, item2.b);
				num++;
			}
		}
		return num;
	}

	private int find_max_nbr(int tid, out int nbr_same, out int nbr_diff, out int bdry_e)
	{
		Index3i triNeighbourTris = mesh.GetTriNeighbourTris(tid);
		Index3i max = Index3i.Max;
		for (int i = 0; i < 3; i++)
		{
			int num = triNeighbourTris[i];
			max[i] = ((num == -1) ? (-1) : mesh.GetTriangleGroup(triNeighbourTris[i]));
		}
		int num2 = -1;
		for (int j = 0; j < 3; j++)
		{
			if (max[j] != -1 && (max[j] == max[(j + 1) % 3] || max[j] == max[(j + 2) % 3]))
			{
				num2 = j;
			}
		}
		nbr_same = 1;
		nbr_diff = 0;
		bdry_e = 0;
		if (num2 == -1)
		{
			return -1;
		}
		int num3 = max[num2];
		for (int k = 1; k < 3; k++)
		{
			int key = (num2 + k) % 3;
			if (max[key] == -1)
			{
				bdry_e++;
			}
			else if (max[key] == num3)
			{
				nbr_same++;
			}
			else
			{
				nbr_diff++;
			}
		}
		return num3;
	}

	private int is_ear(int tid, bool include_tiny_holes, bool bBackgroundOnly)
	{
		int triangleGroup = mesh.GetTriangleGroup(tid);
		if (bBackgroundOnly && triangleGroup != BackgroundGroupID)
		{
			return -1;
		}
		int nbr_same;
		int nbr_diff;
		int bdry_e;
		int num = find_max_nbr(tid, out nbr_same, out nbr_diff, out bdry_e);
		if (num == -1 || num == triangleGroup)
		{
			return -1;
		}
		if (bdry_e == 2 && nbr_same == 1)
		{
			return num;
		}
		if (nbr_same == 2)
		{
			if (bdry_e == 1 || nbr_diff == 1)
			{
				return num;
			}
		}
		else if (include_tiny_holes && nbr_same == 3)
		{
			return num;
		}
		return -1;
	}

	private void count_same_nbrs(int tid, out int nbr_same, out int nbr_diff, out int nbr_bg, out int bdry_e)
	{
		int triangleGroup = mesh.GetTriangleGroup(tid);
		Index3i triNeighbourTris = mesh.GetTriNeighbourTris(tid);
		nbr_same = 0;
		nbr_diff = 0;
		bdry_e = 0;
		nbr_bg = 0;
		for (int i = 0; i < 3; i++)
		{
			int num = triNeighbourTris[i];
			if (num == -1)
			{
				bdry_e++;
				continue;
			}
			int triangleGroup2 = mesh.GetTriangleGroup(num);
			if (triangleGroup2 == BackgroundGroupID)
			{
				nbr_bg++;
			}
			if (triangleGroup2 == triangleGroup)
			{
				nbr_same++;
			}
			else
			{
				nbr_diff++;
			}
		}
	}

	private bool is_fin(int tid, bool include_loners)
	{
		if (mesh.GetTriangleGroup(tid) == BackgroundGroupID)
		{
			return false;
		}
		count_same_nbrs(tid, out var nbr_same, out var nbr_diff, out var nbr_bg, out var _);
		bool flag = (nbr_same == 1 && nbr_diff == 2) || (include_loners && nbr_same == 0 && nbr_diff == 3);
		if (DontClipEnclosedFins && flag && nbr_bg == 0)
		{
			flag = false;
		}
		return flag;
	}
}

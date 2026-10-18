using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class MeshConnectedComponents : IEnumerable<MeshConnectedComponents.Component>, IEnumerable
{
	public struct Component
	{
		public int[] Indices;
	}

	public DMesh3 Mesh;

	public IEnumerable<int> FilterSet;

	public Func<int, bool> FilterF;

	public Func<int, bool> SeedFilterF;

	public List<Component> Components;

	public int Count => Components.Count;

	public Component this[int index] => Components[index];

	public int LargestByCount
	{
		get
		{
			int num = 0;
			int num2 = Components[num].Indices.Length;
			for (int i = 1; i < Components.Count; i++)
			{
				if (Components[i].Indices.Length > num2)
				{
					num2 = Components[i].Indices.Length;
					num = i;
				}
			}
			return num;
		}
	}

	public MeshConnectedComponents(DMesh3 mesh)
	{
		Mesh = mesh;
		Components = new List<Component>();
	}

	public IEnumerator<Component> GetEnumerator()
	{
		return Components.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Components.GetEnumerator();
	}

	public void SortByCount(bool bIncreasing = true)
	{
		if (bIncreasing)
		{
			Components.Sort((Component x, Component y) => x.Indices.Length.CompareTo(y.Indices.Length));
		}
		else
		{
			Components.Sort((Component x, Component y) => -x.Indices.Length.CompareTo(y.Indices.Length));
		}
	}

	public void SortByValue(Func<Component, double> valueF, bool bIncreasing = true)
	{
		Dictionary<Component, double> vals = new Dictionary<Component, double>();
		foreach (Component component in Components)
		{
			vals[component] = valueF(component);
		}
		if (bIncreasing)
		{
			Components.Sort((Component x, Component y) => vals[x].CompareTo(vals[y]));
		}
		else
		{
			Components.Sort((Component x, Component y) => -vals[x].CompareTo(vals[y]));
		}
	}

	public void FindConnectedT()
	{
		Components = new List<Component>();
		int maxTriangleID = Mesh.MaxTriangleID;
		Func<int, bool> func = (int i) => Mesh.IsTriangle(i);
		if (FilterF != null)
		{
			func = (int i) => Mesh.IsTriangle(i) && FilterF(i);
		}
		byte[] array = new byte[Mesh.MaxTriangleID];
		Interval1i empty = Interval1i.Empty;
		if (FilterSet != null)
		{
			for (int j = 0; j < maxTriangleID; j++)
			{
				array[j] = byte.MaxValue;
			}
			foreach (int item2 in FilterSet)
			{
				if (func(item2))
				{
					array[item2] = 0;
					empty.Contain(item2);
				}
			}
		}
		else
		{
			for (int k = 0; k < maxTriangleID; k++)
			{
				if (func(k))
				{
					array[k] = 0;
					empty.Contain(k);
				}
				else
				{
					array[k] = byte.MaxValue;
				}
			}
		}
		List<int> list = new List<int>(maxTriangleID / 10);
		List<int> list2 = new List<int>(maxTriangleID / 10);
		IEnumerable<int> enumerable2;
		if (FilterSet == null)
		{
			IEnumerable<int> enumerable = empty;
			enumerable2 = enumerable;
		}
		else
		{
			enumerable2 = FilterSet;
		}
		foreach (int item3 in enumerable2)
		{
			if (array[item3] == byte.MaxValue)
			{
				continue;
			}
			int num = item3;
			if (SeedFilterF != null && !SeedFilterF(num))
			{
				continue;
			}
			list.Add(num);
			array[num] = 1;
			while (list.Count > 0)
			{
				int num2 = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				array[num2] = 2;
				list2.Add(num2);
				Index3i triNeighbourTris = Mesh.GetTriNeighbourTris(num2);
				for (int l = 0; l < 3; l++)
				{
					int num3 = triNeighbourTris[l];
					if (num3 != -1 && array[num3] == 0)
					{
						list.Add(num3);
						array[num3] = 1;
					}
				}
			}
			Component component = default(Component);
			component.Indices = list2.ToArray();
			Component item = component;
			Components.Add(item);
			for (int m = 0; m < item.Indices.Length; m++)
			{
				array[item.Indices[m]] = byte.MaxValue;
			}
			list2.Clear();
			list.Clear();
		}
	}

	public static DMesh3[] Separate(DMesh3 meshIn)
	{
		MeshConnectedComponents meshConnectedComponents = new MeshConnectedComponents(meshIn);
		meshConnectedComponents.FindConnectedT();
		meshConnectedComponents.SortByCount(bIncreasing: false);
		DMesh3[] array = new DMesh3[meshConnectedComponents.Components.Count];
		int num = 0;
		foreach (Component component in meshConnectedComponents.Components)
		{
			DSubmesh3 dSubmesh = new DSubmesh3(meshIn, component.Indices);
			array[num++] = dSubmesh.SubMesh;
		}
		return array;
	}

	public static DMesh3 LargestT(DMesh3 meshIn)
	{
		MeshConnectedComponents meshConnectedComponents = new MeshConnectedComponents(meshIn);
		meshConnectedComponents.FindConnectedT();
		meshConnectedComponents.SortByCount(bIncreasing: false);
		return new DSubmesh3(meshIn, meshConnectedComponents.Components[0].Indices).SubMesh;
	}

	public static HashSet<int> FindConnectedT(DMesh3 mesh, int tSeed)
	{
		HashSet<int> hashSet = new HashSet<int>();
		hashSet.Add(tSeed);
		List<int> list = new List<int>(64) { tSeed };
		while (list.Count > 0)
		{
			int tID = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			Index3i triNeighbourTris = mesh.GetTriNeighbourTris(tID);
			for (int i = 0; i < 3; i++)
			{
				int num = triNeighbourTris[i];
				if (num != -1 && !hashSet.Contains(num))
				{
					hashSet.Add(num);
					list.Add(num);
				}
			}
		}
		return hashSet;
	}
}

using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

[Serializable]
public class MB_TexArraySlice
{
	public bool considerMeshUVs;

	[NonReorderable]
	public List<MB_TexArraySliceRendererMatPair> sourceMaterials = new List<MB_TexArraySliceRendererMatPair>();

	public bool ContainsMaterial(Material mat)
	{
		for (int i = 0; i < sourceMaterials.Count; i++)
		{
			if (sourceMaterials[i].sourceMaterial == mat)
			{
				return true;
			}
		}
		return false;
	}

	public HashSet<Material> GetDistinctMaterials()
	{
		HashSet<Material> hashSet = new HashSet<Material>();
		if (sourceMaterials == null)
		{
			return hashSet;
		}
		for (int i = 0; i < sourceMaterials.Count; i++)
		{
			hashSet.Add(sourceMaterials[i].sourceMaterial);
		}
		return hashSet;
	}

	public bool ContainsMaterialAndMesh(Material mat, Mesh mesh)
	{
		for (int i = 0; i < sourceMaterials.Count; i++)
		{
			if (sourceMaterials[i].sourceMaterial == mat && MB_Utility.GetMesh(sourceMaterials[i].renderer) == mesh)
			{
				return true;
			}
		}
		return false;
	}

	public List<Material> GetAllUsedMaterials(List<Material> usedMats)
	{
		usedMats.Clear();
		for (int i = 0; i < sourceMaterials.Count; i++)
		{
			usedMats.Add(sourceMaterials[i].sourceMaterial);
		}
		return usedMats;
	}

	public List<GameObject> GetAllUsedRenderers(List<GameObject> allObjsFromTextureBaker)
	{
		if (considerMeshUVs)
		{
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < sourceMaterials.Count; i++)
			{
				list.Add(sourceMaterials[i].renderer);
			}
			return list;
		}
		return allObjsFromTextureBaker;
	}
}

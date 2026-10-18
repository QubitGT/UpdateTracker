using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public abstract class MB3_MeshBakerGrouperBehaviour
{
	public abstract Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection, GrouperData d);

	public abstract void DrawGizmos(Bounds sourceObjectBounds, GrouperData d);

	public List<MB3_MeshBakerCommon> DoClustering(MB3_TextureBaker tb, MB3_MeshBakerGrouper grouper, GrouperData d)
	{
		List<MB3_MeshBakerCommon> list = new List<MB3_MeshBakerCommon>();
		if ((grouper.prefabOptions_autoGeneratePrefabs || grouper.prefabOptions_mergeOutputIntoSinglePrefab) && Application.isPlaying)
		{
			Debug.LogError("Cannot generate prefabs while playing. Prefabs can only be generated in the editor and not in play mode.");
			return list;
		}
		Dictionary<string, List<Renderer>> dictionary = FilterIntoGroups(tb.GetObjectsToCombine(), d);
		if (d.clusterOnLMIndex)
		{
			Dictionary<string, List<Renderer>> dictionary2 = new Dictionary<string, List<Renderer>>();
			foreach (string key4 in dictionary.Keys)
			{
				List<Renderer> gaws = dictionary[key4];
				Dictionary<int, List<Renderer>> dictionary3 = GroupByLightmapIndex(gaws);
				foreach (int key5 in dictionary3.Keys)
				{
					string key = key4 + "-LM-" + key5;
					dictionary2.Add(key, dictionary3[key5]);
				}
			}
			dictionary = dictionary2;
		}
		if (d.clusterByLODLevel)
		{
			Dictionary<string, List<Renderer>> dictionary4 = new Dictionary<string, List<Renderer>>();
			foreach (string key6 in dictionary.Keys)
			{
				foreach (Renderer r in dictionary[key6])
				{
					if (r == null)
					{
						continue;
					}
					bool flag = false;
					LODGroup componentInParent = r.GetComponentInParent<LODGroup>();
					if (componentInParent != null)
					{
						LOD[] lODs = componentInParent.GetLODs();
						for (int i = 0; i < lODs.Length; i++)
						{
							if (Array.Find(lODs[i].renderers, (Renderer x) => x == r) != null)
							{
								flag = true;
								string key2 = $"{key6}_LOD{i}";
								if (!dictionary4.TryGetValue(key2, out var value))
								{
									value = new List<Renderer>();
									dictionary4.Add(key2, value);
								}
								if (!value.Contains(r))
								{
									value.Add(r);
								}
							}
						}
					}
					if (!flag)
					{
						string key3 = $"{key6}_LOD0";
						if (!dictionary4.TryGetValue(key3, out var value2))
						{
							value2 = new List<Renderer>();
							dictionary4.Add(key3, value2);
						}
						if (!value2.Contains(r))
						{
							value2.Add(r);
						}
					}
				}
			}
			dictionary = dictionary4;
		}
		int num = 0;
		foreach (string key7 in dictionary.Keys)
		{
			List<Renderer> list2 = dictionary[key7];
			if (list2.Count > 1 || grouper.data.includeCellsWithOnlyOneRenderer)
			{
				list.Add(AddMeshBaker(grouper, tb, key7, list2));
			}
			else
			{
				num++;
			}
		}
		Debug.Log($"Found {dictionary.Count} cells with Renderers. Not creating bakers for {num} because there is only one mesh in the cell. Creating {dictionary.Count - num} bakers.");
		return list;
	}

	private Dictionary<int, List<Renderer>> GroupByLightmapIndex(List<Renderer> gaws)
	{
		Dictionary<int, List<Renderer>> dictionary = new Dictionary<int, List<Renderer>>();
		for (int i = 0; i < gaws.Count; i++)
		{
			List<Renderer> list = null;
			if (dictionary.ContainsKey(gaws[i].lightmapIndex))
			{
				list = dictionary[gaws[i].lightmapIndex];
			}
			else
			{
				list = new List<Renderer>();
				dictionary.Add(gaws[i].lightmapIndex, list);
			}
			list.Add(gaws[i]);
		}
		return dictionary;
	}

	private MB3_MeshBakerCommon AddMeshBaker(MB3_MeshBakerGrouper grouper, MB3_TextureBaker tb, string key, List<Renderer> gaws)
	{
		int num = 0;
		for (int i = 0; i < gaws.Count; i++)
		{
			Mesh mesh = MB_Utility.GetMesh(gaws[i].gameObject);
			if (mesh != null)
			{
				num += mesh.vertexCount;
			}
		}
		GameObject gameObject = new GameObject("MeshBaker-" + key);
		gameObject.transform.position = Vector3.zero;
		MB3_MeshBakerCommon mB3_MeshBakerCommon;
		if (num >= 65535)
		{
			mB3_MeshBakerCommon = gameObject.AddComponent<MB3_MultiMeshBaker>();
			mB3_MeshBakerCommon.useObjsToMeshFromTexBaker = false;
		}
		else
		{
			mB3_MeshBakerCommon = gameObject.AddComponent<MB3_MeshBaker>();
			mB3_MeshBakerCommon.useObjsToMeshFromTexBaker = false;
		}
		mB3_MeshBakerCommon.textureBakeResults = tb.textureBakeResults;
		mB3_MeshBakerCommon.transform.parent = tb.transform;
		mB3_MeshBakerCommon.meshCombiner.settingsHolder = grouper;
		for (int j = 0; j < gaws.Count; j++)
		{
			mB3_MeshBakerCommon.GetObjectsToCombine().Add(gaws[j].gameObject);
		}
		return mB3_MeshBakerCommon;
	}

	public virtual MB3_MeshBakerGrouper.ClusterType GetClusterType()
	{
		return MB3_MeshBakerGrouper.ClusterType.none;
	}
}

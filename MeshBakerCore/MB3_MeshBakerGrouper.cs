using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_MeshBakerGrouper : MonoBehaviour, MB_IMeshBakerSettingsHolder
{
	public enum ClusterType
	{
		none,
		grid,
		pie,
		agglomerative
	}

	public static readonly Color WHITE_TRANSP = new Color(1f, 1f, 1f, 0.1f);

	public MB3_MeshBakerGrouperBehaviour grouper;

	public ClusterType clusterType;

	public Transform parentSceneObject;

	public GrouperData data;

	[HideInInspector]
	public Bounds sourceObjectBounds = new Bounds(Vector3.zero, Vector3.one);

	public string prefabOptions_outputFolder = "";

	public bool prefabOptions_autoGeneratePrefabs;

	public bool prefabOptions_mergeOutputIntoSinglePrefab;

	public MB3_MeshCombinerSettings meshBakerSettingsAsset;

	public MB3_MeshCombinerSettingsData meshBakerSettings;

	public MB_IMeshBakerSettings GetMeshBakerSettings()
	{
		if (meshBakerSettingsAsset == null)
		{
			if (meshBakerSettings == null)
			{
				meshBakerSettings = new MB3_MeshCombinerSettingsData();
			}
			return meshBakerSettings;
		}
		return meshBakerSettingsAsset.GetMeshBakerSettings();
	}

	public void GetMeshBakerSettingsAsSerializedProperty(out string propertyName, out Object targetObj)
	{
		if (meshBakerSettingsAsset == null)
		{
			targetObj = this;
			propertyName = "meshBakerSettings";
		}
		else
		{
			targetObj = meshBakerSettingsAsset;
			propertyName = "data";
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (grouper == null)
		{
			grouper = CreateGrouper(clusterType);
		}
		grouper.DrawGizmos(sourceObjectBounds, data);
	}

	public MB3_MeshBakerGrouperBehaviour CreateGrouper(ClusterType t)
	{
		if (t == ClusterType.grid)
		{
			grouper = new MB3_MeshBakerGrouperGrid();
		}
		if (t == ClusterType.pie)
		{
			grouper = new MB3_MeshBakerGrouperPie();
		}
		if (t == ClusterType.agglomerative)
		{
			grouper = new MB3_MeshBakerGrouperCluster();
		}
		if (t == ClusterType.none)
		{
			grouper = new MB3_MeshBakerGrouperNone();
		}
		return grouper;
	}

	public void DeleteAllChildMeshBakers()
	{
		MB3_MeshBakerCommon[] componentsInChildren = GetComponentsInChildren<MB3_MeshBakerCommon>();
		foreach (MB3_MeshBakerCommon obj in componentsInChildren)
		{
			MB_Utility.Destroy(obj.meshCombiner.resultSceneObject);
			MB_Utility.Destroy(obj.gameObject);
		}
	}

	public List<MB3_MeshBakerCommon> GenerateMeshBakers()
	{
		MB3_TextureBaker component = GetComponent<MB3_TextureBaker>();
		if (component == null)
		{
			Debug.LogError("There must be an MB3_TextureBaker attached to this game object.");
			return new List<MB3_MeshBakerCommon>();
		}
		if (component.GetObjectsToCombine().Count == 0)
		{
			Debug.LogError("The MB3_MeshBakerGrouper creates clusters based on the objects to combine in the MB3_TextureBaker component. There were no objects in this list.");
			return new List<MB3_MeshBakerCommon>();
		}
		if (parentSceneObject == null || !MB_Utility.IsSceneInstance(parentSceneObject.gameObject))
		{
			GameObject gameObject = new GameObject("CombinedMeshes-" + base.name);
			parentSceneObject = gameObject.transform;
		}
		List<GameObject> objectsToCombine = component.GetObjectsToCombine();
		MB3_MeshBakerCommon[] componentsInChildren = GetComponentsInChildren<MB3_MeshBakerCommon>();
		bool flag = false;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			List<GameObject> objectsToCombine2 = componentsInChildren[i].GetObjectsToCombine();
			for (int j = 0; j < objectsToCombine2.Count; j++)
			{
				if (objectsToCombine2[j] != null && objectsToCombine.Contains(objectsToCombine2[j]))
				{
					flag = true;
					break;
				}
			}
		}
		bool flag2 = true;
		if (flag)
		{
			flag2 = false;
			Debug.LogError("There are previously generated MeshBaker objects. Please use the editor to delete or replace them");
		}
		if (Application.isPlaying && prefabOptions_autoGeneratePrefabs)
		{
			Debug.LogError("Can only use Auto Generate Prefabs in the editor when the game is not playing.");
			flag2 = false;
		}
		if (flag2)
		{
			if (flag)
			{
				DeleteAllChildMeshBakers();
			}
			if (grouper == null || grouper.GetClusterType() != clusterType)
			{
				grouper = CreateGrouper(clusterType);
			}
			return grouper.DoClustering(component, this, data);
		}
		return new List<MB3_MeshBakerCommon>();
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.BuildingBlocks;

[RequireComponent(typeof(SpatialAnchorSpawnerBuildingBlock))]
public class SpatialAnchorLoaderBuildingBlock : MonoBehaviour
{
	private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;

	private SpatialAnchorSpawnerBuildingBlock _spatialAnchorSpawner;

	private void Awake()
	{
		_spatialAnchorSpawner = GetComponent<SpatialAnchorSpawnerBuildingBlock>();
		_spatialAnchorCore = SpatialAnchorCoreBuildingBlock.GetFirstInstance();
	}

	public virtual void LoadAndInstantiateAnchors(List<Guid> uuids)
	{
		_spatialAnchorCore.LoadAndInstantiateAnchors(_spatialAnchorSpawner.AnchorPrefab, uuids);
	}

	public virtual void LoadAnchorsFromDefaultLocalStorage()
	{
		SpatialAnchorLocalStorageManagerBuildingBlock spatialAnchorLocalStorageManagerBuildingBlock = UnityEngine.Object.FindAnyObjectByType<SpatialAnchorLocalStorageManagerBuildingBlock>();
		if (!spatialAnchorLocalStorageManagerBuildingBlock)
		{
			Debug.Log("[SpatialAnchorLocalStorageManagerBuildingBlock] component is missing.");
			return;
		}
		List<Guid> list;
		using (new OVRObjectPool.ListScope<Guid>(out list))
		{
			spatialAnchorLocalStorageManagerBuildingBlock.GetAnchorAnchorUuidFromLocalStorage(list);
			if (list.Count > 0)
			{
				_spatialAnchorCore.LoadAndInstantiateAnchors(_spatialAnchorSpawner.AnchorPrefab, list);
			}
		}
	}
}

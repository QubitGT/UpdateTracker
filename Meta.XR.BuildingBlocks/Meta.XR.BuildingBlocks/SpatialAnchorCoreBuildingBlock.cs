using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.BuildingBlocks;

public class SpatialAnchorCoreBuildingBlock : MonoBehaviour
{
	[Header("# Events")]
	[SerializeField]
	private UnityEvent<OVRSpatialAnchor, OVRSpatialAnchor.OperationResult> _onAnchorCreateCompleted;

	[SerializeField]
	private UnityEvent<List<OVRSpatialAnchor>> _onAnchorsLoadCompleted;

	[SerializeField]
	private UnityEvent<OVRSpatialAnchor.OperationResult> _onAnchorsEraseAllCompleted;

	[SerializeField]
	private UnityEvent<OVRSpatialAnchor, OVRSpatialAnchor.OperationResult> _onAnchorEraseCompleted;

	public UnityEvent<OVRSpatialAnchor, OVRSpatialAnchor.OperationResult> OnAnchorCreateCompleted
	{
		get
		{
			return _onAnchorCreateCompleted;
		}
		set
		{
			_onAnchorCreateCompleted = value;
		}
	}

	public UnityEvent<List<OVRSpatialAnchor>> OnAnchorsLoadCompleted
	{
		get
		{
			return _onAnchorsLoadCompleted;
		}
		set
		{
			_onAnchorsLoadCompleted = value;
		}
	}

	public UnityEvent<OVRSpatialAnchor.OperationResult> OnAnchorsEraseAllCompleted
	{
		get
		{
			return _onAnchorsEraseAllCompleted;
		}
		set
		{
			_onAnchorsEraseAllCompleted = value;
		}
	}

	public UnityEvent<OVRSpatialAnchor, OVRSpatialAnchor.OperationResult> OnAnchorEraseCompleted
	{
		get
		{
			return _onAnchorEraseCompleted;
		}
		set
		{
			_onAnchorEraseCompleted = value;
		}
	}

	protected OVRSpatialAnchor.OperationResult Result { get; set; }

	public void InstantiateSpatialAnchor(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (prefab == null)
		{
			prefab = new GameObject("Spatial Anchor");
		}
		OVRSpatialAnchor anchor = UnityEngine.Object.Instantiate(prefab, position, rotation).AddComponent<OVRSpatialAnchor>();
		InitSpatialAnchorAsync(anchor);
	}

	private async void InitSpatialAnchorAsync(OVRSpatialAnchor anchor)
	{
		await WaitForInit(anchor);
		if (Result == OVRSpatialAnchor.OperationResult.Failure)
		{
			OnAnchorCreateCompleted?.Invoke(anchor, Result);
			return;
		}
		await SaveAsync(anchor);
		OnAnchorCreateCompleted?.Invoke(anchor, Result);
	}

	protected async Task WaitForInit(OVRSpatialAnchor anchor)
	{
		float timeoutThreshold = 5f;
		float startTime = Time.time;
		while ((bool)anchor && !anchor.Created)
		{
			if (Time.time - startTime >= timeoutThreshold)
			{
				Debug.LogWarning("[SpatialAnchorCoreBuildingBlock] Failed to create the spatial anchor due to timeout.");
				Result = OVRSpatialAnchor.OperationResult.Failure;
				return;
			}
			await Task.Yield();
		}
		if (anchor == null)
		{
			Debug.LogWarning("[SpatialAnchorCoreBuildingBlock] Failed to create the spatial anchor.");
			Result = OVRSpatialAnchor.OperationResult.Failure;
		}
	}

	protected async Task SaveAsync(OVRSpatialAnchor anchor)
	{
		List<OVRSpatialAnchor> list;
		using (new OVRObjectPool.ListScope<OVRSpatialAnchor>(out list))
		{
			list.Add(anchor);
			OVRResult<OVRAnchor.SaveResult> oVRResult = await OVRSpatialAnchor.SaveAnchorsAsync(list);
			if (!oVRResult.Success)
			{
				Debug.LogWarning(string.Format("[{0}] Failed to save the spatial anchor with result {1}.", "SpatialAnchorCoreBuildingBlock", oVRResult));
				OVRSpatialAnchor.OperationResult result = ((oVRResult.Status != OVRAnchor.SaveResult.FailureInsufficientView) ? OVRSpatialAnchor.OperationResult.Failure : OVRSpatialAnchor.OperationResult.Failure_SpaceMappingInsufficient);
				Result = result;
			}
		}
	}

	public virtual void LoadAndInstantiateAnchors(GameObject prefab, List<Guid> uuids)
	{
		if (uuids == null)
		{
			throw new ArgumentNullException();
		}
		if (uuids.Count == 0)
		{
			Debug.Log("[SpatialAnchorCoreBuildingBlock] Uuid list is empty.");
		}
		else
		{
			LoadAnchorsAsync(prefab, uuids);
		}
	}

	public void EraseAllAnchors()
	{
		if (OVRSpatialAnchor.SpatialAnchors.Count != 0)
		{
			EraseAnchorsAsync();
		}
	}

	public async void EraseAnchorByUuid(Guid uuid)
	{
		if (OVRSpatialAnchor.SpatialAnchors.Count != 0)
		{
			if (!OVRSpatialAnchor.SpatialAnchors.TryGetValue(uuid, out var value))
			{
				Debug.LogWarning(string.Format("[{0}] Spatial anchor with uuid [{1}] not found.", "SpatialAnchorCoreBuildingBlock", uuid));
			}
			else
			{
				await EraseAnchorByUuidAsync(value);
			}
		}
	}

	protected async void LoadAnchorsAsync(GameObject prefab, IEnumerable<Guid> uuids)
	{
		List<OVRSpatialAnchor.UnboundAnchor> unboundAnchors;
		using (new OVRObjectPool.ListScope<OVRSpatialAnchor.UnboundAnchor>(out unboundAnchors))
		{
			OVRResult<List<OVRSpatialAnchor.UnboundAnchor>, OVRAnchor.FetchResult> oVRResult = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, unboundAnchors);
			if (!oVRResult.Success || unboundAnchors.Count == 0)
			{
				Debug.LogWarning(string.Format("[{0}] Failed to load the anchors: {1}", "SpatialAnchorCoreBuildingBlock", oVRResult.Status));
				return;
			}
			List<OVRSpatialAnchor> loadedAnchors;
			using (new OVRObjectPool.ListScope<OVRSpatialAnchor>(out loadedAnchors))
			{
				foreach (OVRSpatialAnchor.UnboundAnchor unboundAnchor in unboundAnchors)
				{
					if (!unboundAnchor.Localized && !(await unboundAnchor.LocalizeAsync()))
					{
						Debug.LogWarning(string.Format("[{0}] Failed to localize the anchor. Uuid: {1}", "SpatialAnchorCoreBuildingBlock", unboundAnchor.Uuid));
						continue;
					}
					Pose pose;
					bool num = unboundAnchor.TryGetPose(out pose);
					if (!num)
					{
						Debug.LogWarning("Unable to acquire initial anchor pose. Instantiating prefab at the origin.");
					}
					OVRSpatialAnchor oVRSpatialAnchor = (num ? UnityEngine.Object.Instantiate(prefab, pose.position, pose.rotation) : UnityEngine.Object.Instantiate(prefab)).AddComponent<OVRSpatialAnchor>();
					unboundAnchor.BindTo(oVRSpatialAnchor);
					loadedAnchors.Add(oVRSpatialAnchor);
				}
				OnAnchorsLoadCompleted?.Invoke(new List<OVRSpatialAnchor>(loadedAnchors));
			}
		}
	}

	private async void EraseAnchorsAsync()
	{
		List<OVRSpatialAnchor> anchorsToErase;
		using (new OVRObjectPool.ListScope<OVRSpatialAnchor>(out anchorsToErase))
		{
			foreach (OVRSpatialAnchor value in OVRSpatialAnchor.SpatialAnchors.Values)
			{
				anchorsToErase.Add(value);
			}
			for (int i = 0; i < anchorsToErase.Count; i++)
			{
				OVRSpatialAnchor anchor = anchorsToErase[i];
				await EraseAnchorByUuidAsync(anchor);
			}
			OVRSpatialAnchor.OperationResult arg = ((OVRSpatialAnchor.SpatialAnchors.Count != 0) ? OVRSpatialAnchor.OperationResult.Failure : OVRSpatialAnchor.OperationResult.Success);
			OnAnchorsEraseAllCompleted?.Invoke(arg);
		}
	}

	private async Task EraseAnchorByUuidAsync(OVRSpatialAnchor anchor)
	{
		if (!(await anchor.EraseAnchorAsync()).Success)
		{
			OnAnchorEraseCompleted?.Invoke(anchor, OVRSpatialAnchor.OperationResult.Failure);
			return;
		}
		UnityEngine.Object.Destroy(anchor.gameObject);
		if (OVRSpatialAnchor.SpatialAnchors.ContainsKey(anchor.Uuid))
		{
			await Task.Yield();
		}
		OnAnchorEraseCompleted?.Invoke(anchor, OVRSpatialAnchor.OperationResult.Success);
	}

	internal static SpatialAnchorCoreBuildingBlock GetFirstInstance()
	{
		SpatialAnchorCoreBuildingBlock[] array = UnityEngine.Object.FindObjectsByType<SpatialAnchorCoreBuildingBlock>(FindObjectsSortMode.None);
		foreach (SpatialAnchorCoreBuildingBlock spatialAnchorCoreBuildingBlock in array)
		{
			if (spatialAnchorCoreBuildingBlock != null && spatialAnchorCoreBuildingBlock.GetType() == typeof(SpatialAnchorCoreBuildingBlock))
			{
				return spatialAnchorCoreBuildingBlock;
			}
		}
		return null;
	}
}

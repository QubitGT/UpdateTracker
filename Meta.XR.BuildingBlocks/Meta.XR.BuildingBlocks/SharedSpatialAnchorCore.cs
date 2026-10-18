using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.BuildingBlocks;

public class SharedSpatialAnchorCore : SpatialAnchorCoreBuildingBlock
{
	[SerializeField]
	private UnityEvent<List<OVRSpatialAnchor>, OVRSpatialAnchor.OperationResult> _onSpatialAnchorsShareCompleted;

	[SerializeField]
	private UnityEvent<List<OVRSpatialAnchor>, OVRAnchor.ShareResult> _onSpatialAnchorsShareToGroupCompleted;

	[SerializeField]
	private UnityEvent<List<OVRSpatialAnchor>, OVRSpatialAnchor.OperationResult> _onSharedSpatialAnchorsLoadCompleted;

	private Action<OVRSpatialAnchor.OperationResult, IEnumerable<OVRSpatialAnchor>> _onShareCompleted;

	private Action<OVRResult<OVRAnchor.ShareResult>, IEnumerable<OVRSpatialAnchor>> _onShareToGroupCompleted;

	public UnityEvent<List<OVRSpatialAnchor>, OVRSpatialAnchor.OperationResult> OnSpatialAnchorsShareCompleted
	{
		get
		{
			return _onSpatialAnchorsShareCompleted;
		}
		set
		{
			_onSpatialAnchorsShareCompleted = value;
		}
	}

	public UnityEvent<List<OVRSpatialAnchor>, OVRAnchor.ShareResult> OnSpatialAnchorsShareToGroupCompleted
	{
		get
		{
			return _onSpatialAnchorsShareToGroupCompleted;
		}
		set
		{
			_onSpatialAnchorsShareToGroupCompleted = value;
		}
	}

	public UnityEvent<List<OVRSpatialAnchor>, OVRSpatialAnchor.OperationResult> OnSharedSpatialAnchorsLoadCompleted
	{
		get
		{
			return _onSharedSpatialAnchorsLoadCompleted;
		}
		set
		{
			_onSharedSpatialAnchorsLoadCompleted = value;
		}
	}

	private void Start()
	{
		_onShareCompleted = (Action<OVRSpatialAnchor.OperationResult, IEnumerable<OVRSpatialAnchor>>)Delegate.Combine(_onShareCompleted, new Action<OVRSpatialAnchor.OperationResult, IEnumerable<OVRSpatialAnchor>>(OnShareCompleted));
		_onShareToGroupCompleted = (Action<OVRResult<OVRAnchor.ShareResult>, IEnumerable<OVRSpatialAnchor>>)Delegate.Combine(_onShareToGroupCompleted, new Action<OVRResult<OVRAnchor.ShareResult>, IEnumerable<OVRSpatialAnchor>>(OnShareToGroupCompleted));
	}

	public new async void InstantiateSpatialAnchor(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (prefab == null)
		{
			prefab = new GameObject("Shared Spatial Anchor");
		}
		OVRSpatialAnchor anchor = UnityEngine.Object.Instantiate(prefab, position, rotation).AddComponent<OVRSpatialAnchor>();
		await InitSpatialAnchor(anchor);
	}

	private async Task InitSpatialAnchor(OVRSpatialAnchor anchor)
	{
		await WaitForInit(anchor);
		if (base.Result == OVRSpatialAnchor.OperationResult.Failure)
		{
			base.OnAnchorCreateCompleted?.Invoke(anchor, base.Result);
			return;
		}
		await SaveAsync(anchor);
		if (base.Result.IsError())
		{
			base.OnAnchorCreateCompleted?.Invoke(anchor, base.Result);
		}
		else
		{
			base.OnAnchorCreateCompleted?.Invoke(anchor, base.Result);
		}
	}

	public override async void LoadAndInstantiateAnchors(GameObject prefab, List<Guid> uuids)
	{
		if (uuids == null)
		{
			throw new ArgumentNullException();
		}
		if (uuids.Count == 0)
		{
			throw new ArgumentException("[SpatialAnchorCoreBuildingBlock] Uuid list is empty.");
		}
		List<OVRSpatialAnchor.UnboundAnchor> list;
		using (new OVRObjectPool.ListScope<OVRSpatialAnchor.UnboundAnchor>(out list))
		{
			LoadSharedSpatialAnchorsRoutine(prefab, await OVRSpatialAnchor.LoadUnboundSharedAnchorsAsync(uuids, list));
		}
	}

	public async void LoadAndInstantiateAnchorsFromGroup(GameObject prefab, Guid groupUuid)
	{
		List<OVRSpatialAnchor.UnboundAnchor> list;
		using (new OVRObjectPool.ListScope<OVRSpatialAnchor.UnboundAnchor>(out list))
		{
			LoadSharedSpatialAnchorsRoutine(prefab, await OVRSpatialAnchor.LoadUnboundSharedAnchorsAsync(groupUuid, list));
		}
	}

	private async void LoadSharedSpatialAnchorsRoutine(GameObject prefab, OVRResult<List<OVRSpatialAnchor.UnboundAnchor>, OVRSpatialAnchor.OperationResult> result)
	{
		if (!result.Success)
		{
			Debug.LogWarning(string.Format("[{0}] Failed to load the shared spatial anchors: {1}", "SharedSpatialAnchorCore", result.Status));
			OnSharedSpatialAnchorsLoadCompleted?.Invoke(null, result.Status);
			return;
		}
		List<OVRSpatialAnchor.UnboundAnchor> unboundAnchors = result.Value;
		if (unboundAnchors.Count == 0)
		{
			Debug.LogWarning("[SharedSpatialAnchorCore] There's no shared spatial anchors being loaded.");
			OnSharedSpatialAnchorsLoadCompleted?.Invoke(null, result.Status);
			return;
		}
		List<OVRSpatialAnchor> loadedAnchors;
		using (new OVRObjectPool.ListScope<OVRSpatialAnchor>(out loadedAnchors))
		{
			for (int i = 0; i < unboundAnchors.Count; i++)
			{
				OVRSpatialAnchor.UnboundAnchor unboundAnchor = unboundAnchors[i];
				if (!unboundAnchor.Localized && !(await unboundAnchor.LocalizeAsync()))
				{
					Debug.LogWarning(string.Format("[{0}] Failed to localize the anchor. Uuid: {1}", "SharedSpatialAnchorCore", unboundAnchor.Uuid));
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
			OnSharedSpatialAnchorsLoadCompleted?.Invoke(new List<OVRSpatialAnchor>(loadedAnchors), result.Status);
		}
	}

	public void ShareSpatialAnchors(List<OVRSpatialAnchor> anchors, List<OVRSpaceUser> users)
	{
		if (anchors == null || users == null)
		{
			throw new ArgumentNullException();
		}
		if (anchors.Count == 0 || users.Count == 0)
		{
			throw new ArgumentException("[SharedSpatialAnchorCore] Anchors or users cannot be zero.");
		}
		OVRSpatialAnchor.ShareAsync(anchors, users).ContinueWith(_onShareCompleted, anchors);
	}

	public void ShareSpatialAnchors(List<OVRSpatialAnchor> anchors, Guid groupUuid)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException();
		}
		if (anchors.Count == 0)
		{
			throw new ArgumentException("[SharedSpatialAnchorCore] Anchors list cannot be zero.");
		}
		OVRSpatialAnchor.ShareAsync(anchors, groupUuid).ContinueWith(_onShareToGroupCompleted, anchors);
	}

	private void OnShareCompleted(OVRSpatialAnchor.OperationResult result, IEnumerable<OVRSpatialAnchor> anchors)
	{
		if (result != 0)
		{
			OnSpatialAnchorsShareCompleted?.Invoke(null, result);
			return;
		}
		List<OVRSpatialAnchor> list;
		using (new OVRObjectPool.ListScope<OVRSpatialAnchor>(out list))
		{
			list.AddRange(anchors);
			OnSpatialAnchorsShareCompleted?.Invoke(new List<OVRSpatialAnchor>(list), OVRSpatialAnchor.OperationResult.Success);
		}
	}

	private void OnShareToGroupCompleted(OVRResult<OVRAnchor.ShareResult> result, IEnumerable<OVRSpatialAnchor> anchors)
	{
		if (!result.Success)
		{
			OnSpatialAnchorsShareToGroupCompleted?.Invoke(null, result.Status);
			return;
		}
		List<OVRSpatialAnchor> list;
		using (new OVRObjectPool.ListScope<OVRSpatialAnchor>(out list))
		{
			list.AddRange(anchors);
			OnSpatialAnchorsShareToGroupCompleted?.Invoke(new List<OVRSpatialAnchor>(list), result.Status);
		}
	}

	private void OnDestroy()
	{
		_onShareCompleted = (Action<OVRSpatialAnchor.OperationResult, IEnumerable<OVRSpatialAnchor>>)Delegate.Remove(_onShareCompleted, new Action<OVRSpatialAnchor.OperationResult, IEnumerable<OVRSpatialAnchor>>(OnShareCompleted));
		_onShareToGroupCompleted = (Action<OVRResult<OVRAnchor.ShareResult>, IEnumerable<OVRSpatialAnchor>>)Delegate.Remove(_onShareToGroupCompleted, new Action<OVRResult<OVRAnchor.ShareResult>, IEnumerable<OVRSpatialAnchor>>(OnShareToGroupCompleted));
	}
}

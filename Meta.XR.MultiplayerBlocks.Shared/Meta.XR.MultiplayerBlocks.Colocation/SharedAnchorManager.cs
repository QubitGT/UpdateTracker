using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meta.XR.BuildingBlocks;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Colocation;

internal class SharedAnchorManager
{
	private readonly List<OVRSpatialAnchor> _localAnchors = new List<OVRSpatialAnchor>();

	private readonly List<OVRSpatialAnchor> _sharedAnchors = new List<OVRSpatialAnchor>();

	private readonly HashSet<OVRSpaceUser> _userShareList = new HashSet<OVRSpaceUser>();

	private const int SaveAnchorWaitTimeThreshold = 10000;

	private bool _saveAnchorSaveToCloudIsSuccessful;

	private const int ShareAnchorWaitTimeThreshold = 10000;

	private bool _shareAnchorIsSuccessful;

	private const int RetrieveAnchorWaitTimeThreshold = 10000;

	private bool _retrieveAnchorIsSuccessful;

	private List<Task> _localizationTasks;

	private List<TaskCompletionSource<bool>> _localizationTcsList;

	private SharedSpatialAnchorCore _ssaCore;

	public GameObject AnchorPrefab { get; set; }

	public IReadOnlyList<OVRSpatialAnchor> LocalAnchors => _localAnchors;

	public SharedAnchorManager(SharedSpatialAnchorCore ssaCore)
	{
		_ssaCore = ssaCore;
	}

	public async Task<OVRSpatialAnchor> CreateAlignmentAnchor()
	{
		var (oVRSpatialAnchor, operationResult) = await CreateAnchor(Vector3.zero, Quaternion.identity);
		if (oVRSpatialAnchor == null)
		{
			Logger.Log("AutomaticColocationLauncher: _sharedAnchorManager.CreateAnchor returned null", LogLevel.Error);
			return null;
		}
		if (operationResult == OVRSpatialAnchor.OperationResult.Failure_SpaceNetworkTimeout || operationResult == OVRSpatialAnchor.OperationResult.Failure_SpaceCloudStorageDisabled || operationResult == OVRSpatialAnchor.OperationResult.Failure_SpaceNetworkRequestFailed)
		{
			Logger.Log("AutomaticColocationLauncher: We did not save the local anchor to the cloud", LogLevel.SharedSpatialAnchorsError);
			return null;
		}
		if (operationResult != 0)
		{
			Logger.Log(string.Format("{0}: Anchor creation failed with result: {1}.", "AutomaticColocationLauncher", operationResult), LogLevel.SharedSpatialAnchorsError);
			return null;
		}
		Logger.Log($"ColocationLauncher: Anchor created: {oVRSpatialAnchor.Uuid}", LogLevel.Verbose);
		return oVRSpatialAnchor;
	}

	private async Task<(OVRSpatialAnchor, OVRSpatialAnchor.OperationResult)> CreateAnchor(Vector3 position, Quaternion orientation)
	{
		Logger.Log("SharedAnchorManager: Attempt to InstantiateAnchor", LogLevel.Verbose);
		var (oVRSpatialAnchor, operationResult) = await AnchorCreationTask(position, orientation);
		if (!oVRSpatialAnchor || !oVRSpatialAnchor.Created || operationResult != 0)
		{
			Logger.Log(string.Format("{0}: Anchor creation failed with result: {1}", "SharedAnchorManager", operationResult), LogLevel.SharedSpatialAnchorsError);
			return (null, operationResult);
		}
		Logger.Log(string.Format("{0}: Created anchor with id {1}", "SharedAnchorManager", oVRSpatialAnchor.Uuid), LogLevel.Info);
		_localAnchors.Add(oVRSpatialAnchor);
		return (oVRSpatialAnchor, operationResult);
	}

	private async Task<(OVRSpatialAnchor, OVRSpatialAnchor.OperationResult)> AnchorCreationTask(Vector3 position, Quaternion orientation)
	{
		_saveAnchorSaveToCloudIsSuccessful = false;
		CheckIfSavingAnchorsServiceHung();
		TaskCompletionSource<(OVRSpatialAnchor, OVRSpatialAnchor.OperationResult)> task = new TaskCompletionSource<(OVRSpatialAnchor, OVRSpatialAnchor.OperationResult)>();
		_ssaCore.OnAnchorCreateCompleted.AddListener(CreateCompletedCallback);
		_ssaCore.InstantiateSpatialAnchor(AnchorPrefab, position, orientation);
		(OVRSpatialAnchor, OVRSpatialAnchor.OperationResult) result2 = await task.Task;
		_ssaCore.OnAnchorCreateCompleted.RemoveListener(CreateCompletedCallback);
		return result2;
		void CreateCompletedCallback(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
		{
			_saveAnchorSaveToCloudIsSuccessful = true;
			task.TrySetResult((anchor, result));
		}
	}

	private async void CheckIfSavingAnchorsServiceHung()
	{
		await Task.Delay(10000);
		if (!_saveAnchorSaveToCloudIsSuccessful)
		{
			Logger.Log($"SharedAnchorManager: It has been {10000}ms since attempting to save to the cloud. Anchors service may have failed", LogLevel.Warning);
		}
	}

	public async Task<IReadOnlyList<OVRSpatialAnchor>> RetrieveAnchorsFromGroup(Guid groupUuid)
	{
		TaskCompletionSource<IReadOnlyList<OVRSpatialAnchor>> task = new TaskCompletionSource<IReadOnlyList<OVRSpatialAnchor>>();
		_retrieveAnchorIsSuccessful = false;
		CheckIfRetrievingAnchorServiceHung();
		_ssaCore.OnSharedSpatialAnchorsLoadCompleted.AddListener(LoadCompletedCallback);
		_ssaCore.LoadAndInstantiateAnchorsFromGroup(AnchorPrefab, groupUuid);
		IReadOnlyList<OVRSpatialAnchor> result2 = await task.Task;
		_ssaCore.OnSharedSpatialAnchorsLoadCompleted.RemoveListener(LoadCompletedCallback);
		return result2;
		void LoadCompletedCallback(List<OVRSpatialAnchor> loadedAnchors, OVRSpatialAnchor.OperationResult result)
		{
			if (result == OVRSpatialAnchor.OperationResult.Success)
			{
				_retrieveAnchorIsSuccessful = true;
				_sharedAnchors.AddRange(loadedAnchors);
				task.TrySetResult(loadedAnchors);
			}
		}
	}

	public async Task<IReadOnlyList<OVRSpatialAnchor>> RetrieveAnchors(List<Guid> anchorIds)
	{
		TaskCompletionSource<IReadOnlyList<OVRSpatialAnchor>> task = new TaskCompletionSource<IReadOnlyList<OVRSpatialAnchor>>();
		_retrieveAnchorIsSuccessful = false;
		CheckIfRetrievingAnchorServiceHung();
		Logger.Log("SharedAnchorManager: Querying anchors: " + string.Join(", ", anchorIds), LogLevel.Verbose);
		_ssaCore.OnSharedSpatialAnchorsLoadCompleted.AddListener(LoadCompletedCallback);
		_ssaCore.LoadAndInstantiateAnchors(AnchorPrefab, anchorIds);
		IReadOnlyList<OVRSpatialAnchor> result2 = await task.Task;
		_ssaCore.OnSharedSpatialAnchorsLoadCompleted.RemoveListener(LoadCompletedCallback);
		return result2;
		void LoadCompletedCallback(List<OVRSpatialAnchor> loadedAnchors, OVRSpatialAnchor.OperationResult result)
		{
			if (result == OVRSpatialAnchor.OperationResult.Success)
			{
				_retrieveAnchorIsSuccessful = true;
				_sharedAnchors.AddRange(loadedAnchors);
				task.TrySetResult(loadedAnchors);
			}
		}
	}

	private async void CheckIfRetrievingAnchorServiceHung()
	{
		await Task.Delay(10000);
		if (!_retrieveAnchorIsSuccessful)
		{
			Logger.Log(string.Format("{0}: It has been {1}ms since attempting to retrieve anchor(s). Anchors service may have failed", "SharedAnchorManager", 10000), LogLevel.Warning);
		}
	}

	public async Task<bool> ShareAnchorsWithGroup(Guid groupUuid)
	{
		_shareAnchorIsSuccessful = false;
		CheckIfSharingAnchorServiceHung();
		TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
		_ssaCore.OnSpatialAnchorsShareToGroupCompleted.AddListener(ShareToGroupCompletedCallback);
		_ssaCore.ShareSpatialAnchors(_localAnchors, groupUuid);
		bool result2 = await task.Task;
		_ssaCore.OnSpatialAnchorsShareToGroupCompleted.RemoveListener(ShareToGroupCompletedCallback);
		return result2;
		void ShareToGroupCompletedCallback(List<OVRSpatialAnchor> _, OVRAnchor.ShareResult result)
		{
			Logger.Log(string.Format("{0}: result of sharing the anchor is {1}", "SharedAnchorManager", result), LogLevel.Verbose);
			task.TrySetResult(result == OVRAnchor.ShareResult.Success);
			_shareAnchorIsSuccessful = true;
		}
	}

	public async Task<bool> ShareAnchorsWithUser(ulong userId)
	{
		if (!OVRSpaceUser.TryCreate(userId, out var spaceUser))
		{
			Logger.Log(string.Format("{0}: Failed to create space user using user id {1}.", "SharedAnchorManager", userId), LogLevel.Warning);
			return false;
		}
		_userShareList.Add(spaceUser);
		if (_localAnchors.Count == 0)
		{
			Logger.Log("SharedAnchorManager: No anchors to share.", LogLevel.Warning);
			return true;
		}
		Logger.Log(string.Format("{0}: Sharing {1} anchors with users: {2}", "SharedAnchorManager", _localAnchors.Count, userId), LogLevel.Verbose);
		List<OVRSpaceUser> list = new List<OVRSpaceUser>();
		list.AddRange(_userShareList);
		_shareAnchorIsSuccessful = false;
		CheckIfSharingAnchorServiceHung();
		TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
		_ssaCore.OnSpatialAnchorsShareCompleted.AddListener(ShareCompleteCallback);
		_ssaCore.ShareSpatialAnchors(_localAnchors, list);
		bool result2 = await task.Task;
		_ssaCore.OnSpatialAnchorsShareCompleted.RemoveListener(ShareCompleteCallback);
		return result2;
		void ShareCompleteCallback(List<OVRSpatialAnchor> _, OVRSpatialAnchor.OperationResult result)
		{
			Logger.Log(string.Format("{0}: result of sharing the anchor is {1}", "SharedAnchorManager", result), LogLevel.Verbose);
			task.TrySetResult(result == OVRSpatialAnchor.OperationResult.Success);
			_shareAnchorIsSuccessful = true;
		}
	}

	private async void CheckIfSharingAnchorServiceHung()
	{
		await Task.Delay(10000);
		if (!_shareAnchorIsSuccessful)
		{
			Logger.Log(string.Format("{0}: It has been {1}ms since attempting to share anchor(s). Anchors service may have failed", "SharedAnchorManager", 10000), LogLevel.Warning);
		}
	}

	public void StopSharingAnchorsWithUser(ulong userId)
	{
		_userShareList.RemoveWhere((OVRSpaceUser el) => el.Id == userId);
	}
}

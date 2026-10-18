using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Colocation;

internal class AutomaticColocationLauncher
{
	private GameObject _cameraRig;

	private TaskCompletionSource<bool> _alignToAnchorTask;

	private OVRSpatialAnchor _myAlignmentAnchor;

	private ulong _myPlayerId;

	private ulong _myOculusId;

	private INetworkData _networkData;

	private INetworkMessenger _networkMessenger;

	private ulong _oculusIdToColocateTo;

	private SharedAnchorManager _sharedAnchorManager;

	public event Action ColocationReady;

	public event Action<ColocationFailedReason> ColocationFailed;

	public void Init(INetworkData networkData, INetworkMessenger networkMessenger, SharedAnchorManager sharedAnchorManager, GameObject cameraRig, ulong myPlayerId, ulong myOculusId)
	{
		Logger.Log("AutomaticColocationLauncher: Init function called", LogLevel.Verbose);
		_networkData = networkData;
		_networkMessenger = networkMessenger;
		_networkMessenger.AnchorShareRequestReceived += OnAnchorShareRequestReceived;
		_networkMessenger.AnchorShareRequestCompleted += OnAnchorShareRequestCompleted;
		_sharedAnchorManager = sharedAnchorManager;
		_cameraRig = cameraRig;
		_myPlayerId = myPlayerId;
		_myOculusId = myOculusId;
	}

	public void ColocateAutomatically()
	{
		ColocateAutomaticallyInternal();
	}

	public void ColocateByPlayerWithOculusId(ulong oculusId)
	{
		ColocateByPlayerWithOculusIdInternal(oculusId);
	}

	public void CreateColocatedSpace()
	{
		CreateColocatedSpaceInternal();
	}

	private async void ColocateAutomaticallyInternal()
	{
		Logger.Log("AutomaticColocationLauncher: Called Init Anchor Flow", LogLevel.Verbose);
		bool successfullyAlignedToAnchor = false;
		List<Anchor> allAlignmentAnchors = GetAllAlignmentAnchors();
		foreach (Anchor anchor in allAlignmentAnchors)
		{
			if (await ShareAndLocalizeAnchor(anchor))
			{
				successfullyAlignedToAnchor = true;
				Logger.Log(string.Format("{0}: successfully aligned to anchor with id: {1}", "AutomaticColocationLauncher", anchor.automaticAnchorUuid), LogLevel.Info);
				_networkData.AddPlayer(new Player(_myPlayerId, _myOculusId, anchor.colocationGroupId));
				AlignPlayerToAnchor();
				this.ColocationReady?.Invoke();
				break;
			}
		}
		if (!successfullyAlignedToAnchor)
		{
			CreateNewColocatedSpace();
		}
	}

	private async void ColocateByPlayerWithOculusIdInternal(ulong oculusId)
	{
		Anchor? anchorToAlignTo = FindAlignmentAnchorUsedByOculusId(oculusId);
		if (!anchorToAlignTo.HasValue)
		{
			Logger.Log(string.Format("{0}: Unable to find alignment anchor used by oculusId {1}", "AutomaticColocationLauncher", oculusId), LogLevel.Error);
		}
		else if (await ShareAndLocalizeAnchor(anchorToAlignTo.Value))
		{
			Logger.Log(string.Format("{0}: successfully aligned to anchor with id: {1}", "AutomaticColocationLauncher", anchorToAlignTo.Value.automaticAnchorUuid), LogLevel.Verbose);
			_networkData.AddPlayer(new Player(_myPlayerId, _myOculusId, anchorToAlignTo.Value.colocationGroupId));
			AlignPlayerToAnchor();
			this.ColocationReady?.Invoke();
		}
		else
		{
			Logger.Log("AutomaticColocationLauncher: ColocateByPlayerWithOculusIdInternal: Failed to ShareAndLocalizeToAnchor", LogLevel.Verbose);
		}
	}

	private Anchor? FindAlignmentAnchorUsedByOculusId(ulong oculusId)
	{
		List<Player> allPlayers = _networkData.GetAllPlayers();
		uint? num = null;
		foreach (Player item in allPlayers)
		{
			if (oculusId == item.oculusId)
			{
				num = item.colocationGroupId;
			}
		}
		if (!num.HasValue)
		{
			Logger.Log(string.Format("{0}: Could not find the colocated group belonging to oculusId: {1}", "AutomaticColocationLauncher", oculusId), LogLevel.Error);
			return null;
		}
		foreach (Anchor allAnchor in _networkData.GetAllAnchors())
		{
			if (num.Value == allAnchor.colocationGroupId)
			{
				return allAnchor;
			}
		}
		Logger.Log(string.Format("{0}: Could not find the anchor belonging on colocationGroupId: {1}", "AutomaticColocationLauncher", num), LogLevel.Error);
		return null;
	}

	private void CreateColocatedSpaceInternal()
	{
		CreateNewColocatedSpace();
	}

	private async void CreateNewColocatedSpace()
	{
		_myAlignmentAnchor = await _sharedAnchorManager.CreateAlignmentAnchor();
		if (_myAlignmentAnchor == null)
		{
			Logger.Log("AutomaticColocationLauncher: Could not create the anchor", LogLevel.Error);
			this.ColocationFailed?.Invoke(ColocationFailedReason.AutomaticFailedToCreateAnchor);
			return;
		}
		uint colocationGroupCount = _networkData.GetColocationGroupCount();
		_networkData.IncrementColocationGroupCount();
		_networkData.AddAnchor(new Anchor(isAutomaticAnchor: true, isAlignmentAnchor: true, _myOculusId, colocationGroupCount, _myAlignmentAnchor.Uuid));
		_networkData.AddPlayer(new Player(_myPlayerId, _myOculusId, colocationGroupCount));
		AlignPlayerToAnchor();
		this.ColocationReady?.Invoke();
	}

	private void AlignPlayerToAnchor()
	{
		Logger.Log("AutomaticColocationLauncher AlignPlayerToAnchor was called", LogLevel.Verbose);
		AlignCameraToAnchor alignCameraToAnchor = _cameraRig.AddComponent<AlignCameraToAnchor>();
		alignCameraToAnchor.CameraAlignmentAnchor = _myAlignmentAnchor;
		alignCameraToAnchor.RealignToAnchor();
	}

	private List<Anchor> GetAllAlignmentAnchors()
	{
		List<Anchor> list = new List<Anchor>();
		foreach (Anchor allAnchor in _networkData.GetAllAnchors())
		{
			if (allAnchor.isAlignmentAnchor)
			{
				list.Add(allAnchor);
			}
		}
		return list;
	}

	private Task<bool> ShareAndLocalizeAnchor(Anchor anchor)
	{
		_alignToAnchorTask = new TaskCompletionSource<bool>();
		SendAnchorShareRequest(anchor);
		return _alignToAnchorTask.Task;
	}

	private void SendAnchorShareRequest(Anchor anchor)
	{
		Logger.Log(string.Format("{0}: Called {1} with anchor id: {2}, playerId: {3}, oculusId: {4}", "AutomaticColocationLauncher", "SendAnchorShareRequest", anchor.automaticAnchorUuid, _myPlayerId, _myOculusId), LogLevel.Verbose);
		Player? playerWithOculusId = _networkData.GetPlayerWithOculusId(anchor.ownerOculusId);
		if (!playerWithOculusId.HasValue)
		{
			Logger.Log(string.Format("{0}: Anchor owner {1} isn't connected.", "AutomaticColocationLauncher", anchor.ownerOculusId), LogLevel.Error);
			_alignToAnchorTask.TrySetResult(result: false);
		}
		ulong playerId = playerWithOculusId.Value.playerId;
		Logger.Log(string.Format("{0}: Request anchor sharing from playerId: {1}, oculusId: {2}", "AutomaticColocationLauncher", playerId, anchor.ownerOculusId), LogLevel.Info);
		_networkMessenger.SendAnchorShareRequest(playerId, new ShareAndLocalizeParams(_myPlayerId, _myOculusId, anchor.automaticAnchorUuid));
	}

	private async void OnAnchorShareRequestReceived(ShareAndLocalizeParams shareAndLocalizeParams)
	{
		Logger.Log(string.Format("{0}: Called {1} with playerId: {2}, oculusId: {3}", "AutomaticColocationLauncher", "OnAnchorShareRequestReceived", _myPlayerId, _myOculusId), LogLevel.Info);
		bool flag = await _sharedAnchorManager.ShareAnchorsWithUser(shareAndLocalizeParams.requestingPlayerOculusId);
		Logger.Log(string.Format("{0}: Anchor Shared: {1}", "AutomaticColocationLauncher", flag), LogLevel.Info);
		shareAndLocalizeParams.anchorFlowSucceeded = flag;
		_networkMessenger.SendAnchorShareCompleted(shareAndLocalizeParams.requestingPlayerId, shareAndLocalizeParams);
	}

	private void OnAnchorShareRequestCompleted(ShareAndLocalizeParams shareAndLocalizeParams)
	{
		Logger.Log(string.Format("{0}: Called {1} with playerId: {2}, oculusId: {3}", "AutomaticColocationLauncher", "OnAnchorShareRequestCompleted", _myPlayerId, _myOculusId), LogLevel.Info);
		if (!shareAndLocalizeParams.anchorFlowSucceeded)
		{
			Logger.Log("AutomaticColocationLauncher: Anchor flow failed.", LogLevel.Error);
			this.ColocationFailed?.Invoke(ColocationFailedReason.AutomaticFailedToShareAnchor);
			_alignToAnchorTask.TrySetResult(result: false);
		}
		else
		{
			Guid anchorToLocalize = new Guid(shareAndLocalizeParams.anchorUUID.ToString());
			LocalizeAnchor(anchorToLocalize);
		}
	}

	private async void LocalizeAnchor(Guid anchorToLocalize)
	{
		Logger.Log(string.Format("{0}: Localize Anchor Called id: {1}", "AutomaticColocationLauncher", _myOculusId), LogLevel.Verbose);
		List<Guid> anchorIds = new List<Guid> { anchorToLocalize };
		IReadOnlyList<OVRSpatialAnchor> readOnlyList = await _sharedAnchorManager.RetrieveAnchors(anchorIds);
		if (readOnlyList == null || readOnlyList.Count == 0)
		{
			Logger.Log("AutomaticColocationLauncher: Retrieving Anchors Failed", LogLevel.Error);
			this.ColocationFailed?.Invoke(ColocationFailedReason.AutomaticFailedToLocalizeAnchor);
			_alignToAnchorTask.TrySetResult(result: false);
		}
		else
		{
			Logger.Log("AutomaticColocationLauncher: Localizing Anchors is Successful", LogLevel.Verbose);
			_myAlignmentAnchor = readOnlyList[0];
			_alignToAnchorTask.TrySetResult(result: true);
		}
	}
}

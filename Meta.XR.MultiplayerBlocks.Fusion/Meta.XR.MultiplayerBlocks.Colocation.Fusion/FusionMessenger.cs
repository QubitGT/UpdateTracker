using System;
using System.Text;
using Fusion;
using UnityEngine.Scripting;

namespace Meta.XR.MultiplayerBlocks.Colocation.Fusion;

[NetworkBehaviourWeaved(76)]
internal class FusionMessenger : NetworkBehaviour, INetworkMessenger
{
	private enum MessageEvent
	{
		AnchorShareRequest,
		AnchorShareComplete
	}

	[WeaverGenerated]
	[DefaultForProperty("_networkIds", 0, 33)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private int[] __networkIds;

	[WeaverGenerated]
	[DefaultForProperty("_playerIds", 33, 43)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private ulong[] __playerIds;

	[Networked]
	[Capacity(10)]
	[NetworkedWeaved(0, 33)]
	[NetworkedWeavedLinkedList(10, 1, typeof(global::Fusion.ElementReaderWriterInt32))]
	private unsafe NetworkLinkedList<int> _networkIds
	{
		get
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing FusionMessenger._networkIds. Networked properties can only be accessed when Spawned() has been called.");
			}
			return new NetworkLinkedList<int>((byte*)base.Ptr + 0, 10, global::Fusion.ElementReaderWriterInt32.GetInstance());
		}
	}

	[Networked]
	[Capacity(10)]
	[NetworkedWeaved(33, 43)]
	[NetworkedWeavedLinkedList(10, 2, typeof(global::Fusion.ElementReaderWriterUInt64))]
	private unsafe NetworkLinkedList<ulong> _playerIds
	{
		get
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing FusionMessenger._playerIds. Networked properties can only be accessed when Spawned() has been called.");
			}
			return new NetworkLinkedList<ulong>((byte*)(base.Ptr + 33), 10, global::Fusion.ElementReaderWriterUInt64.GetInstance());
		}
	}

	public event Action<ShareAndLocalizeParams> AnchorShareRequestReceived;

	public event Action<ShareAndLocalizeParams> AnchorShareRequestCompleted;

	public void RegisterLocalPlayer(ulong localPlayerId)
	{
		Logger.Log(string.Format("{0}: RegisterLocalPlayer: localPlayerId {1}", "FusionMessenger", localPlayerId), LogLevel.Verbose);
		Logger.Log(string.Format("{0} RegisterLocalPlayer: fusionId {1}", "FusionMessenger", base.Runner.LocalPlayer.PlayerId), LogLevel.Verbose);
		AddPlayerIdHostRPC(localPlayerId, base.Runner.LocalPlayer.PlayerId);
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	private unsafe void AddPlayerIdHostRPC(ulong localPlayerId, int localNetworkId)
	{
		if (base.InvokeRpc)
		{
			base.InvokeRpc = false;
		}
		else
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage == SimulationStages.Resimulate)
			{
				return;
			}
			int localAuthorityMask = base.Object.GetLocalAuthorityMask();
			if ((localAuthorityMask & 7) == 0)
			{
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger::AddPlayerIdHostRPC(System.UInt64,System.Int32)", base.Object, 7);
				return;
			}
			if ((localAuthorityMask & 1) != 1)
			{
				int num = 8;
				num += 8;
				num += 4;
				if (!SimulationMessage.CanAllocateUserPayload(num))
				{
					NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger::AddPlayerIdHostRPC(System.UInt64,System.Int32)", num);
					return;
				}
				if (base.Runner.HasAnyActiveConnections())
				{
					SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
					byte* ptr2 = (byte*)ptr + 28;
					*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, base.ObjectIndex, 1);
					int num2 = 8;
					*(ulong*)(ptr2 + num2) = localPlayerId;
					num2 += 8;
					*(int*)(ptr2 + num2) = localNetworkId;
					num2 += 4;
					ptr->Offset = num2 * 8;
					base.Runner.SendRpc(ptr);
				}
				if ((localAuthorityMask & 1) == 0)
				{
					return;
				}
			}
		}
		Logger.Log("Add Player Id Host RPC: player id", LogLevel.Verbose);
		_playerIds.Add(localPlayerId);
		Logger.Log("Add Player Id Host RPC: network id", LogLevel.Verbose);
		_networkIds.Add(localNetworkId);
		PrintIDDictionary();
	}

	private bool TryGetNetworkId(ulong playerId, out int networkId)
	{
		for (int i = 0; i < _playerIds.Count; i++)
		{
			if (playerId == _playerIds[i])
			{
				networkId = _networkIds[i];
				return true;
			}
		}
		networkId = 0;
		Logger.Log($"FusionMessenger: playerId {playerId} got invalid networkId {networkId}", LogLevel.Error);
		return false;
	}

	public void SendAnchorShareRequest(ulong targetPlayerId, ShareAndLocalizeParams shareAndLocalizeParams)
	{
		Logger.Log(string.Format("{0}: Sending anchor share request to player {1}. (anchorID {2})", "FusionMessenger", targetPlayerId, shareAndLocalizeParams.anchorUUID), LogLevel.Verbose);
		FusionShareAndLocalizeParams fusionData = new FusionShareAndLocalizeParams(shareAndLocalizeParams);
		SendMessageToPlayer(MessageEvent.AnchorShareRequest, targetPlayerId, fusionData);
	}

	public void SendAnchorShareCompleted(ulong targetPlayerId, ShareAndLocalizeParams shareAndLocalizeParams)
	{
		Logger.Log(string.Format("{0}: Sending anchor share completed to player {1}. (anchorID {2})", "FusionMessenger", targetPlayerId, shareAndLocalizeParams.anchorUUID), LogLevel.Verbose);
		FusionShareAndLocalizeParams fusionData = new FusionShareAndLocalizeParams(shareAndLocalizeParams);
		SendMessageToPlayer(MessageEvent.AnchorShareComplete, targetPlayerId, fusionData);
	}

	private void SendMessageToPlayer(MessageEvent eventCode, ulong playerId, FusionShareAndLocalizeParams fusionData)
	{
		Logger.Log($"Calling SendMessageToPlayer with MessageEvent: {eventCode}, to playerId {playerId}", LogLevel.Verbose);
		if (TryGetNetworkId(playerId, out var networkId))
		{
			Logger.Log($"Calling FindRPCToCallServerRPC playerId {playerId} maps to fusionId {networkId}", LogLevel.Verbose);
			FindRPCToCallServerRPC(eventCode, networkId, fusionData);
		}
		else
		{
			Logger.Log($"Could not find fusionId for playerId {playerId}", LogLevel.Error);
		}
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	private unsafe void FindRPCToCallServerRPC(MessageEvent eventCode, int fusionId, FusionShareAndLocalizeParams fusionData, RpcInfo info = default(RpcInfo))
	{
		if (base.InvokeRpc)
		{
			base.InvokeRpc = false;
		}
		else
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage == SimulationStages.Resimulate)
			{
				return;
			}
			int localAuthorityMask = base.Object.GetLocalAuthorityMask();
			if ((localAuthorityMask & 7) == 0)
			{
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger::FindRPCToCallServerRPC(Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger/MessageEvent,System.Int32,Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionShareAndLocalizeParams,Fusion.RpcInfo)", base.Object, 7);
				return;
			}
			if ((localAuthorityMask & 1) != 1)
			{
				int num = 8;
				num += 4;
				num += 4;
				num += 280;
				if (!SimulationMessage.CanAllocateUserPayload(num))
				{
					NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger::FindRPCToCallServerRPC(Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger/MessageEvent,System.Int32,Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionShareAndLocalizeParams,Fusion.RpcInfo)", num);
					return;
				}
				if (base.Runner.HasAnyActiveConnections())
				{
					SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
					byte* ptr2 = (byte*)ptr + 28;
					*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, base.ObjectIndex, 2);
					int num2 = 8;
					*(MessageEvent*)(ptr2 + num2) = eventCode;
					num2 += 4;
					*(int*)(ptr2 + num2) = fusionId;
					num2 += 4;
					*(FusionShareAndLocalizeParams*)(ptr2 + num2) = fusionData;
					num2 += 280;
					ptr->Offset = num2 * 8;
					base.Runner.SendRpc(ptr);
				}
				if ((localAuthorityMask & 1) == 0)
				{
					return;
				}
			}
			info = RpcInfo.FromLocal(base.Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
		}
		Logger.Log("FindRPCToCallServerRPC called", LogLevel.Verbose);
		PlayerRef playerRef = PlayerRef.FromIndex(fusionId);
		Logger.Log("Created PlayerRef right before calling HandleMessageClientRPC", LogLevel.Verbose);
		HandleMessageClientRPC(playerRef, eventCode, fusionData);
	}

	[Rpc(RpcSources.All, RpcTargets.All)]
	private unsafe void HandleMessageClientRPC([RpcTarget] PlayerRef playerRef, MessageEvent eventCode, FusionShareAndLocalizeParams fusionData)
	{
		if (base.InvokeRpc)
		{
			base.InvokeRpc = false;
		}
		else
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage == SimulationStages.Resimulate)
			{
				return;
			}
			int localAuthorityMask = base.Object.GetLocalAuthorityMask();
			RpcTargetStatus rpcTargetStatus = base.Runner.GetRpcTargetStatus(playerRef);
			if (rpcTargetStatus == RpcTargetStatus.Unreachable)
			{
				NetworkBehaviourUtils.NotifyRpcTargetUnreachable(playerRef, "System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger::HandleMessageClientRPC(Fusion.PlayerRef,Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger/MessageEvent,Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionShareAndLocalizeParams)");
				return;
			}
			if (rpcTargetStatus != RpcTargetStatus.Self)
			{
				if ((localAuthorityMask & 7) == 0)
				{
					NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger::HandleMessageClientRPC(Fusion.PlayerRef,Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger/MessageEvent,Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionShareAndLocalizeParams)", base.Object, 7);
					return;
				}
				int num = 8;
				num += 4;
				num += 280;
				if (!SimulationMessage.CanAllocateUserPayload(num))
				{
					NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger::HandleMessageClientRPC(Fusion.PlayerRef,Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionMessenger/MessageEvent,Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionShareAndLocalizeParams)", num);
					return;
				}
				SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
				byte* ptr2 = (byte*)ptr + 28;
				*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, base.ObjectIndex, 3);
				int num2 = 8;
				*(MessageEvent*)(ptr2 + num2) = eventCode;
				num2 += 4;
				*(FusionShareAndLocalizeParams*)(ptr2 + num2) = fusionData;
				num2 += 280;
				ptr->Offset = num2 * 8;
				NetworkRunner runner = base.Runner;
				ptr->SetTarget(playerRef);
				runner.SendRpc(ptr);
				return;
			}
			if ((localAuthorityMask & 7) == 0)
			{
				return;
			}
		}
		Logger.Log("HandleMessageClientRPC: " + eventCode, LogLevel.Verbose);
		switch (eventCode)
		{
		case MessageEvent.AnchorShareRequest:
			this.AnchorShareRequestReceived?.Invoke(fusionData.GetShareAndLocalizeParams());
			break;
		case MessageEvent.AnchorShareComplete:
			this.AnchorShareRequestCompleted?.Invoke(fusionData.GetShareAndLocalizeParams());
			break;
		default:
			throw new ArgumentOutOfRangeException("eventCode", eventCode, null);
		}
	}

	private void PrintIDDictionary()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _playerIds.Count; i++)
		{
			stringBuilder.Append($"[{_playerIds[i]},{_networkIds[i]}]");
			if (i < _playerIds.Count - 1)
			{
				stringBuilder.Append(",");
			}
		}
		Logger.Log("FusionMessenger: ID dictionary is " + stringBuilder.ToString(), LogLevel.Verbose);
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool P_0)
	{
		NetworkBehaviourUtils.InitializeNetworkList(_networkIds, __networkIds, "_networkIds");
		NetworkBehaviourUtils.InitializeNetworkList(_playerIds, __playerIds, "_playerIds");
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		NetworkBehaviourUtils.CopyFromNetworkList(_networkIds, ref __networkIds);
		NetworkBehaviourUtils.CopyFromNetworkList(_playerIds, ref __playerIds);
	}

	[NetworkRpcWeavedInvoker(1, 7, 1)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void AddPlayerIdHostRPC_0040Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)message + 28;
		int num = 8;
		long num2 = *(long*)(ptr + num);
		num += 8;
		ulong localPlayerId = (ulong)num2;
		int num3 = *(int*)(ptr + num);
		num += 4;
		int localNetworkId = num3;
		behaviour.InvokeRpc = true;
		((FusionMessenger)behaviour).AddPlayerIdHostRPC(localPlayerId, localNetworkId);
	}

	[NetworkRpcWeavedInvoker(2, 7, 1)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void FindRPCToCallServerRPC_0040Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)message + 28;
		int num = 8;
		int num2 = *(int*)(ptr + num);
		num += 4;
		MessageEvent eventCode = (MessageEvent)num2;
		int num3 = *(int*)(ptr + num);
		num += 4;
		int fusionId = num3;
		FusionShareAndLocalizeParams fusionShareAndLocalizeParams = *(FusionShareAndLocalizeParams*)(ptr + num);
		num += 280;
		FusionShareAndLocalizeParams fusionData = fusionShareAndLocalizeParams;
		RpcInfo info = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((FusionMessenger)behaviour).FindRPCToCallServerRPC(eventCode, fusionId, fusionData, info);
	}

	[NetworkRpcWeavedInvoker(3, 7, 7)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void HandleMessageClientRPC_0040Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)message + 28;
		int num = 8;
		PlayerRef target = message->Target;
		int num2 = *(int*)(ptr + num);
		num += 4;
		MessageEvent eventCode = (MessageEvent)num2;
		FusionShareAndLocalizeParams fusionShareAndLocalizeParams = *(FusionShareAndLocalizeParams*)(ptr + num);
		num += 280;
		FusionShareAndLocalizeParams fusionData = fusionShareAndLocalizeParams;
		behaviour.InvokeRpc = true;
		((FusionMessenger)behaviour).HandleMessageClientRPC(target, eventCode, fusionData);
	}
}

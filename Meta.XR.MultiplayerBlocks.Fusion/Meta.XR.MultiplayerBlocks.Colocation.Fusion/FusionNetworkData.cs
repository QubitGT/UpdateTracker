using System;
using System.Collections.Generic;
using Fusion;
using Fusion.CodeGen;
using UnityEngine.Scripting;

namespace Meta.XR.MultiplayerBlocks.Colocation.Fusion;

[NetworkBehaviourWeaved(797)]
internal class FusionNetworkData : NetworkBehaviour, INetworkData
{
	[WeaverGenerated]
	[DefaultForProperty("ColocationGroupCount", 0, 1)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private uint _ColocationGroupCount;

	[WeaverGenerated]
	[DefaultForProperty("AnchorList", 1, 723)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private FusionAnchor[] _AnchorList;

	[WeaverGenerated]
	[DefaultForProperty("PlayerList", 724, 73)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private FusionPlayer[] _PlayerList;

	[Networked]
	[NetworkedWeaved(0, 1)]
	private unsafe uint ColocationGroupCount
	{
		get
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing FusionNetworkData.ColocationGroupCount. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(uint*)((byte*)base.Ptr + 0);
		}
		set
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing FusionNetworkData.ColocationGroupCount. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(uint*)((byte*)base.Ptr + 0) = value;
		}
	}

	[Networked]
	[Capacity(10)]
	[NetworkedWeaved(1, 723)]
	[NetworkedWeavedLinkedList(10, 70, typeof(ReaderWriter_0040Meta_XR_MultiplayerBlocks_Colocation_Fusion_FusionAnchor))]
	private unsafe NetworkLinkedList<FusionAnchor> AnchorList
	{
		get
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing FusionNetworkData.AnchorList. Networked properties can only be accessed when Spawned() has been called.");
			}
			return new NetworkLinkedList<FusionAnchor>((byte*)(base.Ptr + 1), 10, ReaderWriter_0040Meta_XR_MultiplayerBlocks_Colocation_Fusion_FusionAnchor.GetInstance());
		}
	}

	[Networked]
	[Capacity(10)]
	[NetworkedWeaved(724, 73)]
	[NetworkedWeavedLinkedList(10, 5, typeof(ReaderWriter_0040Meta_XR_MultiplayerBlocks_Colocation_Fusion_FusionPlayer))]
	private unsafe NetworkLinkedList<FusionPlayer> PlayerList
	{
		get
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing FusionNetworkData.PlayerList. Networked properties can only be accessed when Spawned() has been called.");
			}
			return new NetworkLinkedList<FusionPlayer>((byte*)(base.Ptr + 724), 10, ReaderWriter_0040Meta_XR_MultiplayerBlocks_Colocation_Fusion_FusionPlayer.GetInstance());
		}
	}

	public void AddPlayer(Player player)
	{
		AddFusionPlayer(new FusionPlayer(player));
	}

	public void RemovePlayer(Player player)
	{
		RemoveFusionPlayer(new FusionPlayer(player));
	}

	public Player? GetPlayerWithPlayerId(ulong playerId)
	{
		foreach (FusionPlayer player in PlayerList)
		{
			if (player.GetPlayer().playerId == playerId)
			{
				return player.GetPlayer();
			}
		}
		return null;
	}

	public Player? GetPlayerWithOculusId(ulong oculusId)
	{
		foreach (FusionPlayer player in PlayerList)
		{
			if (player.GetPlayer().oculusId == oculusId)
			{
				return player.GetPlayer();
			}
		}
		return null;
	}

	public List<Player> GetAllPlayers()
	{
		List<Player> list = new List<Player>();
		foreach (FusionPlayer player in PlayerList)
		{
			list.Add(player.GetPlayer());
		}
		return list;
	}

	public void AddAnchor(Anchor anchor)
	{
		AnchorList.Add(new FusionAnchor(anchor));
	}

	public void RemoveAnchor(Anchor anchor)
	{
		AnchorList.Remove(new FusionAnchor(anchor));
	}

	public Anchor? GetAnchor(ulong ownerOculusId)
	{
		foreach (FusionAnchor anchor in AnchorList)
		{
			if (anchor.GetAnchor().ownerOculusId == ownerOculusId)
			{
				return anchor.GetAnchor();
			}
		}
		return null;
	}

	public List<Anchor> GetAllAnchors()
	{
		List<Anchor> list = new List<Anchor>();
		foreach (FusionAnchor anchor in AnchorList)
		{
			list.Add(anchor.GetAnchor());
		}
		return list;
	}

	public uint GetColocationGroupCount()
	{
		return ColocationGroupCount;
	}

	public void IncrementColocationGroupCount()
	{
		if (base.HasStateAuthority)
		{
			ColocationGroupCount++;
		}
		else
		{
			IncrementColocationGroupCountRpc();
		}
	}

	private void AddFusionPlayer(FusionPlayer player)
	{
		if (base.HasStateAuthority)
		{
			PlayerList.Add(player);
		}
		else
		{
			AddPlayerRpc(player);
		}
	}

	private void RemoveFusionPlayer(FusionPlayer player)
	{
		if (base.HasStateAuthority)
		{
			PlayerList.Remove(player);
		}
		else
		{
			RemovePlayerRpc(player);
		}
	}

	private void AddFusionAnchor(FusionAnchor anchor)
	{
		if (base.HasStateAuthority)
		{
			AnchorList.Add(anchor);
		}
		else
		{
			AddAnchorRpc(anchor);
		}
	}

	private void RemoveFusionAnchor(FusionAnchor anchor)
	{
		if (base.HasStateAuthority)
		{
			AnchorList.Remove(anchor);
		}
		else
		{
			RemoveAnchorRpc(anchor);
		}
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	private unsafe void AddPlayerRpc(FusionPlayer player)
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
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionNetworkData::AddPlayerRpc(Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionPlayer)", base.Object, 7);
				return;
			}
			if ((localAuthorityMask & 1) != 1)
			{
				int num = 8;
				num += 20;
				if (!SimulationMessage.CanAllocateUserPayload(num))
				{
					NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionNetworkData::AddPlayerRpc(Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionPlayer)", num);
					return;
				}
				if (base.Runner.HasAnyActiveConnections())
				{
					SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
					byte* ptr2 = (byte*)ptr + 28;
					*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, base.ObjectIndex, 1);
					int num2 = 8;
					*(FusionPlayer*)(ptr2 + num2) = player;
					num2 += 20;
					ptr->Offset = num2 * 8;
					base.Runner.SendRpc(ptr);
				}
				if ((localAuthorityMask & 1) == 0)
				{
					return;
				}
			}
		}
		AddFusionPlayer(player);
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	private unsafe void RemovePlayerRpc(FusionPlayer player)
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
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionNetworkData::RemovePlayerRpc(Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionPlayer)", base.Object, 7);
				return;
			}
			if ((localAuthorityMask & 1) != 1)
			{
				int num = 8;
				num += 20;
				if (!SimulationMessage.CanAllocateUserPayload(num))
				{
					NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionNetworkData::RemovePlayerRpc(Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionPlayer)", num);
					return;
				}
				if (base.Runner.HasAnyActiveConnections())
				{
					SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
					byte* ptr2 = (byte*)ptr + 28;
					*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, base.ObjectIndex, 2);
					int num2 = 8;
					*(FusionPlayer*)(ptr2 + num2) = player;
					num2 += 20;
					ptr->Offset = num2 * 8;
					base.Runner.SendRpc(ptr);
				}
				if ((localAuthorityMask & 1) == 0)
				{
					return;
				}
			}
		}
		RemoveFusionPlayer(player);
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	private unsafe void AddAnchorRpc(FusionAnchor anchor)
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
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionNetworkData::AddAnchorRpc(Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionAnchor)", base.Object, 7);
				return;
			}
			if ((localAuthorityMask & 1) != 1)
			{
				int num = 8;
				num += 280;
				if (!SimulationMessage.CanAllocateUserPayload(num))
				{
					NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionNetworkData::AddAnchorRpc(Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionAnchor)", num);
					return;
				}
				if (base.Runner.HasAnyActiveConnections())
				{
					SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
					byte* ptr2 = (byte*)ptr + 28;
					*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, base.ObjectIndex, 3);
					int num2 = 8;
					*(FusionAnchor*)(ptr2 + num2) = anchor;
					num2 += 280;
					ptr->Offset = num2 * 8;
					base.Runner.SendRpc(ptr);
				}
				if ((localAuthorityMask & 1) == 0)
				{
					return;
				}
			}
		}
		AddFusionAnchor(anchor);
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	private unsafe void RemoveAnchorRpc(FusionAnchor anchor)
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
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionNetworkData::RemoveAnchorRpc(Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionAnchor)", base.Object, 7);
				return;
			}
			if ((localAuthorityMask & 1) != 1)
			{
				int num = 8;
				num += 280;
				if (!SimulationMessage.CanAllocateUserPayload(num))
				{
					NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionNetworkData::RemoveAnchorRpc(Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionAnchor)", num);
					return;
				}
				if (base.Runner.HasAnyActiveConnections())
				{
					SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
					byte* ptr2 = (byte*)ptr + 28;
					*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, base.ObjectIndex, 4);
					int num2 = 8;
					*(FusionAnchor*)(ptr2 + num2) = anchor;
					num2 += 280;
					ptr->Offset = num2 * 8;
					base.Runner.SendRpc(ptr);
				}
				if ((localAuthorityMask & 1) == 0)
				{
					return;
				}
			}
		}
		RemoveFusionAnchor(anchor);
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
	private unsafe void IncrementColocationGroupCountRpc()
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
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionNetworkData::IncrementColocationGroupCountRpc()", base.Object, 7);
				return;
			}
			if ((localAuthorityMask & 1) != 1)
			{
				int num = 8;
				if (!SimulationMessage.CanAllocateUserPayload(num))
				{
					NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void Meta.XR.MultiplayerBlocks.Colocation.Fusion.FusionNetworkData::IncrementColocationGroupCountRpc()", num);
					return;
				}
				if (base.Runner.HasAnyActiveConnections())
				{
					SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
					byte* ptr2 = (byte*)ptr + 28;
					*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, base.ObjectIndex, 5);
					int num2 = 8;
					ptr->Offset = num2 * 8;
					base.Runner.SendRpc(ptr);
				}
				if ((localAuthorityMask & 1) == 0)
				{
					return;
				}
			}
		}
		IncrementColocationGroupCount();
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool P_0)
	{
		ColocationGroupCount = _ColocationGroupCount;
		NetworkBehaviourUtils.InitializeNetworkList(AnchorList, _AnchorList, "AnchorList");
		NetworkBehaviourUtils.InitializeNetworkList(PlayerList, _PlayerList, "PlayerList");
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		_ColocationGroupCount = ColocationGroupCount;
		NetworkBehaviourUtils.CopyFromNetworkList(AnchorList, ref _AnchorList);
		NetworkBehaviourUtils.CopyFromNetworkList(PlayerList, ref _PlayerList);
	}

	[NetworkRpcWeavedInvoker(1, 7, 1)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void AddPlayerRpc_0040Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)message + 28;
		int num = 8;
		FusionPlayer fusionPlayer = *(FusionPlayer*)(ptr + num);
		num += 20;
		FusionPlayer player = fusionPlayer;
		behaviour.InvokeRpc = true;
		((FusionNetworkData)behaviour).AddPlayerRpc(player);
	}

	[NetworkRpcWeavedInvoker(2, 7, 1)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RemovePlayerRpc_0040Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)message + 28;
		int num = 8;
		FusionPlayer fusionPlayer = *(FusionPlayer*)(ptr + num);
		num += 20;
		FusionPlayer player = fusionPlayer;
		behaviour.InvokeRpc = true;
		((FusionNetworkData)behaviour).RemovePlayerRpc(player);
	}

	[NetworkRpcWeavedInvoker(3, 7, 1)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void AddAnchorRpc_0040Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)message + 28;
		int num = 8;
		FusionAnchor fusionAnchor = *(FusionAnchor*)(ptr + num);
		num += 280;
		FusionAnchor anchor = fusionAnchor;
		behaviour.InvokeRpc = true;
		((FusionNetworkData)behaviour).AddAnchorRpc(anchor);
	}

	[NetworkRpcWeavedInvoker(4, 7, 1)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RemoveAnchorRpc_0040Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)message + 28;
		int num = 8;
		FusionAnchor fusionAnchor = *(FusionAnchor*)(ptr + num);
		num += 280;
		FusionAnchor anchor = fusionAnchor;
		behaviour.InvokeRpc = true;
		((FusionNetworkData)behaviour).RemoveAnchorRpc(anchor);
	}

	[NetworkRpcWeavedInvoker(5, 7, 1)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void IncrementColocationGroupCountRpc_0040Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)message + 28;
		int num = 8;
		behaviour.InvokeRpc = true;
		((FusionNetworkData)behaviour).IncrementColocationGroupCountRpc();
	}
}

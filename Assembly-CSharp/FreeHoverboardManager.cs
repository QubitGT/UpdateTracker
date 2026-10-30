using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaNetworking;
using GorillaTag.CosmeticSystem;
using Photon.Pun;
using UnityEngine;

public class FreeHoverboardManager : NetworkSceneObject
{
	private struct DataPerPlayer
	{
		public FreeHoverboardInstance board0;

		public FreeHoverboardInstance board1;

		public CallLimiterWithCooldown spamCheck;

		public void Init(int actorNumber, Stack<FreeHoverboardInstance> freeBoardPool)
		{
			board0 = freeBoardPool.Pop();
			board0.ownerActorNumber = actorNumber;
			board0.boardIndex = 0;
			board1 = freeBoardPool.Pop();
			board1.ownerActorNumber = actorNumber;
			board1.boardIndex = 1;
			spamCheck = new CallLimiterWithCooldown(5f, 10, 1f);
		}

		public void ReturnBoards(Stack<FreeHoverboardInstance> freeBoardPool)
		{
			board0.gameObject.SetActive(value: false);
			freeBoardPool.Push(board0);
			board1.gameObject.SetActive(value: false);
			freeBoardPool.Push(board1);
		}

		public FreeHoverboardInstance GetBoard(int boardIndex)
		{
			if (boardIndex != 1)
			{
				return board0;
			}
			return board1;
		}
	}

	[SerializeField]
	private FreeHoverboardInstance freeHoverboardPrefab;

	private Stack<FreeHoverboardInstance> freeBoardPool = new Stack<FreeHoverboardInstance>(20);

	private const int NumPlayers = 10;

	private const int NumFreeBoardsPerPlayer = 2;

	private int localPlayerLastSpawnedBoardIndex;

	private Dictionary<int, DataPerPlayer> perPlayerData = new Dictionary<int, DataPerPlayer>();

	public static FreeHoverboardManager instance { get; private set; }

	private DataPerPlayer GetOrCreatePlayerData(int actorNumber)
	{
		if (!perPlayerData.TryGetValue(actorNumber, out var value))
		{
			value = default(DataPerPlayer);
			value.Init(actorNumber, freeBoardPool);
			perPlayerData.Add(actorNumber, value);
		}
		return value;
	}

	private void Awake()
	{
		instance = this;
		for (int i = 0; i < 20; i++)
		{
			FreeHoverboardInstance freeHoverboardInstance = UnityEngine.Object.Instantiate(freeHoverboardPrefab);
			freeHoverboardInstance.gameObject.SetActive(value: false);
			freeBoardPool.Push(freeHoverboardInstance);
		}
		NetworkSystem.Instance.OnPlayerLeft += new Action<NetPlayer>(OnPlayerLeftRoom);
		NetworkSystem.Instance.OnReturnedToSinglePlayer += new Action(OnLeftRoom);
		NetworkSystem.Instance.OnPlayerJoined += new Action<NetPlayer>(OnPlayerJoinedRoom);
	}

	private void OnPlayerJoinedRoom(NetPlayer netPlayer)
	{
		if (PhotonNetwork.InRoom && !(VRRig.LocalRig == null) && VRRig.LocalRig.hoverboardVisual.IsHeld)
		{
			string skinDisplayName = ((CosmeticsController.instance != null) ? CosmeticsController.instance.currentWornSet.items[32].displayName : null);
			SendHeldBoardSkinRPC(skinDisplayName);
		}
	}

	private void OnPlayerLeftRoom(NetPlayer netPlayer)
	{
		if (perPlayerData.TryGetValue(netPlayer.ActorNumber, out var value))
		{
			value.ReturnBoards(freeBoardPool);
			perPlayerData.Remove(netPlayer.ActorNumber);
		}
	}

	private void OnLeftRoom()
	{
		foreach (KeyValuePair<int, DataPerPlayer> perPlayerDatum in perPlayerData)
		{
			perPlayerDatum.Value.ReturnBoards(freeBoardPool);
		}
		perPlayerData.Clear();
	}

	private void SpawnBoard(DataPerPlayer playerData, int boardIndex, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 avelocity, Color boardColor, Vector3 globalScale, string skinDisplayName)
	{
		FreeHoverboardInstance obj = ((boardIndex == 0) ? playerData.board0 : playerData.board1);
		obj.transform.position = position;
		obj.transform.rotation = rotation;
		obj.transform.SetScale(globalScale);
		obj.Rigidbody.linearVelocity = velocity;
		obj.Rigidbody.angularVelocity = avelocity;
		obj.SetColor(boardColor);
		obj.ApplySkin(skinDisplayName);
		obj.gameObject.SetActive(value: true);
		if (obj.ownerActorNumber == NetworkSystem.Instance.LocalPlayer?.ActorNumber)
		{
			localPlayerLastSpawnedBoardIndex = boardIndex;
		}
	}

	public void SendHeldBoardSkinRPC(string skinDisplayName)
	{
		if (PhotonNetwork.InRoom)
		{
			photonView.RPC("HeldBoardSkin_RPC", RpcTarget.All, skinDisplayName ?? string.Empty);
		}
		else
		{
			VRRig.LocalRig.heldBoardSkinDisplayName = skinDisplayName;
		}
	}

	[PunRPC]
	public void HeldBoardSkin_RPC(string skinDisplayName, PhotonMessageInfo info)
	{
		MonkeAgent.IncrementRPCCall(info, "HeldBoardSkin_RPC");
		if (info.Sender != PhotonNetwork.LocalPlayer && perPlayerData.TryGetValue(info.Sender.ActorNumber, out var value) && !value.spamCheck.CheckCallTime(Time.unscaledTime))
		{
			return;
		}
		if (string.IsNullOrEmpty(skinDisplayName))
		{
			skinDisplayName = null;
		}
		if (skinDisplayName != null)
		{
			CosmeticSO cosmeticSO = CosmeticsController.instance?.GetCosmeticSOFromDisplayName(skinDisplayName);
			if (cosmeticSO == null || cosmeticSO.info.category.Value != CosmeticsController.CosmeticCategory.Hoverboard)
			{
				skinDisplayName = null;
			}
		}
		RigContainer playerRig;
		VRRig vRRig = ((info.Sender == PhotonNetwork.LocalPlayer) ? VRRig.LocalRig : (VRRigCache.Instance.TryGetVrrig(info.Sender, out playerRig) ? playerRig.Rig : null));
		if (!(vRRig == null))
		{
			vRRig.heldBoardSkinDisplayName = skinDisplayName;
			if (vRRig.hoverboardVisual.IsHeld)
			{
				vRRig.hoverboardVisual.ApplySkin(skinDisplayName);
			}
		}
	}

	public void SendDropBoardRPC(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 avelocity, Color boardColor, Vector3 globalScale)
	{
		DataPerPlayer orCreatePlayerData = GetOrCreatePlayerData(NetworkSystem.Instance.LocalPlayer.ActorNumber);
		int num = (orCreatePlayerData.board0.gameObject.activeSelf ? ((!orCreatePlayerData.board1.gameObject.activeSelf) ? 1 : (1 - localPlayerLastSpawnedBoardIndex)) : 0);
		string text = ((CosmeticsController.instance != null) ? CosmeticsController.instance.currentWornSet.items[32].displayName : null);
		if (PhotonNetwork.InRoom)
		{
			long num2 = BitPackUtils.PackWorldPosForNetwork(position);
			int num3 = BitPackUtils.PackQuaternionForNetwork(rotation);
			long num4 = BitPackUtils.PackWorldPosForNetwork(velocity);
			long num5 = BitPackUtils.PackWorldPosForNetwork(avelocity);
			short num6 = BitPackUtils.PackColorForNetwork(boardColor);
			photonView.RPC("DropBoard_RPC", RpcTarget.All, num == 1, num2, num3, num4, num5, num6, text ?? string.Empty);
		}
		else
		{
			SpawnBoard(orCreatePlayerData, num, position, rotation, velocity, avelocity, boardColor, globalScale, text);
		}
	}

	[PunRPC]
	public void DropBoard_RPC(bool boardIndex1, long positionPacked, int rotationPacked, long velocityPacked, long avelocityPacked, short colorPacked, string skinDisplayName, PhotonMessageInfo info)
	{
		MonkeAgent.IncrementRPCCall(info, "DropBoard_RPC");
		int boardIndex2 = (boardIndex1 ? 1 : 0);
		DataPerPlayer orCreatePlayerData = GetOrCreatePlayerData(info.Sender.ActorNumber);
		if ((info.Sender != PhotonNetwork.LocalPlayer && !orCreatePlayerData.spamCheck.CheckCallTime(Time.unscaledTime)) || !VRRigCache.Instance.TryGetVrrig(info.Sender, out var playerRig))
		{
			return;
		}
		Vector3 position = BitPackUtils.UnpackWorldPosFromNetwork(positionPacked);
		if (!playerRig.Rig.IsPositionInRange(position, 5f))
		{
			return;
		}
		if (!string.IsNullOrEmpty(skinDisplayName) && skinDisplayName != "NOTHING")
		{
			CosmeticSO cosmeticSO = CosmeticsController.instance?.GetCosmeticSOFromDisplayName(skinDisplayName);
			if (cosmeticSO == null || cosmeticSO.info.category.Value != CosmeticsController.CosmeticCategory.Hoverboard)
			{
				skinDisplayName = null;
			}
		}
		SpawnBoard(orCreatePlayerData, boardIndex2, position, BitPackUtils.UnpackQuaternionFromNetwork(rotationPacked), BitPackUtils.UnpackWorldPosFromNetwork(velocityPacked), BitPackUtils.UnpackWorldPosFromNetwork(avelocityPacked), BitPackUtils.UnpackColorFromNetwork(colorPacked), playerRig.transform.lossyScale, skinDisplayName);
	}

	public void SendGrabBoardRPC(FreeHoverboardInstance board)
	{
		if (PhotonNetwork.InRoom)
		{
			photonView.RPC("GrabBoard_RPC", RpcTarget.All, board.ownerActorNumber, board.boardIndex == 1);
			board.gameObject.SetActive(value: false);
		}
		else
		{
			board.gameObject.SetActive(value: false);
		}
	}

	[PunRPC]
	public void GrabBoard_RPC(int ownerActorNumber, bool boardIndex1, PhotonMessageInfo info)
	{
		MonkeAgent.IncrementRPCCall(info, "GrabBoard_RPC");
		int boardIndex2 = (boardIndex1 ? 1 : 0);
		if (NetworkSystem.Instance.GetNetPlayerByID(ownerActorNumber) == null)
		{
			return;
		}
		DataPerPlayer orCreatePlayerData = GetOrCreatePlayerData(ownerActorNumber);
		if (info.Sender == PhotonNetwork.LocalPlayer || orCreatePlayerData.spamCheck.CheckCallTime(Time.unscaledTime))
		{
			FreeHoverboardInstance board = orCreatePlayerData.GetBoard(boardIndex2);
			if (!board.IsNull() && (info.Sender.ActorNumber == ownerActorNumber || (VRRigCache.Instance.TryGetVrrig(info.Sender, out var playerRig) && playerRig.Rig.IsPositionInRange(board.transform.position, 5f))))
			{
				board.gameObject.SetActive(value: false);
			}
		}
	}

	public void PreserveMaxHoverboardsConstraint(int actorNumber)
	{
		if (perPlayerData.TryGetValue(actorNumber, out var value) && value.board0.gameObject.activeSelf && value.board1.gameObject.activeSelf)
		{
			FreeHoverboardInstance board = value.GetBoard(1 - localPlayerLastSpawnedBoardIndex);
			SendGrabBoardRPC(board);
		}
	}
}

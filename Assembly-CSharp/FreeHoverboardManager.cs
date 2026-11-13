using System;
using System.Collections.Generic;
using GorillaExtensions;
using GorillaNetworking;
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

	private List<FreeHoverboardInstance> allBoards = new List<FreeHoverboardInstance>(20);

	private const int NumPlayers = 10;

	public const int NoSkinActorNumber = 0;

	private int cachedLocalActorNumber;

	private const int NumFreeBoardsPerPlayer = 2;

	private int localPlayerLastSpawnedBoardIndex;

	private Dictionary<int, DataPerPlayer> perPlayerData = new Dictionary<int, DataPerPlayer>();

	public static FreeHoverboardManager instance { get; private set; }

	private static int LocalActorNumber => NetworkSystem.Instance.LocalPlayer?.ActorNumber ?? 0;

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
			allBoards.Add(freeHoverboardInstance);
		}
		cachedLocalActorNumber = LocalActorNumber;
		NetworkSystem.Instance.OnPlayerLeft += new Action<NetPlayer>(OnPlayerLeftRoom);
		NetworkSystem.Instance.OnReturnedToSinglePlayer += new Action(OnLeftRoom);
		NetworkSystem.Instance.OnMultiplayerStarted += new Action(OnLocalActorNumberMayHaveChanged);
	}

	public static string GetSkinDisplayName(int skinActorNumber)
	{
		if (skinActorNumber == 0 || CosmeticsController.instance == null)
		{
			return null;
		}
		NetPlayer localPlayer = NetworkSystem.Instance.LocalPlayer;
		VRRig vRRig;
		if (localPlayer != null && skinActorNumber == localPlayer.ActorNumber)
		{
			vRRig = VRRig.LocalRig;
		}
		else
		{
			NetPlayer netPlayerByID = NetworkSystem.Instance.GetNetPlayerByID(skinActorNumber);
			if (netPlayerByID == null || !VRRigCache.Instance.TryGetVrrig(netPlayerByID, out var playerRig))
			{
				return null;
			}
			vRRig = playerRig.Rig;
		}
		string text = ((vRRig != null) ? vRRig.mergedSet : null)?.items[32].displayName;
		if (!string.IsNullOrEmpty(text) && !(text == "NOTHING"))
		{
			return text;
		}
		return null;
	}

	private void RevertSkinsOwnedBy(int skinActorNumber)
	{
		foreach (FreeHoverboardInstance allBoard in allBoards)
		{
			allBoard.RevertSkinIfOwnedBy(skinActorNumber);
		}
		foreach (VRRig activeRig in VRRigCache.ActiveRigs)
		{
			activeRig.hoverboardVisual.RevertSkinIfOwnedBy(skinActorNumber);
		}
		if (VRRig.LocalRig != null)
		{
			VRRig.LocalRig.hoverboardVisual.RevertSkinIfOwnedBy(skinActorNumber);
		}
	}

	private void OnLocalActorNumberMayHaveChanged()
	{
		int localActorNumber = LocalActorNumber;
		if (localActorNumber != cachedLocalActorNumber && VRRig.LocalRig != null)
		{
			HoverboardVisual hoverboardVisual = VRRig.LocalRig.hoverboardVisual;
			hoverboardVisual.SetSkinActorNumber((hoverboardVisual.boardSkinActorNumber == cachedLocalActorNumber) ? localActorNumber : 0);
		}
		cachedLocalActorNumber = localActorNumber;
	}

	private void OnPlayerLeftRoom(NetPlayer netPlayer)
	{
		if (perPlayerData.TryGetValue(netPlayer.ActorNumber, out var value))
		{
			value.ReturnBoards(freeBoardPool);
			perPlayerData.Remove(netPlayer.ActorNumber);
		}
		RevertSkinsOwnedBy(netPlayer.ActorNumber);
	}

	private void OnLeftRoom()
	{
		foreach (KeyValuePair<int, DataPerPlayer> perPlayerDatum in perPlayerData)
		{
			perPlayerDatum.Value.ReturnBoards(freeBoardPool);
		}
		perPlayerData.Clear();
		OnLocalActorNumberMayHaveChanged();
	}

	private void SpawnBoard(DataPerPlayer playerData, int boardIndex, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 avelocity, Color boardColor, Vector3 globalScale, int skinActorNumber, bool droppedByLocalPlayer)
	{
		FreeHoverboardInstance freeHoverboardInstance = ((boardIndex == 0) ? playerData.board0 : playerData.board1);
		freeHoverboardInstance.transform.position = position;
		freeHoverboardInstance.transform.rotation = rotation;
		freeHoverboardInstance.transform.SetScale(globalScale);
		freeHoverboardInstance.Rigidbody.linearVelocity = velocity;
		freeHoverboardInstance.Rigidbody.angularVelocity = avelocity;
		freeHoverboardInstance.SetColor(boardColor);
		freeHoverboardInstance.gameObject.SetActive(value: true);
		if (droppedByLocalPlayer && VRRig.LocalRig != null)
		{
			BoardSkinVisual orCreateSkinVisual = VRRig.LocalRig.hoverboardVisual.GetOrCreateSkinVisual();
			string skinDisplayName = GetSkinDisplayName(skinActorNumber);
			if (skinDisplayName != null && orCreateSkinVisual.SkinDisplayName == skinDisplayName && orCreateSkinVisual.TryTransferSkinTo(freeHoverboardInstance.GetOrCreateSkinVisual()))
			{
				orCreateSkinVisual.TryAttachPendingSkin();
			}
		}
		freeHoverboardInstance.SetSkinActorNumber(skinActorNumber);
		if (freeHoverboardInstance.ownerActorNumber == NetworkSystem.Instance.LocalPlayer?.ActorNumber)
		{
			localPlayerLastSpawnedBoardIndex = boardIndex;
		}
	}

	public void SendDropBoardRPC(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 avelocity, Color boardColor, Vector3 globalScale, int skinActorNumber)
	{
		DataPerPlayer orCreatePlayerData = GetOrCreatePlayerData(NetworkSystem.Instance.LocalPlayer.ActorNumber);
		int num = (orCreatePlayerData.board0.gameObject.activeSelf ? ((!orCreatePlayerData.board1.gameObject.activeSelf) ? 1 : (1 - localPlayerLastSpawnedBoardIndex)) : 0);
		if (PhotonNetwork.InRoom)
		{
			long num2 = BitPackUtils.PackWorldPosForNetwork(position);
			int num3 = BitPackUtils.PackQuaternionForNetwork(rotation);
			long num4 = BitPackUtils.PackWorldPosForNetwork(velocity);
			long num5 = BitPackUtils.PackWorldPosForNetwork(avelocity);
			short num6 = BitPackUtils.PackColorForNetwork(boardColor);
			photonView.RPC("DropBoard_RPC", RpcTarget.All, num == 1, num2, num3, num4, num5, num6, skinActorNumber);
		}
		else
		{
			SpawnBoard(orCreatePlayerData, num, position, rotation, velocity, avelocity, boardColor, globalScale, skinActorNumber, droppedByLocalPlayer: true);
		}
	}

	[PunRPC]
	public void DropBoard_RPC(bool boardIndex1, long positionPacked, int rotationPacked, long velocityPacked, long avelocityPacked, short colorPacked, int skinActorNumber, PhotonMessageInfo info)
	{
		MonkeAgent.IncrementRPCCall(info, "DropBoard_RPC");
		int boardIndex2 = (boardIndex1 ? 1 : 0);
		DataPerPlayer orCreatePlayerData = GetOrCreatePlayerData(info.Sender.ActorNumber);
		if ((info.Sender != PhotonNetwork.LocalPlayer && !orCreatePlayerData.spamCheck.CheckCallTime(Time.unscaledTime)) || !VRRigCache.Instance.TryGetVrrig(info.Sender, out var playerRig))
		{
			return;
		}
		Vector3 position = BitPackUtils.UnpackWorldPosFromNetwork(positionPacked);
		if (playerRig.Rig.IsPositionInRange(position, 5f))
		{
			if (NetworkSystem.Instance.GetNetPlayerByID(skinActorNumber) == null)
			{
				skinActorNumber = 0;
			}
			SpawnBoard(orCreatePlayerData, boardIndex2, position, BitPackUtils.UnpackQuaternionFromNetwork(rotationPacked), BitPackUtils.UnpackWorldPosFromNetwork(velocityPacked), BitPackUtils.UnpackWorldPosFromNetwork(avelocityPacked), BitPackUtils.UnpackColorFromNetwork(colorPacked), playerRig.transform.lossyScale, skinActorNumber, info.Sender == PhotonNetwork.LocalPlayer);
		}
	}

	public void SendGrabBoardRPC(FreeHoverboardInstance board, bool isDespawn = false)
	{
		if (!isDespawn && VRRig.LocalRig != null && board.gameObject.activeSelf)
		{
			board.GetOrCreateSkinVisual().TryTransferSkinToPending(VRRig.LocalRig.hoverboardVisual.GetOrCreateSkinVisual());
		}
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
			SendGrabBoardRPC(board, isDespawn: true);
		}
	}
}

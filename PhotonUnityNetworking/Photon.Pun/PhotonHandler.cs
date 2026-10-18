using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Pun;

public class PhotonHandler : ConnectionHandler, IInRoomCallbacks, IMatchmakingCallbacks
{
	private static PhotonHandler instance;

	public static int MaxDatagrams = 3;

	public static bool SendAsap;

	private const int SerializeRateFrameCorrection = 8;

	protected internal int UpdateInterval;

	protected internal int UpdateIntervalOnSerialize;

	private int nextSendTickCount;

	private int nextSendTickCountOnSerialize;

	private SupportLogger supportLoggerComponent;

	protected List<int> reusableIntList = new List<int>();

	internal static PhotonHandler Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindAnyObjectByType<PhotonHandler>();
				if (instance == null)
				{
					instance = new GameObject
					{
						name = "PhotonMono"
					}.AddComponent<PhotonHandler>();
				}
			}
			return instance;
		}
	}

	protected override void Awake()
	{
		if (instance == null || (object)this == instance)
		{
			instance = this;
			base.Awake();
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	protected virtual void OnEnable()
	{
		if (Instance != this)
		{
			Debug.LogError("PhotonHandler is a singleton but there are multiple instances. this != Instance.");
			return;
		}
		base.Client = PhotonNetwork.NetworkingClient;
		if (PhotonNetwork.PhotonServerSettings.EnableSupportLogger)
		{
			SupportLogger supportLogger = base.gameObject.GetComponent<SupportLogger>();
			if (supportLogger == null)
			{
				supportLogger = base.gameObject.AddComponent<SupportLogger>();
			}
			if (supportLoggerComponent != null && supportLogger.GetInstanceID() != supportLoggerComponent.GetInstanceID())
			{
				Debug.LogWarningFormat("Cached SupportLogger component is different from the one attached to PhotonMono GameObject");
			}
			supportLoggerComponent = supportLogger;
			supportLoggerComponent.Client = PhotonNetwork.NetworkingClient;
		}
		UpdateInterval = 1000 / PhotonNetwork.SendRate;
		UpdateIntervalOnSerialize = 1000 / PhotonNetwork.SerializationRate;
		PhotonNetwork.AddCallbackTarget(this);
		StartFallbackSendAckThread();
	}

	protected void Start()
	{
		SceneManager.sceneLoaded += delegate
		{
		};
	}

	protected override void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
		base.OnDisable();
	}

	protected void FixedUpdate()
	{
		if (Time.timeScale > PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate)
		{
			Dispatch();
		}
	}

	protected void LateUpdate()
	{
		if (Time.timeScale <= PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate)
		{
			Dispatch();
		}
		int num = (int)(Time.realtimeSinceStartup * 1000f);
		if (PhotonNetwork.IsMessageQueueRunning && num > nextSendTickCountOnSerialize)
		{
			PhotonNetwork.RunViewUpdate();
			nextSendTickCountOnSerialize = num + UpdateIntervalOnSerialize - 8;
			nextSendTickCount = 0;
		}
		num = (int)(Time.realtimeSinceStartup * 1000f);
		if (SendAsap || num > nextSendTickCount)
		{
			SendAsap = false;
			bool flag = true;
			int num2 = 0;
			while (PhotonNetwork.IsMessageQueueRunning && flag && num2 < MaxDatagrams)
			{
				flag = PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
				num2++;
			}
			nextSendTickCount = num + UpdateInterval;
		}
	}

	protected void Dispatch()
	{
		if (PhotonNetwork.NetworkingClient == null)
		{
			Debug.LogError("NetworkPeer broke!");
			return;
		}
		bool flag = true;
		Exception ex = null;
		int num = 0;
		while (PhotonNetwork.IsMessageQueueRunning && flag)
		{
			try
			{
				flag = PhotonNetwork.NetworkingClient.LoadBalancingPeer.DispatchIncomingCommands();
			}
			catch (Exception ex2)
			{
				num++;
				if (ex == null)
				{
					ex = ex2;
				}
			}
		}
		if (ex == null)
		{
			return;
		}
		throw new AggregateException("Caught " + num + " exception(s) in methods called by DispatchIncomingCommands(). Rethrowing first only (see above).", ex);
	}

	public void OnCreatedRoom()
	{
		PhotonNetwork.SetLevelInPropsIfSynced(SceneManagerHelper.ActiveSceneName);
	}

	public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		PhotonNetwork.LoadLevelIfSynced();
	}

	public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	public void OnMasterClientSwitched(Player newMasterClient)
	{
		foreach (PhotonView item in PhotonNetwork.PhotonViewCollection)
		{
			if (item.IsRoomView)
			{
				item.OwnerActorNr = newMasterClient.ActorNumber;
				item.ControllerActorNr = newMasterClient.ActorNumber;
			}
		}
	}

	public void OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	public void OnCreateRoomFailed(short returnCode, string message)
	{
	}

	public void OnJoinRoomFailed(short returnCode, string message)
	{
	}

	public void OnJoinRandomFailed(short returnCode, string message)
	{
	}

	public void OnJoinedRoom()
	{
		if (PhotonNetwork.ViewCount == 0)
		{
			return;
		}
		foreach (PhotonView item in PhotonNetwork.PhotonViewCollection)
		{
			item.RebuildControllerCache();
		}
	}

	public void OnLeftRoom()
	{
		PhotonNetwork.LocalCleanupAnythingInstantiated(destroyInstantiatedGameObjects: true);
	}

	public void OnPreLeavingRoom()
	{
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
		NonAllocDictionary<int, PhotonView>.ValueIterator photonViewCollection = PhotonNetwork.PhotonViewCollection;
		int actorNumber = otherPlayer.ActorNumber;
		if (otherPlayer.IsInactive)
		{
			foreach (PhotonView item in photonViewCollection)
			{
				if (item.ControllerActorNr == actorNumber)
				{
					item.ControllerActorNr = PhotonNetwork.MasterClient.ActorNumber;
				}
			}
			return;
		}
		bool autoCleanUp = PhotonNetwork.CurrentRoom.AutoCleanUp;
		foreach (PhotonView item2 in photonViewCollection)
		{
			if ((!autoCleanUp || item2.CreatorActorNr != actorNumber) && (item2.OwnerActorNr == actorNumber || item2.ControllerActorNr == actorNumber))
			{
				item2.OwnerActorNr = 0;
				item2.ControllerActorNr = PhotonNetwork.MasterClient.ActorNumber;
			}
		}
	}
}

using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public class PunTurnManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	private Player sender;

	public float TurnDuration = 20f;

	public IPunTurnManagerCallbacks TurnManagerListener;

	private readonly HashSet<Player> finishedPlayers = new HashSet<Player>();

	public const byte TurnManagerEventOffset = 0;

	public const byte EvMove = 1;

	public const byte EvFinalMove = 2;

	private bool _isOverCallProcessed;

	public int Turn
	{
		get
		{
			return PhotonNetwork.CurrentRoom.GetTurn();
		}
		private set
		{
			_isOverCallProcessed = false;
			PhotonNetwork.CurrentRoom.SetTurn(value, setStartTime: true);
		}
	}

	public float ElapsedTimeInTurn => (float)(PhotonNetwork.ServerTimestamp - PhotonNetwork.CurrentRoom.GetTurnStart()) / 1000f;

	public float RemainingSecondsInTurn => Mathf.Max(0f, TurnDuration - ElapsedTimeInTurn);

	public bool IsCompletedByAll
	{
		get
		{
			if (PhotonNetwork.CurrentRoom != null && Turn > 0)
			{
				return finishedPlayers.Count == PhotonNetwork.CurrentRoom.PlayerCount;
			}
			return false;
		}
	}

	public bool IsFinishedByMe => finishedPlayers.Contains(PhotonNetwork.LocalPlayer);

	public bool IsOver => RemainingSecondsInTurn <= 0f;

	private void Start()
	{
	}

	private void Update()
	{
		if (Turn > 0 && IsOver && !_isOverCallProcessed)
		{
			_isOverCallProcessed = true;
			TurnManagerListener.OnTurnTimeEnds(Turn);
		}
	}

	public void BeginTurn()
	{
		Turn++;
	}

	public void SendMove(object move, bool finished)
	{
		if (IsFinishedByMe)
		{
			Debug.LogWarning("Can't SendMove. Turn is finished by this player.");
			return;
		}
		Hashtable hashtable = new Hashtable();
		hashtable.Add("turn", Turn);
		hashtable.Add("move", move);
		byte eventCode = (byte)((!finished) ? 1 : 2);
		PhotonNetwork.RaiseEvent(eventCode, hashtable, new RaiseEventOptions
		{
			CachingOption = EventCaching.AddToRoomCache
		}, SendOptions.SendReliable);
		if (finished)
		{
			PhotonNetwork.LocalPlayer.SetFinishedTurn(Turn);
		}
		ProcessOnEvent(eventCode, hashtable, PhotonNetwork.LocalPlayer.ActorNumber);
	}

	public bool GetPlayerFinishedTurn(Player player)
	{
		if (player != null && finishedPlayers != null && finishedPlayers.Contains(player))
		{
			return true;
		}
		return false;
	}

	private void ProcessOnEvent(byte eventCode, object content, int senderId)
	{
		if (senderId == -1)
		{
			return;
		}
		sender = PhotonNetwork.CurrentRoom.GetPlayer(senderId);
		switch (eventCode)
		{
		case 1:
		{
			Hashtable obj2 = content as Hashtable;
			int turn = (int)obj2["turn"];
			object move2 = obj2["move"];
			TurnManagerListener.OnPlayerMove(sender, turn, move2);
			break;
		}
		case 2:
		{
			Hashtable obj = content as Hashtable;
			int num = (int)obj["turn"];
			object move = obj["move"];
			if (num == Turn)
			{
				finishedPlayers.Add(sender);
				TurnManagerListener.OnPlayerFinished(sender, num, move);
			}
			if (IsCompletedByAll)
			{
				TurnManagerListener.OnTurnCompleted(Turn);
			}
			break;
		}
		}
	}

	public void OnEvent(EventData photonEvent)
	{
		ProcessOnEvent(photonEvent.Code, photonEvent.CustomData, photonEvent.Sender);
	}

	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		if (propertiesThatChanged.ContainsKey("Turn"))
		{
			_isOverCallProcessed = false;
			finishedPlayers.Clear();
			TurnManagerListener.OnTurnBegins(Turn);
		}
	}
}

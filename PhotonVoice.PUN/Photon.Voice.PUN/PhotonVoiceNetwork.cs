using System;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine;

namespace Photon.Voice.PUN;

[DisallowMultipleComponent]
[AddComponentMenu("Photon Voice/Photon Voice Network")]
[HelpURL("https://doc.photonengine.com/en-us/voice/v2/getting-started/voice-for-pun")]
public class PhotonVoiceNetwork : VoiceConnection
{
	public const string VoiceRoomNameSuffix = "_voice_";

	public bool AutoConnectAndJoin = true;

	public bool AutoLeaveAndDisconnect = true;

	public bool WorkInOfflineMode = true;

	private EnterRoomParams voiceRoomParams = new EnterRoomParams
	{
		RoomOptions = new RoomOptions
		{
			IsVisible = false,
			PublishUserId = true
		}
	};

	private bool clientCalledConnectAndJoin;

	private bool clientCalledDisconnect;

	private bool clientCalledConnectOnly;

	private bool internalDisconnect;

	private bool internalConnect;

	private static object instanceLock = new object();

	private static PhotonVoiceNetwork instance;

	private static bool instantiated;

	[SerializeField]
	private bool usePunAppSettings = true;

	[SerializeField]
	private bool usePunAuthValues = true;

	public static PhotonVoiceNetwork Instance
	{
		get
		{
			lock (instanceLock)
			{
				if (ConnectionHandler.AppQuits)
				{
					if (instance.Logger.IsWarningEnabled)
					{
						instance.Logger.LogWarning("PhotonVoiceNetwork Instance already destroyed on application quit. Won't create again - returning null.");
					}
					return null;
				}
				if (!instantiated)
				{
					PhotonVoiceNetwork[] array = UnityEngine.Object.FindObjectsByType<PhotonVoiceNetwork>(FindObjectsSortMode.None);
					if (array == null || array.Length < 1)
					{
						instance = new GameObject
						{
							name = "PhotonVoiceNetwork singleton"
						}.AddComponent<PhotonVoiceNetwork>();
						if (instance.Logger.IsInfoEnabled)
						{
							instance.Logger.LogInfo("An instance of PhotonVoiceNetwork was automatically created in the scene.");
						}
					}
					else if (array.Length >= 1)
					{
						instance = array[0];
						if (array.Length > 1)
						{
							if (instance.Logger.IsErrorEnabled)
							{
								instance.Logger.LogError("{0} PhotonVoiceNetwork instances found. Using first one only and destroying all the other extra instances.", array.Length);
							}
							for (int i = 1; i < array.Length; i++)
							{
								UnityEngine.Object.Destroy(array[i]);
							}
						}
					}
					instantiated = true;
					if (instance.Logger.IsDebugEnabled)
					{
						instance.Logger.LogDebug("PhotonVoiceNetwork singleton instance is now set.");
					}
				}
				return instance;
			}
		}
		set
		{
			lock (instanceLock)
			{
				if ((object)value == null || !value)
				{
					if (instantiated)
					{
						if (instance.Logger.IsErrorEnabled)
						{
							instance.Logger.LogError("Cannot set PhotonVoiceNetwork.Instance to null or destroyed.");
						}
					}
					else
					{
						Debug.LogError("Cannot set PhotonVoiceNetwork.Instance to null or destroyed.");
					}
				}
				else if (instantiated)
				{
					if (instance.GetInstanceID() != value.GetInstanceID())
					{
						if (instance.Logger.IsErrorEnabled)
						{
							instance.Logger.LogError("An instance of PhotonVoiceNetwork is already set. Destroying extra instance.");
						}
						UnityEngine.Object.Destroy(value);
					}
				}
				else
				{
					instance = value;
					instantiated = true;
					if (instance.Logger.IsDebugEnabled)
					{
						instance.Logger.LogDebug("PhotonVoiceNetwork singleton instance is now set.");
					}
				}
			}
		}
	}

	public bool UsePunAuthValues
	{
		get
		{
			return usePunAuthValues;
		}
		set
		{
			usePunAuthValues = value;
		}
	}

	public bool ConnectAndJoinRoom()
	{
		if (!PhotonNetwork.InRoom)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Cannot connect and join if PUN is not joined.");
			}
			return false;
		}
		if (Connect())
		{
			clientCalledConnectAndJoin = true;
			clientCalledDisconnect = false;
			return true;
		}
		if (base.Logger.IsErrorEnabled)
		{
			base.Logger.LogError("Connecting to server failed.");
		}
		return false;
	}

	public void Disconnect()
	{
		if (!base.Client.IsConnected)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Cannot Disconnect if not connected.");
			}
		}
		else
		{
			clientCalledDisconnect = true;
			clientCalledConnectAndJoin = false;
			clientCalledConnectOnly = false;
			base.Client.Disconnect();
		}
	}

	protected override void Awake()
	{
		Instance = this;
		lock (instanceLock)
		{
			if (instantiated && instance.GetInstanceID() == GetInstanceID())
			{
				base.Awake();
			}
		}
	}

	private void OnEnable()
	{
		PhotonNetwork.NetworkingClient.StateChanged += OnPunStateChanged;
		FollowPun();
		clientCalledConnectAndJoin = false;
		clientCalledConnectOnly = false;
		clientCalledDisconnect = false;
		internalDisconnect = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		PhotonNetwork.NetworkingClient.StateChanged -= OnPunStateChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		lock (instanceLock)
		{
			if (instantiated && instance.GetInstanceID() == GetInstanceID())
			{
				instantiated = false;
				if (instance.Logger.IsDebugEnabled)
				{
					instance.Logger.LogDebug("PhotonVoiceNetwork singleton instance is being reset because destroyed.");
				}
				instance = null;
			}
		}
	}

	private void OnPunStateChanged(ClientState fromState, ClientState toState)
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("OnPunStateChanged from {0} to {1}", fromState, toState);
		}
		FollowPun(toState);
	}

	protected override void OnVoiceStateChanged(ClientState fromState, ClientState toState)
	{
		base.OnVoiceStateChanged(fromState, toState);
		switch (toState)
		{
		case ClientState.Disconnected:
			if (internalDisconnect)
			{
				internalDisconnect = false;
			}
			else if (!clientCalledDisconnect)
			{
				clientCalledDisconnect = base.Client.DisconnectedCause == DisconnectCause.DisconnectByClientLogic;
			}
			if ((object)base.PrimaryRecorder != null && (bool)base.PrimaryRecorder)
			{
				base.PrimaryRecorder.UserData = -1;
			}
			break;
		case ClientState.ConnectedToMasterServer:
			if (internalConnect)
			{
				internalConnect = false;
			}
			else if (!clientCalledConnectOnly && !clientCalledConnectAndJoin)
			{
				clientCalledConnectOnly = true;
				clientCalledDisconnect = false;
			}
			break;
		}
		FollowPun(toState);
	}

	private void FollowPun(ClientState toState)
	{
		if (toState == ClientState.Joined || (uint)(toState - 14) <= 1u)
		{
			FollowPun();
		}
	}

	protected override Speaker SimpleSpeakerFactory(int playerId, byte voiceId, object userData)
	{
		if (!(userData is int))
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("UserData ({0}) does not contain PhotonViewId. Remote voice {1}/{2} not linked. Do you have a Recorder not used with a PhotonVoiceView? is this expected?", (userData == null) ? "null" : userData.ToString(), playerId, voiceId);
			}
			return null;
		}
		PhotonView photonView = PhotonView.Find((int)userData);
		if ((object)photonView == null || !photonView)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("No PhotonView with ID {0} found. Remote voice {1}/{2} not linked.", userData, playerId, voiceId);
			}
			return null;
		}
		PhotonVoiceView component = photonView.GetComponent<PhotonVoiceView>();
		if ((object)component == null || !component)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("No PhotonVoiceView attached to the PhotonView with ID {0}. Remote voice {1}/{2} not linked.", userData, playerId, voiceId);
			}
			return null;
		}
		if (!component.IgnoreGlobalLogLevel)
		{
			component.LogLevel = base.LogLevel;
		}
		if (!component.IsSpeaker)
		{
			component.SetupSpeakerInUse();
		}
		return component.SpeakerInUse;
	}

	internal static string GetVoiceRoomName()
	{
		if (PhotonNetwork.InRoom)
		{
			return string.Format("{0}{1}", PhotonNetwork.CurrentRoom.Name, "_voice_");
		}
		return null;
	}

	private void ConnectOrJoin()
	{
		switch (base.ClientState)
		{
		case ClientState.PeerCreated:
		case ClientState.Disconnected:
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("PUN joined room, now connecting Voice client");
			}
			if (!Connect())
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Connecting to server failed.");
				}
			}
			else
			{
				internalConnect = AutoConnectAndJoin && !clientCalledConnectOnly && !clientCalledConnectAndJoin;
			}
			break;
		case ClientState.ConnectedToMasterServer:
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("PUN joined room, now joining Voice room");
			}
			if (!JoinRoom(GetVoiceRoomName()) && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Joining a voice room failed.");
			}
			break;
		default:
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("PUN joined room, Voice client is busy ({0}). Is this expected?", base.ClientState);
			}
			break;
		}
	}

	private bool Connect()
	{
		AppSettings appSettings = null;
		if (usePunAppSettings)
		{
			appSettings = new AppSettings();
			appSettings = PhotonNetwork.PhotonServerSettings.AppSettings.CopyTo(appSettings);
			if (!string.IsNullOrEmpty(PhotonNetwork.CloudRegion))
			{
				appSettings.FixedRegion = PhotonNetwork.CloudRegion;
			}
			base.Client.SerializationProtocol = PhotonNetwork.NetworkingClient.SerializationProtocol;
		}
		if (UsePunAuthValues)
		{
			if (PhotonNetwork.AuthValues != null)
			{
				if (base.Client.AuthValues == null)
				{
					base.Client.AuthValues = new AuthenticationValues();
				}
				base.Client.AuthValues = PhotonNetwork.AuthValues.CopyTo(base.Client.AuthValues);
			}
			base.Client.AuthMode = PhotonNetwork.NetworkingClient.AuthMode;
			base.Client.EncryptionMode = PhotonNetwork.NetworkingClient.EncryptionMode;
		}
		return ConnectUsingSettings(appSettings);
	}

	private bool JoinRoom(string voiceRoomName)
	{
		if (string.IsNullOrEmpty(voiceRoomName))
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Voice room name is null or empty.");
			}
			return false;
		}
		voiceRoomParams.RoomName = voiceRoomName;
		return base.Client.OpJoinOrCreateRoom(voiceRoomParams);
	}

	private void FollowPun()
	{
		if (ConnectionHandler.AppQuits || (PhotonNetwork.OfflineMode && !WorkInOfflineMode))
		{
			return;
		}
		if (PhotonNetwork.NetworkClientState == base.ClientState)
		{
			if (PhotonNetwork.InRoom && AutoConnectAndJoin)
			{
				string voiceRoomName = GetVoiceRoomName();
				string text = base.Client.CurrentRoom.Name;
				if (!text.Equals(voiceRoomName))
				{
					if (base.Logger.IsWarningEnabled)
					{
						base.Logger.LogWarning("Voice room mismatch: Expected:\"{0}\" Current:\"{1}\", leaving the second to join the first.", voiceRoomName, text);
					}
					if (!base.Client.OpLeaveRoom(becomeInactive: false) && base.Logger.IsErrorEnabled)
					{
						base.Logger.LogError("Leaving the current voice room failed.");
					}
				}
			}
			else if (base.ClientState == ClientState.ConnectedToMasterServer && AutoLeaveAndDisconnect && !clientCalledConnectAndJoin && !clientCalledConnectOnly)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Unexpected: PUN and Voice clients have the same client state: ConnectedToMasterServer, Disconnecting Voice client.");
				}
				internalDisconnect = true;
				base.Client.Disconnect();
			}
		}
		else if (PhotonNetwork.InRoom)
		{
			if (clientCalledConnectAndJoin || (AutoConnectAndJoin && !clientCalledDisconnect))
			{
				ConnectOrJoin();
			}
		}
		else if (base.Client.InRoom && AutoLeaveAndDisconnect && !clientCalledConnectAndJoin && !clientCalledConnectOnly)
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("PUN left room, disconnecting Voice");
			}
			internalDisconnect = true;
			base.Client.Disconnect();
		}
	}

	internal void CheckLateLinking(Speaker speaker, int viewId)
	{
		if ((object)speaker == null || !speaker)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot check late linking for null Speaker");
			}
			return;
		}
		if (viewId <= 0)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot check late linking for ViewID = {0} (<= 0)", viewId);
			}
			return;
		}
		if (!base.Client.InRoom)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot check late linking while not joined to a voice room, client state: {0}", Enum.GetName(typeof(ClientState), base.ClientState));
			}
			return;
		}
		for (int i = 0; i < cachedRemoteVoices.Count; i++)
		{
			RemoteVoiceLink remoteVoiceLink = cachedRemoteVoices[i];
			if (remoteVoiceLink.Info.UserData is int)
			{
				int num = (int)remoteVoiceLink.Info.UserData;
				if (viewId == num)
				{
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Speaker 'late-linking' for the PhotonView with ID {0} with remote voice {1}/{2}.", viewId, remoteVoiceLink.PlayerId, remoteVoiceLink.VoiceId);
					}
					LinkSpeaker(speaker, remoteVoiceLink);
					break;
				}
			}
			else if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("VoiceInfo.UserData should be int/ViewId, received: {0}, do you have a Recorder not used with a PhotonVoiceView? is this expected?", (remoteVoiceLink.Info.UserData == null) ? "null" : $"{remoteVoiceLink.Info.UserData} ({remoteVoiceLink.Info.UserData.GetType()})");
				if (remoteVoiceLink.PlayerId == viewId / PhotonNetwork.MAX_VIEW_IDS)
				{
					base.Logger.LogWarning("Player with ActorNumber {0} has started recording (voice # {1}) too early without setting a ViewId maybe? (before PhotonVoiceView setup)", remoteVoiceLink.PlayerId, remoteVoiceLink.VoiceId);
				}
			}
		}
	}
}

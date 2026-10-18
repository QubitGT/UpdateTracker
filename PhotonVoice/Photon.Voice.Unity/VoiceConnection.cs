using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Photon.Voice.Unity;

[AddComponentMenu("Photon Voice/Voice Connection")]
[DisallowMultipleComponent]
[HelpURL("https://doc.photonengine.com/en-us/voice/v2/getting-started/voice-intro")]
public class VoiceConnection : ConnectionHandler, ILoggable
{
	public delegate bool ValidateRemoteLinkDelegate(RemoteVoiceLink link);

	private VoiceLogger logger;

	[SerializeField]
	private DebugLevel logLevel = DebugLevel.INFO;

	private const string PlayerPrefsKey = "VoiceCloudBestRegion";

	private LoadBalancingTransport client;

	[SerializeField]
	private bool enableSupportLogger;

	private SupportLogger supportLoggerComponent;

	[SerializeField]
	private int updateInterval = 50;

	private int nextSendTickCount;

	[SerializeField]
	private bool runInBackground = true;

	[SerializeField]
	private int statsResetInterval = 1000;

	private int nextStatsTickCount;

	private float statsReferenceTime;

	private int referenceFramesLost;

	private int referenceFramesReceived;

	[SerializeField]
	private GameObject speakerPrefab;

	private bool cleanedUp;

	protected List<RemoteVoiceLink> cachedRemoteVoices = new List<RemoteVoiceLink>();

	[SerializeField]
	[FormerlySerializedAs("PrimaryRecorder")]
	private Recorder primaryRecorder;

	private bool primaryRecorderInitialized;

	[SerializeField]
	private DebugLevel globalRecordersLogLevel = DebugLevel.INFO;

	[SerializeField]
	private DebugLevel globalSpeakersLogLevel = DebugLevel.INFO;

	[SerializeField]
	[HideInInspector]
	private int globalPlaybackDelay = 200;

	[SerializeField]
	private PlaybackDelaySettings globalPlaybackDelaySettings = new PlaybackDelaySettings
	{
		MinDelaySoft = 200,
		MaxDelaySoft = 400,
		MaxDelayHard = 1000
	};

	private List<Speaker> linkedSpeakers = new List<Speaker>();

	private List<Recorder> initializedRecorders = new List<Recorder>();

	public AppSettings Settings;

	public Func<int, byte, object, Speaker> SpeakerFactory;

	public ValidateRemoteLinkDelegate RemoteLinkValidator;

	public float MinimalTimeScaleToDispatchInFixedUpdate = -1f;

	public bool AutoCreateSpeakerIfNotFound = true;

	public int MaxDatagrams = 3;

	public bool SendAsap;

	public VoiceLogger Logger
	{
		get
		{
			if (logger == null)
			{
				logger = new VoiceLogger(this, $"{base.name}.{GetType().Name}", logLevel);
			}
			return logger;
		}
		protected set
		{
			logger = value;
		}
	}

	public DebugLevel LogLevel
	{
		get
		{
			if (Logger != null)
			{
				logLevel = Logger.LogLevel;
			}
			return logLevel;
		}
		set
		{
			logLevel = value;
			if (Logger != null)
			{
				Logger.LogLevel = logLevel;
			}
		}
	}

	public new LoadBalancingTransport Client
	{
		get
		{
			if (client == null)
			{
				client = new LoadBalancingTransport2(Logger);
				client.ClientType = ClientAppType.Voice;
				VoiceClient voiceClient = client.VoiceClient;
				voiceClient.OnRemoteVoiceInfoAction = (VoiceClient.RemoteVoiceInfoDelegate)Delegate.Combine(voiceClient.OnRemoteVoiceInfoAction, new VoiceClient.RemoteVoiceInfoDelegate(OnRemoteVoiceInfo));
				client.StateChanged += OnVoiceStateChanged;
				client.OpResponseReceived += OnOperationResponseReceived;
				base.Client = client;
				StartFallbackSendAckThread();
			}
			return client;
		}
	}

	public VoiceClient VoiceClient => Client.VoiceClient;

	public ClientState ClientState => Client.State;

	public float FramesReceivedPerSecond { get; private set; }

	public float FramesLostPerSecond { get; private set; }

	public float FramesLostPercent { get; private set; }

	public GameObject SpeakerPrefab
	{
		get
		{
			return speakerPrefab;
		}
		set
		{
			if (!(value != speakerPrefab))
			{
				return;
			}
			if ((object)value != null && (bool)value)
			{
				Speaker componentInChildren = value.GetComponentInChildren<Speaker>(includeInactive: true);
				if ((object)componentInChildren == null || !componentInChildren)
				{
					if (Logger.IsErrorEnabled)
					{
						Logger.LogError("SpeakerPrefab must have a component of type Speaker in its hierarchy.");
					}
					return;
				}
			}
			speakerPrefab = value;
		}
	}

	public Recorder PrimaryRecorder
	{
		get
		{
			if (!primaryRecorderInitialized)
			{
				TryInitializePrimaryRecorder();
			}
			return primaryRecorder;
		}
		set
		{
			primaryRecorder = value;
			primaryRecorderInitialized = false;
			TryInitializePrimaryRecorder();
		}
	}

	public DebugLevel GlobalRecordersLogLevel
	{
		get
		{
			return globalRecordersLogLevel;
		}
		set
		{
			globalRecordersLogLevel = value;
			for (int i = 0; i < initializedRecorders.Count; i++)
			{
				Recorder recorder = initializedRecorders[i];
				if (!recorder.IgnoreGlobalLogLevel)
				{
					recorder.LogLevel = globalRecordersLogLevel;
				}
			}
		}
	}

	public DebugLevel GlobalSpeakersLogLevel
	{
		get
		{
			return globalSpeakersLogLevel;
		}
		set
		{
			globalSpeakersLogLevel = value;
			for (int i = 0; i < linkedSpeakers.Count; i++)
			{
				Speaker speaker = linkedSpeakers[i];
				if (!speaker.IgnoreGlobalLogLevel)
				{
					speaker.LogLevel = globalSpeakersLogLevel;
				}
			}
		}
	}

	[Obsolete("Use SetGlobalPlaybackDelayConfiguration methods instead")]
	public int GlobalPlaybackDelay
	{
		get
		{
			return globalPlaybackDelaySettings.MinDelaySoft;
		}
		set
		{
			if (value >= 0 && value <= globalPlaybackDelaySettings.MaxDelaySoft)
			{
				globalPlaybackDelaySettings.MinDelaySoft = value;
			}
		}
	}

	public string BestRegionSummaryInPreferences
	{
		get
		{
			return PlayerPrefs.GetString("VoiceCloudBestRegion", null);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				PlayerPrefs.DeleteKey("VoiceCloudBestRegion");
			}
			else
			{
				PlayerPrefs.SetString("VoiceCloudBestRegion", value);
			}
		}
	}

	public int GlobalPlaybackDelayMinSoft => globalPlaybackDelaySettings.MinDelaySoft;

	public int GlobalPlaybackDelayMaxSoft => globalPlaybackDelaySettings.MaxDelaySoft;

	public int GlobalPlaybackDelayMaxHard => globalPlaybackDelaySettings.MaxDelayHard;

	public event Action<Speaker> SpeakerLinked;

	public event Action<RemoteVoiceLink> RemoteVoiceAdded;

	public bool ConnectUsingSettings(AppSettings overwriteSettings = null)
	{
		if (Client.LoadBalancingPeer.PeerState != 0)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("ConnectUsingSettings() failed. Can only connect while in state 'Disconnected'. Current state: {0}", Client.LoadBalancingPeer.PeerState);
			}
			return false;
		}
		if (ConnectionHandler.AppQuits)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Can't connect: Application is closing. Unity called OnApplicationQuit().");
			}
			return false;
		}
		if (overwriteSettings != null)
		{
			Settings = overwriteSettings;
		}
		if (Settings == null)
		{
			if (Logger.IsErrorEnabled)
			{
				Logger.LogError("Settings are null");
			}
			return false;
		}
		if (string.IsNullOrEmpty(Settings.AppIdVoice) && string.IsNullOrEmpty(Settings.Server))
		{
			if (Logger.IsErrorEnabled)
			{
				Logger.LogError("Provide an AppId or a Server address in Settings to be able to connect");
			}
			return false;
		}
		if (Settings.IsMasterServerAddress && string.IsNullOrEmpty(Client.UserId))
		{
			Client.UserId = Guid.NewGuid().ToString();
		}
		if (string.IsNullOrEmpty(Settings.BestRegionSummaryFromStorage))
		{
			Settings.BestRegionSummaryFromStorage = BestRegionSummaryInPreferences;
		}
		return client.ConnectUsingSettings(Settings);
	}

	public void InitRecorder(Recorder rec)
	{
		if ((object)rec == null)
		{
			if (Logger.IsErrorEnabled)
			{
				Logger.LogError("rec is null.");
			}
		}
		else if (!rec)
		{
			if (Logger.IsErrorEnabled)
			{
				Logger.LogError("rec is destroyed.");
			}
		}
		else
		{
			rec.Init(this);
		}
	}

	public void SetPlaybackDelaySettings(PlaybackDelaySettings gpds)
	{
		SetGlobalPlaybackDelaySettings(gpds.MinDelaySoft, gpds.MaxDelaySoft, gpds.MaxDelayHard);
	}

	public void SetGlobalPlaybackDelaySettings(int low, int high, int max)
	{
		if (low >= 0 && low < high)
		{
			if (max < high)
			{
				max = high;
			}
			globalPlaybackDelaySettings.MinDelaySoft = low;
			globalPlaybackDelaySettings.MaxDelaySoft = high;
			globalPlaybackDelaySettings.MaxDelayHard = max;
			for (int i = 0; i < linkedSpeakers.Count; i++)
			{
				linkedSpeakers[i].SetPlaybackDelaySettings(globalPlaybackDelaySettings);
			}
		}
		else if (Logger.IsErrorEnabled)
		{
			Logger.LogError("Wrong playback delay config values, make sure 0 <= Low < High, low={0}, high={1}, max={2}", low, high, max);
		}
	}

	public virtual bool TryLateLinkingUsingUserData(Speaker speaker, object userData)
	{
		if ((object)speaker == null || !speaker)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Speaker is null or destroyed.");
			}
			return false;
		}
		if (speaker.IsLinked)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Speaker already linked.");
			}
			return false;
		}
		if (!Client.InRoom)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Client not joined to a voice room, client state: {0}.", Enum.GetName(typeof(ClientState), ClientState));
			}
			return false;
		}
		if (TryGetFirstVoiceStreamByUserData(userData, out var remoteVoiceLink))
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.LogInfo("Speaker 'late-linking' for remoteVoice {0}.", remoteVoiceLink);
			}
			LinkSpeaker(speaker, remoteVoiceLink);
			return speaker.IsLinked;
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		if (enableSupportLogger)
		{
			supportLoggerComponent = base.gameObject.AddComponent<SupportLogger>();
			supportLoggerComponent.Client = Client;
			supportLoggerComponent.LogTrafficStats = true;
		}
		if (runInBackground)
		{
			Application.runInBackground = runInBackground;
		}
		if (!primaryRecorderInitialized)
		{
			TryInitializePrimaryRecorder();
		}
	}

	protected virtual void Update()
	{
		VoiceClient.Service();
		for (int i = 0; i < linkedSpeakers.Count; i++)
		{
			linkedSpeakers[i].Service();
		}
		for (int j = 0; j < initializedRecorders.Count; j++)
		{
			Recorder recorder = initializedRecorders[j];
			if (recorder.MicrophoneDeviceChangeDetected)
			{
				recorder.HandleDeviceChange();
			}
		}
	}

	protected virtual void FixedUpdate()
	{
		if (Time.timeScale > MinimalTimeScaleToDispatchInFixedUpdate)
		{
			Dispatch();
		}
	}

	protected void Dispatch()
	{
		bool flag = true;
		while (flag)
		{
			flag = Client.LoadBalancingPeer.DispatchIncomingCommands();
		}
	}

	private void LateUpdate()
	{
		if (Time.timeScale <= MinimalTimeScaleToDispatchInFixedUpdate)
		{
			Dispatch();
		}
		int num = (int)(Time.realtimeSinceStartup * 1000f);
		if (SendAsap || num > nextSendTickCount)
		{
			SendAsap = false;
			bool flag = true;
			int num2 = 0;
			while (flag && num2 < MaxDatagrams)
			{
				flag = Client.LoadBalancingPeer.SendOutgoingCommands();
				num2++;
			}
			nextSendTickCount = num + updateInterval;
		}
		if (num > nextStatsTickCount && statsResetInterval > 0)
		{
			CalcStatistics();
			nextStatsTickCount = num + statsResetInterval;
		}
	}

	protected override void OnDisable()
	{
		if (ConnectionHandler.AppQuits)
		{
			CleanUp();
			SupportClass.StopAllBackgroundCalls();
		}
	}

	protected virtual void OnDestroy()
	{
		CleanUp();
	}

	protected virtual Speaker SimpleSpeakerFactory(int playerId, byte voiceId, object userData)
	{
		Speaker speaker = null;
		bool flag = false;
		if ((object)SpeakerPrefab != null && (bool)SpeakerPrefab)
		{
			Speaker[] componentsInChildren = UnityEngine.Object.Instantiate(SpeakerPrefab).GetComponentsInChildren<Speaker>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				speaker = componentsInChildren[0];
				if (componentsInChildren.Length > 1 && Logger.IsWarningEnabled)
				{
					Logger.LogWarning("Multiple Speaker components found attached to the GameObject (VoiceConnection.SpeakerPrefab) or its children. Using the first one we found.");
				}
			}
			if ((object)speaker == null)
			{
				if (Logger.IsErrorEnabled)
				{
					Logger.LogError("Unexpected: SpeakerPrefab does not have a component of type Speaker in its hierarchy.");
				}
			}
			else
			{
				flag = true;
			}
		}
		if (!flag)
		{
			if (!AutoCreateSpeakerIfNotFound)
			{
				return null;
			}
			if (Logger.IsInfoEnabled)
			{
				Logger.LogInfo("Auto creating a new Speaker as none found");
			}
			speaker = new GameObject().AddComponent<Speaker>();
		}
		speaker.Actor = ((Client.CurrentRoom != null) ? Client.CurrentRoom.GetPlayer(playerId) : null);
		speaker.name = ((speaker.Actor != null && !string.IsNullOrEmpty(speaker.Actor.NickName)) ? speaker.Actor.NickName : $"Speaker for Player {playerId} Voice #{voiceId}");
		Speaker speaker2 = speaker;
		speaker2.OnRemoteVoiceRemoveAction = (Action<Speaker>)Delegate.Combine(speaker2.OnRemoteVoiceRemoveAction, new Action<Speaker>(DeleteVoiceOnRemoteVoiceRemove));
		return speaker;
	}

	internal void DeleteVoiceOnRemoteVoiceRemove(Speaker speaker)
	{
		if (speaker != null)
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.LogInfo("Remote voice removed, delete speaker");
			}
			UnityEngine.Object.Destroy(speaker.gameObject);
		}
	}

	private void OnRemoteVoiceInfo(int channelId, int playerId, byte voiceId, VoiceInfo voiceInfo, ref RemoteVoiceOptions options)
	{
		RemoteVoiceLink remoteVoice = new RemoteVoiceLink(voiceInfo, playerId, voiceId, channelId);
		if (RemoteLinkValidator != null && !RemoteLinkValidator(remoteVoice))
		{
			return;
		}
		if (voiceInfo.Codec != Codec.AudioOpus)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.LogInfo("OnRemoteVoiceInfo skipped as codec is not Opus, {0}", remoteVoice);
			}
			return;
		}
		remoteVoice.Init(ref options);
		if (Logger.IsInfoEnabled)
		{
			Logger.LogInfo("OnRemoteVoiceInfo {0}", remoteVoice);
		}
		for (int i = 0; i < cachedRemoteVoices.Count; i++)
		{
			RemoteVoiceLink remoteVoiceLink = cachedRemoteVoices[i];
			if (remoteVoiceLink.Equals(remoteVoice) && Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Possible duplicate remoteVoiceInfo cached:{0} vs. received:{1}", remoteVoiceLink, remoteVoice);
			}
		}
		cachedRemoteVoices.Add(remoteVoice);
		if (this.RemoteVoiceAdded != null)
		{
			this.RemoteVoiceAdded(remoteVoice);
		}
		remoteVoice.RemoteVoiceRemoved += delegate
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.LogInfo("RemoteVoiceRemoved {0}", remoteVoice);
			}
			if (!cachedRemoteVoices.Remove(remoteVoice) && Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Cached remote voice not removed {0}", remoteVoice);
			}
		};
		Speaker speaker = null;
		if (SpeakerFactory != null)
		{
			speaker = SpeakerFactory(playerId, voiceId, voiceInfo.UserData);
		}
		if ((object)speaker == null)
		{
			speaker = SimpleSpeakerFactory(playerId, voiceId, voiceInfo.UserData);
		}
		else if (speaker.IsLinked)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Overriding speaker link, old:{0} new:{1}", speaker.RemoteVoiceLink, remoteVoice);
			}
			speaker.OnRemoteVoiceRemove();
		}
		LinkSpeaker(speaker, remoteVoice);
	}

	protected virtual void OnVoiceStateChanged(ClientState fromState, ClientState toState)
	{
		if (Logger.IsDebugEnabled)
		{
			Logger.LogDebug("OnVoiceStateChanged from {0} to {1}", fromState, toState);
		}
		if (fromState == ClientState.Joined)
		{
			StopInitializedRecorders();
			ClearRemoteVoicesCache();
		}
		switch (toState)
		{
		case ClientState.ConnectedToMasterServer:
			if (Client.RegionHandler != null)
			{
				if (Settings != null)
				{
					Settings.BestRegionSummaryFromStorage = Client.RegionHandler.SummaryToCache;
				}
				BestRegionSummaryInPreferences = Client.RegionHandler.SummaryToCache;
			}
			break;
		case ClientState.Joined:
			StartInitializedRecorders();
			break;
		}
	}

	protected void CalcStatistics()
	{
		float time = Time.time;
		int num = VoiceClient.FramesReceived - referenceFramesReceived;
		int num2 = VoiceClient.FramesLost - referenceFramesLost;
		float num3 = time - statsReferenceTime;
		if (num3 > 0f)
		{
			if (num + num2 > 0)
			{
				FramesReceivedPerSecond = (float)num / num3;
				FramesLostPerSecond = (float)num2 / num3;
				FramesLostPercent = 100f * (float)num2 / (float)(num + num2);
			}
			else
			{
				FramesReceivedPerSecond = 0f;
				FramesLostPerSecond = 0f;
				FramesLostPercent = 0f;
			}
		}
		referenceFramesReceived = VoiceClient.FramesReceived;
		referenceFramesLost = VoiceClient.FramesLost;
		statsReferenceTime = time;
	}

	private void CleanUp()
	{
		bool flag = client != null;
		if (Logger.IsDebugEnabled)
		{
			Logger.LogDebug("Client exists? {0}, already cleaned up? {1}", flag, cleanedUp);
		}
		if (cleanedUp)
		{
			return;
		}
		StopFallbackSendAckThread();
		if (flag)
		{
			client.StateChanged -= OnVoiceStateChanged;
			client.OpResponseReceived -= OnOperationResponseReceived;
			client.Disconnect();
			if (client.LoadBalancingPeer != null)
			{
				client.LoadBalancingPeer.Disconnect();
				client.LoadBalancingPeer.StopThread();
			}
			client.Dispose();
		}
		cleanedUp = true;
	}

	protected void LinkSpeaker(Speaker speaker, RemoteVoiceLink remoteVoice)
	{
		if (speaker != null)
		{
			if (!speaker.IgnoreGlobalLogLevel)
			{
				speaker.LogLevel = GlobalSpeakersLogLevel;
			}
			speaker.SetPlaybackDelaySettings(globalPlaybackDelaySettings);
			if (!speaker.OnRemoteVoiceInfo(remoteVoice))
			{
				return;
			}
			if (speaker.Actor == null)
			{
				if (Client.CurrentRoom == null)
				{
					if (Logger.IsErrorEnabled)
					{
						Logger.LogError("RemoteVoiceInfo event received while CurrentRoom is null");
					}
				}
				else
				{
					Player player = Client.CurrentRoom.GetPlayer(remoteVoice.PlayerId);
					if (player == null)
					{
						if (Logger.IsErrorEnabled)
						{
							Logger.LogError("RemoteVoiceInfo event received while respective actor not found in the room, {0}", remoteVoice);
						}
					}
					else
					{
						speaker.Actor = player;
					}
				}
			}
			if (Logger.IsInfoEnabled)
			{
				Logger.LogInfo("Speaker linked with remote voice {0}", remoteVoice);
			}
			linkedSpeakers.Add(speaker);
			remoteVoice.RemoteVoiceRemoved += delegate
			{
				linkedSpeakers.Remove(speaker);
			};
			if (this.SpeakerLinked != null)
			{
				this.SpeakerLinked(speaker);
			}
		}
		else if (Logger.IsWarningEnabled)
		{
			Logger.LogWarning("Speaker is null. Remote voice {0} not linked.", remoteVoice);
		}
	}

	private void ClearRemoteVoicesCache()
	{
		if (cachedRemoteVoices.Count > 0)
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.LogInfo("{0} cached remote voices info cleared", cachedRemoteVoices.Count);
			}
			cachedRemoteVoices.Clear();
		}
	}

	private void TryInitializePrimaryRecorder()
	{
		if (primaryRecorder != null)
		{
			if (!primaryRecorder.IsInitialized)
			{
				primaryRecorder.Init(this);
			}
			primaryRecorderInitialized = primaryRecorder.IsInitialized;
		}
	}

	internal void AddInitializedRecorder(Recorder rec)
	{
		initializedRecorders.Add(rec);
	}

	internal void RemoveInitializedRecorder(Recorder rec)
	{
		initializedRecorders.Remove(rec);
	}

	private void StartInitializedRecorders()
	{
		for (int i = 0; i < initializedRecorders.Count; i++)
		{
			initializedRecorders[i].CheckAndAutoStart();
		}
	}

	private void StopInitializedRecorders()
	{
		for (int i = 0; i < initializedRecorders.Count; i++)
		{
			Recorder recorder = initializedRecorders[i];
			if (recorder.IsRecording && recorder.RecordOnlyWhenJoined)
			{
				recorder.StopRecordingInternal();
			}
		}
	}

	private bool TryGetFirstVoiceStreamByUserData(object userData, out RemoteVoiceLink remoteVoiceLink)
	{
		remoteVoiceLink = null;
		if (userData == null)
		{
			return false;
		}
		if (Logger.IsWarningEnabled)
		{
			int num = 0;
			for (int i = 0; i < cachedRemoteVoices.Count; i++)
			{
				RemoteVoiceLink remoteVoiceLink2 = cachedRemoteVoices[i];
				if (!userData.Equals(remoteVoiceLink2.Info.UserData))
				{
					continue;
				}
				num++;
				if (num == 1)
				{
					remoteVoiceLink = remoteVoiceLink2;
					if (Logger.IsDebugEnabled)
					{
						Logger.LogWarning("(first) remote voice stream found by UserData:{0}", userData, remoteVoiceLink2);
					}
				}
				else
				{
					Logger.LogWarning("{0} remote voice stream found (so far) using same UserData:{0}", num, remoteVoiceLink2);
				}
			}
			return num > 0;
		}
		for (int j = 0; j < cachedRemoteVoices.Count; j++)
		{
			RemoteVoiceLink remoteVoiceLink3 = cachedRemoteVoices[j];
			if (userData.Equals(remoteVoiceLink3.Info.UserData))
			{
				remoteVoiceLink = remoteVoiceLink3;
				if (Logger.IsDebugEnabled)
				{
					Logger.LogWarning("(first) remote voice stream found by UserData:{0}", userData, remoteVoiceLink3);
				}
				return true;
			}
		}
		return false;
	}

	protected virtual void OnOperationResponseReceived(OperationResponse operationResponse)
	{
		if (Logger.IsErrorEnabled && operationResponse.ReturnCode != 0 && (operationResponse.OperationCode != 225 || operationResponse.ReturnCode == 32760))
		{
			Logger.LogError("Operation {0} response error code {1} message {2}", operationResponse.OperationCode, operationResponse.ReturnCode, operationResponse.DebugMessage);
		}
	}
}

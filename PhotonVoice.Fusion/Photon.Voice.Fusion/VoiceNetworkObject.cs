using ExitGames.Client.Photon;
using Fusion;
using Photon.Voice.Unity;
using UnityEngine;

namespace Photon.Voice.Fusion;

[NetworkBehaviourWeaved(0)]
public class VoiceNetworkObject : NetworkBehaviour, ILoggableDependent, ILoggable
{
	private VoiceConnection voiceConnection;

	[SerializeField]
	private Speaker speakerInUse;

	[SerializeField]
	private Recorder recorderInUse;

	[SerializeField]
	protected DebugLevel logLevel = DebugLevel.ERROR;

	private VoiceLogger logger;

	[SerializeField]
	[HideInInspector]
	private bool ignoreGlobalLogLevel;

	public bool AutoCreateRecorderIfNotFound;

	public bool UsePrimaryRecorder;

	public bool SetupDebugSpeaker;

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

	public bool IgnoreGlobalLogLevel
	{
		get
		{
			return ignoreGlobalLogLevel;
		}
		set
		{
			ignoreGlobalLogLevel = value;
		}
	}

	public Recorder RecorderInUse
	{
		get
		{
			return recorderInUse;
		}
		set
		{
			if (value != recorderInUse)
			{
				recorderInUse = value;
				IsRecorder = false;
			}
			if (RequiresRecorder)
			{
				SetupRecorderInUse();
			}
			else if (IsNetworkObjectReady && Logger.IsWarningEnabled)
			{
				Logger.LogWarning("No need to set Recorder as this is a remote NetworkObject.");
			}
		}
	}

	public Speaker SpeakerInUse
	{
		get
		{
			return speakerInUse;
		}
		set
		{
			if (speakerInUse != value)
			{
				speakerInUse = value;
				IsSpeaker = false;
			}
			if (RequiresSpeaker)
			{
				SetupSpeakerInUse();
			}
			else if (IsNetworkObjectReady && Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Speaker not set because this is a local NetworkObject and SetupDebugSpeaker is disabled.");
			}
		}
	}

	public bool IsSetup
	{
		get
		{
			if (IsNetworkObjectReady && (!RequiresRecorder || IsRecorder))
			{
				if (RequiresSpeaker)
				{
					return IsSpeaker;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsSpeaker { get; private set; }

	public bool IsSpeaking => SpeakerInUse.IsPlaying;

	public bool IsRecorder { get; private set; }

	public bool IsRecording
	{
		get
		{
			if (IsRecorder)
			{
				return RecorderInUse.IsCurrentlyTransmitting;
			}
			return false;
		}
	}

	public bool IsSpeakerLinked
	{
		get
		{
			if (IsSpeaker)
			{
				return SpeakerInUse.IsLinked;
			}
			return false;
		}
	}

	internal bool IsNetworkObjectReady
	{
		get
		{
			if ((bool)base.Object && (object)base.Object != null && (bool)base.Object)
			{
				return base.Object.IsValid;
			}
			return false;
		}
	}

	internal bool RequiresSpeaker
	{
		get
		{
			if (IsNetworkObjectReady && IsPlayer)
			{
				if (!SetupDebugSpeaker)
				{
					return !IsLocal;
				}
				return true;
			}
			return false;
		}
	}

	internal bool RequiresRecorder
	{
		get
		{
			if (IsNetworkObjectReady && IsPlayer)
			{
				return IsLocal;
			}
			return false;
		}
	}

	internal bool IsPlayer => base.Runner.IsPlayer;

	internal bool IsLocal
	{
		get
		{
			if (!base.Object.HasInputAuthority)
			{
				return base.Object.HasStateAuthority;
			}
			return true;
		}
	}

	internal void Setup()
	{
		if (IsSetup)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.LogDebug("VoiceNetworkObject already setup");
			}
		}
		else
		{
			SetupRecorderInUse();
			SetupSpeakerInUse();
		}
	}

	private bool SetupRecorder()
	{
		if ((object)recorderInUse == null)
		{
			if (UsePrimaryRecorder)
			{
				if ((object)voiceConnection.PrimaryRecorder != null && (bool)voiceConnection.PrimaryRecorder)
				{
					recorderInUse = voiceConnection.PrimaryRecorder;
					return SetupRecorder(recorderInUse);
				}
				if (Logger.IsErrorEnabled)
				{
					Logger.LogError("PrimaryRecorder is not set.");
				}
			}
			Recorder[] componentsInChildren = GetComponentsInChildren<Recorder>();
			if (componentsInChildren.Length != 0)
			{
				Recorder recorder = componentsInChildren[0];
				if (componentsInChildren.Length > 1 && Logger.IsWarningEnabled)
				{
					Logger.LogWarning("Multiple Recorder components found attached to the GameObject or its children.");
				}
				if ((object)recorder != null && (bool)recorder)
				{
					recorderInUse = recorder;
					return SetupRecorder(recorderInUse);
				}
			}
			if (!AutoCreateRecorderIfNotFound)
			{
				if (Logger.IsWarningEnabled)
				{
					Logger.LogWarning("No Recorder found to be setup.");
				}
				return false;
			}
			recorderInUse = base.gameObject.AddComponent<Recorder>();
		}
		return SetupRecorder(recorderInUse);
	}

	private bool SetupRecorder(Recorder recorder)
	{
		if ((object)recorder == null)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Cannot setup a null Recorder.");
			}
			return false;
		}
		if (!recorder)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Cannot setup a destroyed Recorder.");
			}
			return false;
		}
		if (!IsNetworkObjectReady)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Recorder setup cannot be done as the NetworkObject is not valid or not ready yet.");
			}
			return false;
		}
		recorder.UserData = GetUserData();
		if (!recorder.IsInitialized)
		{
			RecorderInUse.Init(voiceConnection);
		}
		if (recorder.RequiresRestart)
		{
			recorder.RestartRecording();
		}
		return recorder.IsInitialized;
	}

	private bool SetupSpeaker()
	{
		if ((object)speakerInUse == null)
		{
			Speaker[] componentsInChildren = GetComponentsInChildren<Speaker>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				speakerInUse = componentsInChildren[0];
				if (componentsInChildren.Length > 1 && Logger.IsWarningEnabled)
				{
					Logger.LogWarning("Multiple Speaker components found attached to the GameObject or its children. Using the first one we found.");
				}
			}
			if ((object)speakerInUse == null)
			{
				bool flag = false;
				if ((object)voiceConnection.SpeakerPrefab != null)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(voiceConnection.SpeakerPrefab, base.transform, worldPositionStays: false);
					componentsInChildren = gameObject.GetComponentsInChildren<Speaker>(includeInactive: true);
					if (componentsInChildren.Length != 0)
					{
						speakerInUse = componentsInChildren[0];
						if (componentsInChildren.Length > 1 && Logger.IsWarningEnabled)
						{
							Logger.LogWarning("Multiple Speaker components found attached to the GameObject (VoiceConnection.SpeakerPrefab) or its children. Using the first one we found.");
						}
					}
					if ((object)speakerInUse == null)
					{
						if (Logger.IsErrorEnabled)
						{
							Logger.LogError("SpeakerPrefab does not have a component of type Speaker in its hierarchy.");
						}
						UnityEngine.Object.Destroy(gameObject);
					}
					else
					{
						flag = true;
					}
				}
				if (!flag)
				{
					if (!voiceConnection.AutoCreateSpeakerIfNotFound)
					{
						return false;
					}
					speakerInUse = base.gameObject.AddComponent<Speaker>();
				}
			}
		}
		return SetupSpeaker(speakerInUse);
	}

	private bool SetupSpeaker(Speaker speaker)
	{
		if ((object)speakerInUse == null)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Cannot setup a null Speaker");
			}
			return false;
		}
		if (!speakerInUse)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Cannot setup a destroyed Speaker");
			}
			return false;
		}
		AudioSource component = speaker.GetComponent<AudioSource>();
		if ((object)component == null)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Unexpected (null?): no AudioSource found attached to the same GameObject as the Speaker component");
			}
			return false;
		}
		if (!component)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Unexpected (destroyed?): no AudioSource found attached to the same GameObject as the Speaker component");
			}
			return false;
		}
		if (component.mute && Logger.IsWarningEnabled)
		{
			Logger.LogWarning("audioSource.mute is true, playback may not work properly");
		}
		if (component.volume <= 0f && Logger.IsWarningEnabled)
		{
			Logger.LogWarning("audioSource.volume is zero, playback may not work properly");
		}
		if (!component.enabled && Logger.IsWarningEnabled)
		{
			Logger.LogWarning("audioSource.enabled is false, playback may not work properly");
		}
		return true;
	}

	internal void SetupRecorderInUse()
	{
		if (IsRecorder)
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.LogInfo("Recorder already setup");
			}
			return;
		}
		if (!RequiresRecorder)
		{
			if (IsNetworkObjectReady && Logger.IsInfoEnabled)
			{
				Logger.LogInfo("Recorder not needed");
			}
			return;
		}
		IsRecorder = SetupRecorder();
		if (!IsRecorder)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Recorder not setup for VoiceNetworkObject: playback may not work properly.");
			}
			return;
		}
		if (!RecorderInUse.IsRecording && !RecorderInUse.AutoStart && Logger.IsWarningEnabled)
		{
			Logger.LogWarning("VoiceNetworkObject.RecorderInUse.AutoStart is false, don't forget to start recording manually using recorder.StartRecording() or recorder.IsRecording = true.");
		}
		if (!RecorderInUse.TransmitEnabled && Logger.IsWarningEnabled)
		{
			Logger.LogWarning("VoiceNetworkObject.RecorderInUse.TransmitEnabled is false, don't forget to set it to true to enable transmission.");
		}
		if (!RecorderInUse.isActiveAndEnabled && RecorderInUse.RecordOnlyWhenEnabled && Logger.IsWarningEnabled)
		{
			Logger.LogWarning("VoiceNetworkObject.RecorderInUse may not work properly as RecordOnlyWhenEnabled is set to true and recorder is disabled or attached to an inactive GameObject.");
		}
	}

	internal void SetupSpeakerInUse()
	{
		if (IsSpeaker)
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.LogInfo("Speaker already setup");
			}
			return;
		}
		if (!RequiresSpeaker)
		{
			if (IsNetworkObjectReady && Logger.IsInfoEnabled)
			{
				Logger.LogInfo("Speaker not needed");
			}
			return;
		}
		IsSpeaker = SetupSpeaker();
		if (!IsSpeaker)
		{
			if (Logger.IsWarningEnabled)
			{
				Logger.LogWarning("Speaker not setup for VoiceNetworkObject: voice chat will not work.");
			}
		}
		else
		{
			CheckLateLinking();
		}
	}

	private object GetUserData()
	{
		return base.Object.Id;
	}

	private void CheckLateLinking()
	{
		if (voiceConnection.Client.InRoom)
		{
			if (IsSpeaker)
			{
				if (!IsSpeakerLinked)
				{
					if (voiceConnection.TryLateLinkingUsingUserData(SpeakerInUse, GetUserData()))
					{
						if (Logger.IsDebugEnabled)
						{
							Logger.LogDebug("Late linking attempt succeeded.");
						}
					}
					else if (Logger.IsDebugEnabled)
					{
						Logger.LogDebug("Late linking attempt failed.");
					}
				}
				else if (Logger.IsDebugEnabled)
				{
					Logger.LogDebug("Speaker already linked");
				}
			}
			else if (Logger.IsDebugEnabled)
			{
				Logger.LogDebug("VoiceNetworkObject does not have a Speaker and may not need late linking check");
			}
		}
		else if (Logger.IsDebugEnabled)
		{
			Logger.LogDebug("Voice client is still not in a room, skipping late linking check");
		}
	}

	public override void Spawned()
	{
		voiceConnection = base.Runner.GetComponent<VoiceConnection>();
		Setup();
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool P_0)
	{
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
	}
}

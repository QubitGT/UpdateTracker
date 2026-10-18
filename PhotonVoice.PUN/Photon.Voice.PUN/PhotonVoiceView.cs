using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;

namespace Photon.Voice.PUN;

[AddComponentMenu("Photon Voice/Photon Voice View")]
[RequireComponent(typeof(PhotonView))]
[HelpURL("https://doc.photonengine.com/en-us/voice/v2/getting-started/voice-for-pun")]
public class PhotonVoiceView : VoiceComponent
{
	private PhotonView photonView;

	[SerializeField]
	private Recorder recorderInUse;

	[SerializeField]
	private Speaker speakerInUse;

	private bool onEnableCalledOnce;

	public bool AutoCreateRecorderIfNotFound;

	public bool UsePrimaryRecorder;

	public bool SetupDebugSpeaker;

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
			else if (IsPhotonViewReady && base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("No need to set Recorder as the PhotonView does not belong to local player");
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
			else if (IsPhotonViewReady && base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Speaker not set because the PhotonView does not belong to a remote player or SetupDebugSpeaker is disabled");
			}
		}
	}

	public bool IsSetup
	{
		get
		{
			if (IsPhotonViewReady && (!RequiresRecorder || IsRecorder))
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

	public bool IsSpeaking
	{
		get
		{
			if (IsSpeaker)
			{
				return SpeakerInUse.IsPlaying;
			}
			return false;
		}
	}

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

	public bool IsPhotonViewReady
	{
		get
		{
			if ((object)photonView != null && (bool)photonView)
			{
				return photonView.ViewID > 0;
			}
			return false;
		}
	}

	internal bool RequiresSpeaker
	{
		get
		{
			if (!SetupDebugSpeaker)
			{
				if (IsPhotonViewReady)
				{
					return !photonView.IsMine;
				}
				return false;
			}
			return true;
		}
	}

	internal bool RequiresRecorder
	{
		get
		{
			if (IsPhotonViewReady)
			{
				return photonView.IsMine;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		photonView = GetComponent<PhotonView>();
		Init();
	}

	private void OnEnable()
	{
		if (onEnableCalledOnce)
		{
			Init();
		}
		else
		{
			onEnableCalledOnce = true;
		}
	}

	private void Start()
	{
		Init();
	}

	private void CheckLateLinking()
	{
		if (PhotonVoiceNetwork.Instance.Client.InRoom)
		{
			if (IsSpeaker)
			{
				if (!IsSpeakerLinked)
				{
					PhotonVoiceNetwork.Instance.CheckLateLinking(SpeakerInUse, photonView.ViewID);
				}
				else if (base.Logger.IsDebugEnabled)
				{
					base.Logger.LogDebug("Speaker already linked");
				}
			}
			else if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("PhotonVoiceView does not have a Speaker and may not need late linking check");
			}
		}
		else if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Voice client is still not in a room, skipping late linking check");
		}
	}

	internal void Setup()
	{
		if (IsSetup)
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("PhotonVoiceView already setup");
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
				if ((object)PhotonVoiceNetwork.Instance.PrimaryRecorder != null && (bool)PhotonVoiceNetwork.Instance.PrimaryRecorder)
				{
					recorderInUse = PhotonVoiceNetwork.Instance.PrimaryRecorder;
					return SetupRecorder(recorderInUse);
				}
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("PrimaryRecorder is not set.");
				}
			}
			Recorder[] componentsInChildren = GetComponentsInChildren<Recorder>();
			if (componentsInChildren.Length != 0)
			{
				Recorder recorder = componentsInChildren[0];
				if (componentsInChildren.Length > 1 && base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Multiple Recorder components found attached to the GameObject or its children.");
				}
				if ((object)recorder != null && (bool)recorder)
				{
					recorderInUse = recorder;
					return SetupRecorder(recorderInUse);
				}
			}
			if (!AutoCreateRecorderIfNotFound)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("No Recorder found to be setup.");
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
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot setup a null Recorder.");
			}
			return false;
		}
		if (!recorder)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot setup a destroyed Recorder.");
			}
			return false;
		}
		if (!IsPhotonViewReady)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Recorder setup cannot be done before assigning a valid ViewID to the PhotonView attached to the same GameObject as the PhotonVoiceView.");
			}
			return false;
		}
		recorder.UserData = photonView.ViewID;
		if (!recorder.IsInitialized)
		{
			RecorderInUse.Init(PhotonVoiceNetwork.Instance);
		}
		if (recorder.RequiresRestart)
		{
			recorder.RestartRecording();
		}
		if (recorder.IsInitialized && recorder.UserData is int)
		{
			return photonView.ViewID == (int)recorder.UserData;
		}
		return false;
	}

	private bool SetupSpeaker()
	{
		if ((object)speakerInUse == null)
		{
			Speaker[] componentsInChildren = GetComponentsInChildren<Speaker>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				speakerInUse = componentsInChildren[0];
				if (componentsInChildren.Length > 1 && base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Multiple Speaker components found attached to the GameObject or its children. Using the first one we found.");
				}
			}
			if ((object)speakerInUse == null)
			{
				bool flag = false;
				if ((object)PhotonVoiceNetwork.Instance.SpeakerPrefab != null)
				{
					GameObject gameObject = Object.Instantiate(PhotonVoiceNetwork.Instance.SpeakerPrefab, base.transform, worldPositionStays: false);
					componentsInChildren = gameObject.GetComponentsInChildren<Speaker>(includeInactive: true);
					if (componentsInChildren.Length != 0)
					{
						speakerInUse = componentsInChildren[0];
						if (componentsInChildren.Length > 1 && base.Logger.IsWarningEnabled)
						{
							base.Logger.LogWarning("Multiple Speaker components found attached to the GameObject (PhotonVoiceNetwork.SpeakerPrefab) or its children. Using the first one we found.");
						}
					}
					if ((object)speakerInUse == null)
					{
						if (base.Logger.IsErrorEnabled)
						{
							base.Logger.LogError("SpeakerPrefab does not have a component of type Speaker in its hierarchy.");
						}
						Object.Destroy(gameObject);
					}
					else
					{
						flag = true;
					}
				}
				if (!flag)
				{
					if (!PhotonVoiceNetwork.Instance.AutoCreateSpeakerIfNotFound)
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
		if ((object)speaker == null)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot setup a null Speaker");
			}
			return false;
		}
		if (!speaker)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot setup a destroyed Speaker");
			}
			return false;
		}
		AudioSource component = speaker.GetComponent<AudioSource>();
		if ((object)component == null)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Unexpected (null?): no AudioSource found attached to the same GameObject as the Speaker component");
			}
			return false;
		}
		if (!component)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Unexpected (destroyed?): no AudioSource found attached to the same GameObject as the Speaker component");
			}
			return false;
		}
		if (component.mute && base.Logger.IsWarningEnabled)
		{
			base.Logger.LogWarning("audioSource.mute is true, playback may not work properly");
		}
		if (component.volume <= 0f && base.Logger.IsWarningEnabled)
		{
			base.Logger.LogWarning("audioSource.volume is zero, playback may not work properly");
		}
		if (!component.enabled && base.Logger.IsWarningEnabled)
		{
			base.Logger.LogWarning("audioSource.enabled is false, playback may not work properly");
		}
		return true;
	}

	internal void SetupRecorderInUse()
	{
		if (IsRecorder)
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Recorder already setup");
			}
			return;
		}
		if (!RequiresRecorder)
		{
			if (IsPhotonViewReady && base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Recorder not needed");
			}
			return;
		}
		IsRecorder = SetupRecorder();
		if (!IsRecorder)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Recorder not setup for PhotonVoiceView: playback may not work properly.");
			}
			return;
		}
		if (!RecorderInUse.IsRecording && !RecorderInUse.AutoStart && base.Logger.IsWarningEnabled)
		{
			base.Logger.LogWarning("PhotonVoiceView.RecorderInUse.AutoStart is false, don't forget to start recording manually using recorder.StartRecording() or recorder.IsRecording = true.");
		}
		if (!RecorderInUse.TransmitEnabled && base.Logger.IsWarningEnabled)
		{
			base.Logger.LogWarning("PhotonVoiceView.RecorderInUse.TransmitEnabled is false, don't forget to set it to true to enable transmission.");
		}
		if (!RecorderInUse.isActiveAndEnabled && RecorderInUse.RecordOnlyWhenEnabled && base.Logger.IsWarningEnabled)
		{
			base.Logger.LogWarning("PhotonVoiceView.RecorderInUse may not work properly as RecordOnlyWhenEnabled is set to true and recorder is disabled or attached to an inactive GameObject.");
		}
	}

	internal void SetupSpeakerInUse()
	{
		if (IsSpeaker)
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Speaker already setup");
			}
			return;
		}
		if (!RequiresSpeaker)
		{
			if (IsPhotonViewReady && base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Speaker not needed");
			}
			return;
		}
		IsSpeaker = SetupSpeaker();
		if (!IsSpeaker)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Speaker not setup for PhotonVoiceView: voice chat will not work.");
			}
		}
		else
		{
			CheckLateLinking();
		}
	}

	public void Init()
	{
		if (IsPhotonViewReady)
		{
			Setup();
			CheckLateLinking();
		}
		else if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Tried to initialize PhotonVoiceView but PhotonView does not have a valid allocated ViewID yet.");
		}
	}
}

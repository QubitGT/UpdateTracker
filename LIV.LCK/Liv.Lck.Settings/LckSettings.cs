using System;
using Liv.Lck.Core;
using Liv.Lck.NativeMicrophone;
using Liv.NGFX;
using UnityEngine;

namespace Liv.Lck.Settings;

public class LckSettings : ScriptableObject
{
	[Serializable]
	public enum LimiterType
	{
		SoftClip,
		None
	}

	[Serializable]
	public enum MicPermissionAskType
	{
		OnAppStartup,
		OnTabletSpawn,
		OnMicUnmute,
		NeverAskFromLck
	}

	[Serializable]
	public enum ImageFileFormat
	{
		EXR,
		JPG,
		TGA,
		PNG
	}

	public const string SettingsPath = "Assets/Resources/LckSettings.asset";

	public const int RequiredAndroidApiLevel = 29;

	[SerializeField]
	public bool ShowSetupWizard = true;

	[SerializeField]
	[HideInInspector]
	public string DismissedUpdateVersion = "";

	[SerializeField]
	[HideInInspector]
	public string LastShownOverviewVersion = "";

	[SerializeField]
	[HideInInspector]
	public string DismissedNotificationBarVersion = "";

	[SerializeField]
	public string TrackingId = "";

	[SerializeField]
	public string GameName = "MyGame";

	[Space(10f)]
	[SerializeField]
	public string RecordingFilenamePrefix = "MyGamePrefix";

	[SerializeField]
	public string RecordingAlbumName = "MyGameAlbum";

	[SerializeField]
	public string RecordingDateSuffixFormat = "yyyy-MM-dd_HH-mm-ss";

	[Space(10f)]
	[Header("Advanced")]
	[SerializeField]
	[Tooltip("When should the user be asked for microphone access permission in Android builds.")]
	public MicPermissionAskType MicPermissionType;

	[SerializeField]
	[Tooltip("Allow LCK to modify the AndroidManifest.xml file to add Microphone permissions. Disable if you want to manually add permissions.")]
	public bool AddMicPermissionsToAndroidManifest = true;

	[SerializeField]
	[Tooltip("Allow LCK to modify the AndroidManifest.xml file to allow the LIV Control Center app (for streaming) to be launched and queried. Disable if you want to remove the permission.")]
	public bool AddControlCenterPermissionsToAndroidManifest = true;

	[SerializeField]
	[Tooltip("Allow LCK to modify the AndroidManifest.xml file to add Internet permissions. Disable if you want to manually add permissions.")]
	public bool AddInternetPermissionsToAndroidManifest = true;

	[Space(10f)]
	[Header("Logging")]
	[SerializeField]
	public LogLevel BaseLogLevel = LogLevel.Error;

	[SerializeField]
	public Liv.Lck.NativeMicrophone.LogLevel MicrophoneLogLevel = Liv.Lck.NativeMicrophone.LogLevel.Error;

	[SerializeField]
	public Liv.NGFX.LogLevel NativeLogLevel = Liv.NGFX.LogLevel.Error;

	[SerializeField]
	public LevelFilter CoreLogLevel = LevelFilter.Error;

	[SerializeField]
	[Tooltip("OpenGL messages can be useful to debug errors happening at graphics API level.")]
	public bool ShowOpenGLMessages;

	[Header("Audio")]
	[SerializeField]
	[Tooltip("Game audio may appear ahead or behind the game visuals in your game recordings. This property allows for Game Audio to be shifted forward or backwards by the provided milliseconds. Positive values will move the audio forward in time, negative backwards.")]
	public float GameAudioSyncTimeOffsetInMS = 250f;

	[SerializeField]
	[Tooltip("Enabling the audio limiter results in limiter compression applied to the recordings audio.")]
	public LimiterType AudioLimiter;

	[SerializeField]
	[Tooltip("The sample rate used by LCK if it can't get the samplerate from other sources")]
	public int FallbackSampleRate = 48000;

	[Header("Photo")]
	[SerializeField]
	[Tooltip("The format Photo images will be saved in.")]
	public ImageFileFormat ImageCaptureFileFormat = ImageFileFormat.PNG;

	[Space(10f)]
	[Header("Tablet Using Collider Settings")]
	[Tooltip("When using the 'LCK Tablet Using Collider' prefab. Trigger events will check this tag. Make sure to add this tag on your XR Rig Direct Interactors for both controllers")]
	[SerializeField]
	public string TriggerEnterTag = "Hand";

	[HideInInspector]
	public const string Version = "1.4.6";

	[HideInInspector]
	public const int Build = -1;

	private static LckSettings _instance;

	public static LckSettings Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<LckSettings>("LckSettings");
				if (_instance != null)
				{
					Debug.Log("LCK Settings loaded from Resources");
				}
				else
				{
					Debug.LogError("LCK not able to load settings. LckSettings.asset expected to exist in Resources");
				}
			}
			if (_instance == null)
			{
				_instance = ScriptableObject.CreateInstance<LckSettings>();
				Debug.LogError("LCK using default settings because LckSettings.asset not found");
			}
			return _instance;
		}
	}

	private void OnValidate()
	{
		if (!string.IsNullOrEmpty(TrackingId))
		{
			TrackingId = TrackingId.Trim();
		}
	}
}

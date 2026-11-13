using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GorillaGameModes;
using GorillaNetworking.ScheduledEvents;
using GorillaTag.Audio;
using GorillaTagScripts;
using GorillaTagScripts.VirtualStumpCustomMaps;
using KID.Model;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using PlayFab.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace GorillaNetworking;

public class GorillaComputer : MonoBehaviour, IGorillaSliceableSimple
{
	public enum ComputerState
	{
		Startup,
		Color,
		Name,
		Turn,
		Mic,
		Room,
		Queue,
		Group,
		Voice,
		AutoMute,
		Credits,
		Visuals,
		Time,
		NameWarning,
		Loading,
		Support,
		Troop,
		KID,
		Redemption,
		Language,
		Snake
	}

	private enum NameCheckResult
	{
		Success,
		Warning,
		Ban
	}

	public enum RedemptionResult
	{
		Empty,
		Invalid,
		Checking,
		AlreadyUsed,
		TooEarly,
		TooLate,
		AlreadyGranted,
		Success
	}

	[Serializable]
	public class StateOrderItem
	{
		public ComputerState State;

		[Tooltip("Case not important - ToUpper applied at runtime")]
		public string OverrideName = "";

		public LocalizedString StringReference;

		private Locale _previousLocale;

		private string _cachedTranslation = "";

		public StateOrderItem()
		{
		}

		public StateOrderItem(ComputerState state)
		{
			State = state;
		}

		public StateOrderItem(ComputerState state, string overrideName)
		{
			State = state;
			OverrideName = overrideName;
		}

		public string GetName()
		{
			if (_previousLocale == LocalizationSettings.SelectedLocale && !string.IsNullOrEmpty(_cachedTranslation))
			{
				return _cachedTranslation;
			}
			if (StringReference == null || StringReference.IsEmpty)
			{
				return GetPreLocalisedName();
			}
			_previousLocale = LocalizationSettings.SelectedLocale;
			_cachedTranslation = StringReference.GetLocalizedString()?.ToUpper();
			if (string.IsNullOrEmpty(_cachedTranslation))
			{
				if (LocalisationManager.ApplicationRunning)
				{
					Debug.LogError("[LOCALIZATION::STATE_ORDER_ITEM] Failed to get translation for selected locale [" + (_previousLocale?.LocaleName ?? "NULL") + ", for item [" + State.GetName() + "]");
				}
				_cachedTranslation = "";
			}
			return _cachedTranslation;
		}

		public string GetPreLocalisedName()
		{
			if (!string.IsNullOrEmpty(OverrideName))
			{
				return OverrideName.ToUpper();
			}
			return State.ToString().ToUpper();
		}
	}

	private enum SnakeGameState
	{
		Waiting,
		Playing,
		GameOver
	}

	private enum EKidScreenState
	{
		Ready,
		Show_OTP,
		Show_Setup_Screen
	}

	private sealed class CachedLocalisedText
	{
		private readonly string _key;

		private readonly string _fallback;

		private readonly string _context;

		private Locale _locale;

		private string _text;

		public string Value
		{
			get
			{
				Locale currentLanguage = LocalisationManager.CurrentLanguage;
				if (_locale == null || _locale != currentLanguage || string.IsNullOrEmpty(_text))
				{
					_text = GetLocalisedText(_key, _fallback, _context);
					_locale = currentLanguage;
				}
				return _text;
			}
		}

		public CachedLocalisedText(string key, string fallback, string context)
		{
			_key = key;
			_fallback = fallback;
			_context = context;
		}
	}

	private const string VERSION_MISMATCH_KEY = "VERSION_MISMATCH";

	private const string CONNECTION_ISSUE_KEY = "CONNECTION_ISSUE";

	private const string NO_CONNECTION_KEY = "NO_CONNECTION";

	private const string STARTUP_INTRO_KEY = "STARTUP_INTRO";

	private const string STARTUP_PLAYERS_ONLINE_KEY = "STARTUP_PLAYERS_ONLINE";

	private const string STARTUP_USERS_BANNED_KEY = "STARTUP_USERS_BANNED";

	private const string STARTUP_PRESS_KEY_KEY = "STARTUP_PRESS_KEY";

	private const string STARTUP_MANAGED_KEY = "STARTUP_MANAGED";

	private const string COLOR_SELECT_INTRO_KEY = "COLOR_SELECT_INTRO";

	private const string COLOR_RED_KEY = "COLOR_RED";

	private const string COLOR_GREEN_KEY = "COLOR_GREEN";

	private const string COLOR_BLUE_KEY = "COLOR_BLUE";

	private const string ROOM_INTRO_KEY = "ROOM_INTRO";

	private const string ROOM_OPTION_KEY = "ROOM_OPTION";

	private const string ROOM_TEXT_CURRENT_ROOM_KEY = "ROOM_TEXT_CURRENT_ROOM";

	private const string PLAYERS_IN_ROOM_KEY = "PLAYERS_IN_ROOM";

	private const string NOT_IN_ROOM_KEY = "NOT_IN_ROOM";

	private const string PLAYERS_ONLINE_KEY = "PLAYERS_ONLINE";

	private const string ROOM_TO_JOIN_KEY = "ROOM_TO_JOIN";

	private const string ROOM_FULL_KEY = "ROOM_FULL";

	private const string ROOM_JOIN_NOT_ALLOWED_KEY = "ROOM_JOIN_NOT_ALLOWED";

	private const string NAME_SCREEN_KEY = "NAME_SCREEN";

	private const string CURRENT_NAME_KEY = "CURRENT_NAME";

	private const string NEW_NAME_KEY = "NEW_NAME";

	private const string NAME_CHECK_RATE_LIMITED_KEY = "NAME_CHECK_RATE_LIMITED";

	private const string TURN_SCREEN_KEY = "TURN_SCREEN";

	private const string TURN_SCREEN_TURNING_SPEED_KEY = "TURN_SCREEN_TURNING_SPEED";

	private const string TURN_SCREEN_TURN_TYPE_KEY = "TURN_SCREEN_TURN_TYPE";

	private const string TURN_SCREEN_TURN_SPEED_KEY = "TURN_SCREEN_TURN_SPEED";

	private const string TURN_TYPE_SNAP_TURN_KEY = "TURN_TYPE_SNAP_TURN";

	private const string TURN_TYPE_SMOOTH_TURN_KEY = "TURN_TYPE_SMOOTH_TURN";

	private const string TURN_TYPE_NO_TURN_KEY = "TURN_TYPE_NO_TURN";

	private const string QUEUE_SCREEN_KEY = "QUEUE_SCREEN";

	private const string BEAT_OBSTACLE_COURSE_KEY = "BEAT_OBSTACLE_COURSE";

	private const string COMPETITIVE_DESC_KEY = "COMPETITIVE_DESC";

	private const string QUEUE_SCREEN_ALL_QUEUES_KEY = "QUEUE_SCREEN_ALL_QUEUES";

	private const string QUEUE_SCREEN_DEFAULT_QUEUES_KEY = "QUEUE_SCREEN_DEFAULT_QUEUES";

	private const string CURRENT_QUEUE_KEY = "CURRENT_QUEUE";

	private const string DEFAULT_QUEUE_KEY = "DEFAULT_QUEUE";

	private const string MINIGAMES_QUEUE_KEY = "MINIGAMES_QUEUE";

	private const string COMPETITIVE_QUEUE_KEY = "COMPETITIVE_QUEUE";

	private const string MIC_SCREEN_OPTIONS_KEY = "MIC_SCREEN_OPTIONS";

	private const string MIC_SCREEN_CURRENT_KEY = "MIC_SCREEN_CURRENT";

	private const string MIC_SCREEN_PUSH_TO_MUTE_TOOLTIP_KEY = "MIC_SCREEN_PUSH_TO_MUTE_TOOLTIP";

	private const string MIC_SCREEN_MIC_DISABLED_KEY = "MIC_SCREEN_MIC_DISABLED";

	private const string MIC_SCREEN_NO_MIC_KEY = "MIC_SCREEN_NO_MIC";

	private const string MIC_SCREEN_NO_PERMISSIONS_KEY = "MIC_SCREEN_NO_PERMISSIONS";

	private const string MIC_SCREEN_PUSH_TO_TALK_TOOLTIP_KEY = "MIC_SCREEN_PUSH_TO_TALK_TOOLTIP";

	private const string MIC_SCREEN_INPUT_TEST_LABEL_KEY = "MIC_SCREEN_INPUT_TEST_LABEL";

	private const string MIC_SCREEN_INPUT_TEST_NO_MIC_KEY = "MIC_SCREEN_INPUT_TEST_NO_MIC";

	private const string PUSH_TO_TALK_MIC_KEY = "PUSH_TO_TALK_MIC";

	private const string PUSH_TO_MUTE_MIC_KEY = "PUSH_TO_MUTE_MIC";

	private const string OPEN_MIC_KEY = "OPEN_MIC";

	private const string AUTOMOD_SCREEN_INTRO_KEY = "AUTOMOD_SCREEN_INTRO";

	private const string AUTOMOD_SCREEN_OPTIONS_KEY = "AUTOMOD_SCREEN_OPTIONS";

	private const string AUTOMOD_SCREEN_CURRENT_KEY = "AUTOMOD_SCREEN_CURRENT";

	private const string AUTOMOD_AGGRESSIVE_KEY = "AUTOMOD_AGGRESSIVE";

	private const string AUTOMOD_MODERATE_KEY = "AUTOMOD_MODERATE";

	private const string AUTOMOD_OFF_KEY = "AUTOMOD_OFF";

	private const string TRUE_KEY = "TRUE";

	private const string FALSE_KEY = "FALSE";

	private const string VOICE_CHAT_SCREEN_INTRO_KEY = "VOICE_CHAT_SCREEN_INTRO";

	private const string VOICE_CHAT_SCREEN_OPTIONS_KEY = "VOICE_CHAT_SCREEN_OPTIONS";

	private const string VOICE_CHAT_SCREEN_CURRENT_KEY = "VOICE_CHAT_SCREEN_CURRENT";

	private const string VOICE_OPTION_HUMAN_KEY = "VOICE_OPTION_HUMAN";

	private const string VOICE_OPTION_MONKE_KEY = "VOICE_OPTION_MONKE";

	private const string VOICE_OPTION_OFF_KEY = "VOICE_OPTION_OFF";

	private const string VISUALS_SCREEN_INTRO_KEY = "VISUALS_SCREEN_INTRO";

	private const string VISUALS_SCREEN_OPTIONS_KEY = "VISUALS_SCREEN_OPTIONS";

	private const string VISUALS_SCREEN_CURRENT_KEY = "VISUALS_SCREEN_CURRENT";

	private const string VISUALS_SCREEN_VOLUME_KEY = "VISUALS_SCREEN_VOLUME";

	private const string TIME_SCREEN_KEY = "TIME_SCREEN";

	private const string PLATFORM_STEAM_KEY = "PLATFORM_STEAM";

	private const string PLATFORM_QUEST_KEY = "PLATFORM_QUEST";

	private const string PLATFORM_PSVR_KEY = "PLATFORM_PSVR";

	private const string PLATFORM_PICO_KEY = "PLATFORM_PICO";

	private const string PLATFORM_OCULUS_PC_KEY = "PLATFORM_OCULUS_PC";

	private const string SUPPORT_SCREEN_INTRO_KEY = "SUPPORT_SCREEN_INTRO";

	private const string SUPPORT_SCREEN_DETAILS_PLAYER_ID_KEY = "SUPPORT_SCREEN_DETAILS_PLAYERID";

	private const string SUPPORT_SCREEN_DETAILS_VERSION_KEY = "SUPPORT_SCREEN_DETAILS_VERSION";

	private const string SUPPORT_SCREEN_DETAILS_PLATFORM_KEY = "SUPPORT_SCREEN_DETAILS_PLATFORM";

	private const string SUPPORT_SCREEN_DETAILS_BUILD_DATE_KEY = "SUPPORT_SCREEN_DETAILS_BUILD_DATE";

	private const string SUPPORT_SCREEN_DETAILS_MOTHERSHIP_SESSION_ID_KEY = "SUPPORT_SCREEN_DETAILS_MOTHERSHIP_SESSION_ID";

	private const string SUPPORT_SCREEN_INITIAL_KEY = "SUPPORT_SCREEN_INITIAL";

	private const string SUPPORT_SCREEN_INITIAL_WARNING_KEY = "SUPPORT_SCREEN_INITIAL_WARNING";

	private const string OCULUS_BUILD_CODE_KEY = "OCULUS_BUILD_CODE";

	private const string LOADING_SCREEN_KEY = "LOADING_SCREEN";

	private const string WARNING_SCREEN_KEY = "WARNING_SCREEN";

	private const string WARNING_SCREEN_CONFIRMATION_KEY = "WARNING_SCREEN_CONFIRMATION";

	private const string WARNING_SCREEN_TYPE_YES_KEY = "WARNING_SCREEN_TYPE_YES";

	private const string COMPUTER_KEYBOARD_DELETE_KEY = "COMPUTER_KEYBOARD_DELETE";

	private const string COMPUTER_KEYBOARD_ENTER_KEY = "COMPUTER_KEYBOARD_ENTER";

	private const string COMPUTER_KEYBOARD_OPTION1_KEY = "COMPUTER_KEYBOARD_OPTION1";

	private const string COMPUTER_KEYBOARD_OPTION2_KEY = "COMPUTER_KEYBOARD_OPTION2";

	private const string COMPUTER_KEYBOARD_OPTION3_KEY = "COMPUTER_KEYBOARD_OPTION3";

	private const string WARNING_SCREEN_YES_INPUT_KEY = "WARNING_SCREEN_YES_INPUT";

	private const string GROUP_SCREEN_ENTER_PARTY_KEY = "GROUP_SCREEN_ENTER_PARTY";

	private const string GROUP_SCREEN_ENTER_NOPARTY_KEY = "GROUP_SCREEN_ENTER_NOPARTY";

	private const string GROUP_SCREEN_CANNOT_JOIN_KEY = "GROUP_SCREEN_CANNOT_JOIN";

	private const string GROUP_SCREEN_ACTIVE_ZONES_KEY = "GROUP_SCREEN_ACTIVE_ZONES";

	private const string GROUP_SCREEN_DESTINATIONS_KEY = "GROUP_SCREEN_DESTINATIONS";

	private const string NAME_SCREEN_TOGGLE_NAMETAGS_KEY = "NAME_SCREEN_TOGGLE_NAMETAGS";

	private const string NAME_SCREEN_KID_PROHIBITED_VERB_KEY = "NAME_SCREEN_KID_PROHIBITED_VERB";

	private const string NAME_SCREEN_DISABLED_KEY = "NAME_SCREEN_DISABLED";

	private const string ON_KEY = "ON_KEY";

	private const string OFF_KEY = "OFF_KEY";

	private const string KID_PROHIBITED_MESSAGE_KEY = "KID_PROHIBITED_MESSAGE";

	private const string KID_PERMISSION_NEEDED_KEY = "KID_PERMISSION_NEEDED";

	private const string KID_WAITING_PERMISSION_KEY = "KID_WAITING_PERMISSION";

	private const string KID_REFRESH_PERMISSIONS_KEY = "KID_REFRESH_PERMISSIONS";

	private const string KID_CHECK_AGAIN_COOLDOWN_KEY = "KID_CHECK_AGAIN_COOLDOWN";

	private const string STARTUP_TROOP_TEXT_KEY = "STARTUP_TROOP_TEXT";

	private const string ROOM_GROUP_TRAVEL_KEY = "ROOM_GROUP_TRAVEL";

	private const string ROOM_PARTY_WARNING_KEY = "ROOM_PARTY_WARNING";

	private const string ROOM_GAME_LABEL_KEY = "ROOM_GAME_LABEL";

	private const string ROOM_SCREEN_KID_PROHIBITED_VERB_KEY = "ROOM_SCREEN_KID_PROHIBITED_VERB";

	private const string ROOM_SCREEN_DISABLED_KEY = "ROOM_SCREEN_DISABLED";

	private const string REDEMPTION_INTRO_KEY = "REDEMPTION_INTRO";

	private const string REDEMPTION_CODE_LABEL_KEY = "REDEMPTION_CODE_LABEL";

	private const string REDEMPTION_CODE_INVALID_KEY = "REDEMPTION_CODE_INVALID";

	private const string REDEMPTION_CODE_VALIDATING_KEY = "REDEMPTION_CODE_VALIDATING";

	private const string REDEMPTION_CODE_ALREADY_USED_KEY = "REDEMPTION_CODE_ALREADY_USED";

	private const string REDEMPTION_CODE_TOO_EARLY_KEY = "REDEMPTION_CODE_TOO_EARLY";

	private const string REDEMPTION_CODE_TOO_LATE_KEY = "REDEMPTION_CODE_TOO_LATE";

	private const string REDEMPTION_CODE_ALREADY_GRANTED_KEY = "REDEMPTION_CODE_ALREADY_GRANTED";

	private const string REDEMPTION_CODE_SUCCESS_KEY = "REDEMPTION_CODE_SUCCESS";

	private const string LIMITED_ONLINE_FUNC_KEY = "LIMITED_ONLINE_FUNC";

	private const string CURRENT_MODE_KEY = "CURRENT_MODE";

	private const string SUPPORT_META_ACCOUNT_TYPE_KEY = "SUPPORT_META_ACCOUNT_TYPE";

	private const string SUPPORT_FINAL_QUEST_ONE_KEY = "SUPPORT_FINAL_QUEST_ONE";

	private const string SUPPORT_KID_ACCOUNT_TYPE_KEY = "SUPPORT_KID_ACCOUNT_TYPE";

	private const string VOICE_SCREEN_KID_PROHIBITED_VERB_KEY = "VOICE_SCREEN_KID_PROHIBITED_VERB";

	private const string VOICE_SCREEN_DISABLED_KEY = "VOICE_SCREEN_DISABLED";

	private const string MIC_SCREEN_GUARDIAN_FEATURE_DESC_KEY = "MIC_SCREEN_GUARDIAN_FEATURE_DESC";

	private const string VOICE_SCREEN_KID_CURRENT_VOICE_KEY = "VOICE_SCREEN_KID_CURRENT_VOICE";

	private const string TROOP_SCREEN_INTRO_KEY = "TROOP_SCREEN_INTRO";

	private const string TROOP_SCREEN_INSTRUCTIONS_KEY = "TROOP_SCREEN_INSTRUCTIONS";

	private const string TROOP_SCREEN_CURRENT_TROOP_KEY = "TROOP_SCREEN_CURRENT_TROOP";

	private const string TROOP_SCREEN_IN_QUEUE_KEY = "TROOP_SCREEN_IN_QUEUE";

	private const string TROOP_SCREEN_PLAYERS_IN_TROOP_KEY = "TROOP_SCREEN_PLAYERS_IN_TROOP";

	private const string TROOP_SCREEN_DEFAULT_QUEUE_KEY = "TROOP_SCREEN_DEFAULT_QUEUE";

	private const string TROOP_SCREEN_CURRENT_QUEUE_KEY = "TROOP_SCREEN_CURRENT_QUEUE";

	private const string TROOP_SCREEN_TROOP_QUEUE_KEY = "TROOP_SCREEN_TROOP_QUEUE";

	private const string TROOP_SCREEN_LEAVE_KEY = "TROOP_SCREEN_LEAVE";

	private const string TROOP_SCREEN_NOT_IN_TROOP_KEY = "TROOP_SCREEN_NOT_IN_TROOP";

	private const string TROOP_SCREEN_JOIN_TROOP_KEY = "TROOP_SCREEN_JOIN_TROOP";

	private const string TROOP_SCREEN_KID_PROHIBITED_VERB_KEY = "TROOP_SCREEN_KID_PROHIBITED_VERB";

	private const string TROOP_SCREEN_DISABLED_KEY = "TROOP_SCREEN_DISABLED";

	private const string TROOP_SCREEN_KID_DESC_KEY = "TROOP_SCREEN_KID_DESC";

	private const bool HIDE_SCREENS = false;

	public const string NAMETAG_PLAYER_PREF_KEY = "nameTagsOn";

	[OnEnterPlay_SetNull]
	public static volatile GorillaComputer instance;

	[OnEnterPlay_Set(false)]
	public static bool hasInstance = false;

	[OnEnterPlay_SetNull]
	private static Action<bool> onNametagSettingChangedAction;

	private bool tryGetTimeAgain;

	public Material unpressedMaterial;

	public Material pressedMaterial;

	public string offlineTextInitialString;

	public GorillaText screenText;

	public GorillaText functionSelectText;

	[Tooltip("Monospaced font the terminals use for the start and failure screens, so their border and face line up")]
	public TMP_FontAsset monospaceScreenFont;

	private readonly CachedLocalisedText _versionMismatchText = new CachedLocalisedText("VERSION_MISMATCH", "PLEASE UPDATE TO THE LATEST VERSION OF GORILLA TAG. YOU'RE ON AN OLD VERSION. FEEL FREE TO RUN AROUND, BUT YOU WON'T BE ABLE TO PLAY WITH ANYONE ELSE.", "Version Mismatch");

	private readonly CachedLocalisedText _unableToConnectText = new CachedLocalisedText("CONNECTION_ISSUE", "UNABLE TO CONNECT TO THE INTERNET. PLEASE CHECK YOUR CONNECTION AND RESTART THE GAME.", "Connection Issue");

	public Material wrongVersionMaterial;

	public MeshRenderer computerScreenRenderer;

	public long startupMillis;

	public DateTime startupTime;

	public GameModeType lastPressedGameModeType;

	public string lastPressedGameMode;

	public WatchableStringSO currentGameMode;

	public WatchableStringSO currentGameModeText;

	public int includeUpdatedServerSynchTest;

	public PhotonNetworkController networkController;

	public float updateCooldown = 1f;

	private float defaultUpdateCooldown;

	private float micUpdateCooldown = 0.01f;

	public float lastUpdateTime;

	public bool isConnectedToMaster;

	public bool internetFailure;

	public bool limitOnlineScreens;

	[Header("State vars")]
	public bool initialized;

	public List<StateOrderItem> OrderList = new List<StateOrderItem>
	{
		new StateOrderItem(ComputerState.Room),
		new StateOrderItem(ComputerState.Name),
		new StateOrderItem(ComputerState.Language, "Lang"),
		new StateOrderItem(ComputerState.Turn),
		new StateOrderItem(ComputerState.Mic),
		new StateOrderItem(ComputerState.Queue),
		new StateOrderItem(ComputerState.Troop),
		new StateOrderItem(ComputerState.Group),
		new StateOrderItem(ComputerState.Voice),
		new StateOrderItem(ComputerState.AutoMute, "Automod"),
		new StateOrderItem(ComputerState.Visuals, "Items"),
		new StateOrderItem(ComputerState.Credits),
		new StateOrderItem(ComputerState.Support)
	};

	public string Pointer = "<-";

	public int highestCharacterCount;

	public List<string> FunctionNames = new List<string>();

	public int FunctionsCount;

	[Tooltip("Pages shown at once in the terminals' function (sidebar) text. When set, the pages move down a line and the list scrolls, with the scroll indicators on their own lines above and below the pages (blank when the first/last page is visible), so the text needs room for this + 2 lines. 0 shows every page with no indicator lines.")]
	public int MaxVisibleFunctions;

	[Tooltip("Moving up past the first page goes to the last page and down past the last goes to the first. Off stops at the ends.")]
	public bool LoopFunctionSelection = true;

	public string ScrollUpIndicator = "^";

	public string ScrollDownIndicator = "v";

	private int _functionScrollOffset;

	[Header("Room vars")]
	public string roomToJoin;

	public bool roomFull;

	public bool roomNotAllowed;

	[Header("Mic vars")]
	public string pttType;

	private GorillaSpeakerLoudness speakerLoudness;

	private float micInputTestTimer;

	private float lastMicInputTestTime;

	public float micInputTestTimerThreshold = 10f;

	private const float MicAvailabilityCheckInterval = 1f;

	private float lastMicAvailabilityCheckTime = float.MinValue;

	private bool micPermissionGranted = true;

	private bool micDeviceFound = true;

	[Header("Automute vars")]
	public string autoMuteType;

	[Header("Queue vars")]
	public string currentQueue;

	public bool allowedInCompetitive;

	[Header("Group Vars")]
	public string groupMapJoin;

	public int groupMapJoinIndex;

	public GorillaFriendCollider friendJoinCollider;

	[Header("Troop vars")]
	public string troopName;

	public bool troopQueueActive;

	public string troopToJoin;

	private bool rememberTroopQueueState;

	[Header("Join Triggers")]
	public Dictionary<string, GorillaNetworkJoinTrigger> primaryTriggersByZone = new Dictionary<string, GorillaNetworkJoinTrigger>();

	public string voiceChatOn;

	public string buildDate;

	public string buildCode;

	[Header("Cosmetics")]
	public bool disableParticles;

	public float instrumentVolume;

	[Header("Subs")]
	public bool iobtMode;

	public bool isSubscribed;

	[Header("Credits")]
	public CreditsView creditsView;

	[Header("Handedness")]
	public bool leftHanded;

	[Header("Name State vars")]
	public string savedName;

	public string currentName;

	public TextAsset exactOneWeekFile;

	public TextAsset anywhereOneWeekFile;

	public TextAsset anywhereTwoWeekFile;

	private List<ComputerState> _filteredStates = new List<ComputerState>();

	private List<StateOrderItem> _activeOrderList = new List<StateOrderItem>();

	private Stack<ComputerState> stateStack = new Stack<ComputerState>();

	private ComputerState currentComputerState;

	private ComputerState previousComputerState;

	private int currentStateIndex;

	private int usersBanned;

	private float redValue;

	private float blueValue;

	private float greenValue;

	private int colorCursorLine;

	private string warningConfirmationInputString = string.Empty;

	private bool displaySupport;

	private string[] exactOneWeek;

	private string[] anywhereOneWeek;

	private string[] anywhereTwoWeek;

	private bool nameCheckRateLimited;

	private RedemptionResult redemptionResult;

	private string redemptionCode = string.Empty;

	private bool playerInVirtualStump;

	private string virtualStumpRoomPrepend = "";

	private string virtualStumpRoomModePrefix = "C";

	private WaitForSeconds waitLoadingDotStep = new WaitForSeconds(0.5f);

	private Coroutine LoadingRoutine;

	private List<string> topTroops = new List<string>();

	private bool hasRequestedInitialTroopPopulation;

	private int currentTroopPopulation = -1;

	private List<string> topVstumpMaps = new List<string>();

	private float lastCheckedWifi;

	private float checkIfDisconnectedSeconds = 10f;

	private float checkIfConnectedSeconds = 1f;

	private bool didInitializeGameMode;

	private static int sessionCount = -1;

	private const bool k_debug_shouldResetSessionCount = false;

	private const bool k_debug_shouldResetGameMode = false;

	private const string k_sessionCountKey = "sessionCount";

	internal const GameModeType k_defaultGameMode = GameModeType.SuperInfect;

	internal const GameModeType k_noobGameMode = GameModeType.Infection;

	private float troopPopulationCheckCooldown = 3f;

	private float nextPopulationCheckTime;

	private bool functionListTextDirty = true;

	private int functionListTextIndex = -1;

	private int functionListTextMaxVisible = -1;

	private string generalFailureText;

	public Action OnServerTimeUpdated;

	private static readonly Color OptionTextHighlightColor = Color.yellow;

	private static readonly bool s_optionKeyColorsEnabled = false;

	private static readonly Color Option1Color = Color.green;

	private static readonly Color Option2Color = Color.cyan;

	private static readonly Color Option3Color = Color.yellow;

	private static readonly Color Option4Color = Color.magenta;

	private static readonly Color TurnSnapColor = Option1Color;

	private static readonly Color TurnSmoothColor = Option2Color;

	private static readonly Color TurnNoTurnColor = Option3Color;

	private static readonly Color MicAllChatColor = Option1Color;

	private static readonly Color MicPushToTalkColor = Option2Color;

	private static readonly Color MicPushToMuteColor = Option3Color;

	private static readonly Color VoiceHumanColor = Option1Color;

	private static readonly Color VoiceMonkeColor = Option2Color;

	private static readonly Color VoiceMuteAllColor = Option4Color;

	private static readonly Color AutomodAggressiveColor = Option1Color;

	private static readonly Color AutomodModerateColor = Option2Color;

	private static readonly Color AutomodOffColor = Option3Color;

	private static readonly Color RoomDisconnectColor = Option1Color;

	private static readonly Color NameNametagsColor = Option1Color;

	private static readonly Color QueueDefaultColor = Option1Color;

	private static readonly Color QueueMinigamesColor = Option2Color;

	private static readonly Color QueueCompetitiveColor = Option3Color;

	private static readonly Color ItemsParticlesOnColor = Option1Color;

	private static readonly Color ItemsParticlesOffColor = Option2Color;

	private static readonly Color ItemsIobtColor = Option3Color;

	private static readonly Color ItemsGoldNameTagColor = Option4Color;

	private static readonly Color TroopQueueColor = Option1Color;

	private static readonly Color TroopDefaultQueueColor = Option2Color;

	private static readonly Color TroopLeaveColor = Option4Color;

	private static readonly Dictionary<ComputerState, Dictionary<GorillaKeyboardBindings, Color>> s_pageKeyLabelColors = new Dictionary<ComputerState, Dictionary<GorillaKeyboardBindings, Color>>
	{
		{
			ComputerState.Turn,
			new Dictionary<GorillaKeyboardBindings, Color>
			{
				{
					GorillaKeyboardBindings.option1,
					TurnSnapColor
				},
				{
					GorillaKeyboardBindings.option2,
					TurnSmoothColor
				},
				{
					GorillaKeyboardBindings.option3,
					TurnNoTurnColor
				}
			}
		},
		{
			ComputerState.Mic,
			new Dictionary<GorillaKeyboardBindings, Color>
			{
				{
					GorillaKeyboardBindings.option1,
					MicAllChatColor
				},
				{
					GorillaKeyboardBindings.option2,
					MicPushToTalkColor
				},
				{
					GorillaKeyboardBindings.option3,
					MicPushToMuteColor
				}
			}
		},
		{
			ComputerState.Voice,
			new Dictionary<GorillaKeyboardBindings, Color>
			{
				{
					GorillaKeyboardBindings.option1,
					VoiceHumanColor
				},
				{
					GorillaKeyboardBindings.option2,
					VoiceMonkeColor
				},
				{
					GorillaKeyboardBindings.option4,
					VoiceMuteAllColor
				}
			}
		},
		{
			ComputerState.AutoMute,
			new Dictionary<GorillaKeyboardBindings, Color>
			{
				{
					GorillaKeyboardBindings.option1,
					AutomodAggressiveColor
				},
				{
					GorillaKeyboardBindings.option2,
					AutomodModerateColor
				},
				{
					GorillaKeyboardBindings.option3,
					AutomodOffColor
				}
			}
		},
		{
			ComputerState.Room,
			new Dictionary<GorillaKeyboardBindings, Color> { 
			{
				GorillaKeyboardBindings.option1,
				RoomDisconnectColor
			} }
		},
		{
			ComputerState.Name,
			new Dictionary<GorillaKeyboardBindings, Color> { 
			{
				GorillaKeyboardBindings.option1,
				NameNametagsColor
			} }
		},
		{
			ComputerState.Queue,
			new Dictionary<GorillaKeyboardBindings, Color>
			{
				{
					GorillaKeyboardBindings.option1,
					QueueDefaultColor
				},
				{
					GorillaKeyboardBindings.option2,
					QueueMinigamesColor
				},
				{
					GorillaKeyboardBindings.option3,
					QueueCompetitiveColor
				}
			}
		},
		{
			ComputerState.Visuals,
			new Dictionary<GorillaKeyboardBindings, Color>
			{
				{
					GorillaKeyboardBindings.option1,
					ItemsParticlesOnColor
				},
				{
					GorillaKeyboardBindings.option2,
					ItemsParticlesOffColor
				},
				{
					GorillaKeyboardBindings.option3,
					ItemsIobtColor
				},
				{
					GorillaKeyboardBindings.option4,
					ItemsGoldNameTagColor
				}
			}
		},
		{
			ComputerState.Troop,
			new Dictionary<GorillaKeyboardBindings, Color>
			{
				{
					GorillaKeyboardBindings.option1,
					TroopQueueColor
				},
				{
					GorillaKeyboardBindings.option2,
					TroopDefaultQueueColor
				},
				{
					GorillaKeyboardBindings.option4,
					TroopLeaveColor
				}
			}
		}
	};

	[OnEnterPlay_SetNull]
	private static Action onKeyLabelColorsChanged;

	private static bool s_optionColorsEnabled = true;

	private ComputerState? keyLabelColorsState;

	private bool keyLabelColorsFailed;

	private ulong keyLabelColorsActiveKeys;

	private static readonly Regex s_sentenceEnd = new Regex("(?<=[.!?。！？])\\s*", RegexOptions.Compiled);

	private static readonly Regex s_optionNumber = new Regex("((?:(?![\u3040-ゟ一-鿿])\\p{L})+)[\\s\u00a0]?(\\d+)", RegexOptions.Compiled);

	private static readonly string[] s_numberKeysPhrases = new string[6] { "NUMBER KEYS", "ZIFFERNTASTEN", "NUMMERNTASTEN", "TECLAS NUMÉRICAS", "TOUCHES NUMÉRIQUES", "数字キー" };

	private static readonly string[] s_enterKeyPhrases = new string[4] { "ENTER", "EINGABE", "ENTRAR", "ENTRÉE" };

	private static readonly string[] s_volumeRangePhrases = new string[3] { "1-10", "1 BIS 10", "1～10" };

	private static readonly string[] s_calibrateKeyPhrase = new string[1] { "Q" };

	private static readonly string[] s_resetKeyPhrase = new string[1] { "DELETE" };

	private static readonly Regex s_numberedEntry = new Regex("(?<![\\w])(\\d+)\\s?[:)]", RegexOptions.Compiled);

	private static readonly Dictionary<string, string> s_coloredSentencesCache = new Dictionary<string, string>();

	private const bool AllowRoomInRankedOnBeta = false;

	private static readonly GorillaKeyboardBindings[] SnakeSecretCode = new GorillaKeyboardBindings[3]
	{
		GorillaKeyboardBindings.option4,
		GorillaKeyboardBindings.option4,
		GorillaKeyboardBindings.option4
	};

	private const char SnakeBorderChar = '#';

	private const char SnakeBodyChar = '■';

	private const char SnakePelletChar = 'O';

	private const string SnakePelletString = "O";

	private static readonly Color SnakePelletColor = Color.yellow;

	private const string SnakeBestScorePref = "snakeBestScore";

	private const int SnakeColumns = 42;

	[Header("Snake")]
	[Tooltip("Seconds per snake step at the start of a game")]
	public float snakeStartStepTime = 0.35f;

	[Tooltip("Each pellet eaten multiplies the step time by this, so the snake speeds up")]
	public float snakeStepTimeMultiplier = 0.93f;

	[Tooltip("Fastest the snake gets, in seconds per step")]
	public float snakeMinStepTime = 0.07f;

	[Tooltip("Line height of the Snake screen, as a percent of the font's normal line height. Lower packs the rows closer together, so the snake's cells look more square")]
	[Range(10f, 100f)]
	public int snakeLineHeightPercent = 70;

	[Tooltip("The computer screen text's Line Spacing (TextMeshPro), which is added to every line on top of the line height. Used to work out how tall the Snake rows are, so vertical moves match horizontal ones")]
	public float snakeScreenLineSpacing = 39.5f;

	private int snakeSecretCodeProgress;

	private SnakeGameState snakeGameState;

	private readonly List<Vector2Int> snakeBody = new List<Vector2Int>();

	private Vector2Int snakePellet;

	private Vector2Int snakeDirection;

	private Vector2Int snakeLastStepDirection;

	private int snakeScore;

	private int snakeBestScore = -1;

	private int snakeLastScore;

	private bool snakeNewBestScore;

	[Header("Snake Sounds")]
	[Tooltip("Snake opens (the secret code is entered)")]
	public AudioClip snakeOpenClip;

	[Tooltip("The snake starts moving")]
	public AudioClip snakeStartClip;

	[Tooltip("The snake eats a pellet")]
	public AudioClip snakeEatClip;

	[Tooltip("The snake hits a wall or itself")]
	public AudioClip snakeCrashClip;

	[Tooltip("Game over, played after the crash sound")]
	public AudioClip snakeGameOverClip;

	[Tooltip("Game over with a new best score, played after the crash sound instead of the game over sound")]
	public AudioClip snakeNewBestClip;

	[Range(0f, 1f)]
	public float snakeSoundVolume = 0.5f;

	private AudioSource snakeAudioSource;

	private Coroutine snakeGameOverSoundRoutine;

	private const string SnakeNewBestText = "NEW HIGH SCORE";

	[Tooltip("Seconds NEW HIGH SCORE shows for, then hides for, while it blinks on the game over screen")]
	public float snakeNewBestBlinkTime = 0.5f;

	private bool shownSnakeNewBest;

	private int snakeHighlightRow = -1;

	private int snakeHighlightStart;

	private int snakeHighlightLength;

	private float snakeStepTime;

	private float snakeLastStepTime;

	private readonly StringBuilder snakeScreen = new StringBuilder();

	private char[,] snakeGrid;

	private int snakeRows = 13;

	private float snakeVerticalStepScale = 1f;

	private static readonly string[] StartupFace = new string[8] { "           _______            ", "          /       \\           ", "         /  _____  \\          ", "        / / _   _ \\ \\         ", "       [ | (O) (O) | ]        ", "        | \\  . .  / |         ", " _______|  | _._ |  |_______  ", "/        \\  \\___/  /        \\ " };

	private const int StartupFaceEyesLine = 4;

	private const string StartupFaceEyesCentre = "       [ | (O) (O) | ]        ";

	private const string StartupFaceEyesLeft = "       [ |(O) (O)  | ]        ";

	private const string StartupFaceEyesRight = "       [ |  (O) (O)| ]        ";

	private const string StartupFaceEyesFailed = "       [ | (X) (X) | ]        ";

	private static readonly string[] StartupFaceLookCycle = new string[4] { "       [ |(O) (O)  | ]        ", "       [ | (O) (O) | ]        ", "       [ |  (O) (O)| ]        ", "       [ | (O) (O) | ]        " };

	[Tooltip("Seconds each eye position shows for on the start screen's monkey face")]
	public float startupFaceFrameTime = 1.5f;

	[Tooltip("Seconds the start screen's monkey looks straight ahead after each look around")]
	public float startupFacePauseTime = 3f;

	private int shownStartupFaceFrame = -1;

	private static readonly char[] StartupFaceRareEyes = new char[3] { '@', '$', '^' };

	[Tooltip("Chance (0-1) of each rare eye (@, $, ^) on the start screen's monkey, rolled once per look cycle")]
	public float startupFaceRareEyeChance = 0.05f;

	private int startupFaceEyeCycle = -1;

	private char startupFaceEye = 'O';

	[Tooltip("Seconds between flashes of the start screen's press-key line")]
	public float startupPressKeyFlashInterval = 3f;

	[Tooltip("Seconds the start screen's press-key line is hidden for on each flash")]
	public float startupPressKeyFlashOffTime = 0.5f;

	private bool shownStartupPressKey;

	[Tooltip("Seconds without a key press before the computer goes back to the start screen")]
	public float startupScreensaverTime = 120f;

	private float lastKeyPressTime;

	private const int BorderedScreenWidth = 44;

	private const int BorderedScreenHeight = 15;

	private static readonly string[] StartupDateTimeFormats = new string[2] { "MM-dd HH:mm", "HH:mm" };

	private const string StartupPlayersSeparator = " ";

	private readonly StringBuilder borderedScreen = new StringBuilder();

	private int borderedLineCount;

	private static readonly Dictionary<string, string> SupportPlatformKeys = new Dictionary<string, string>
	{
		{ "OCULUS PC", "PLATFORM_OCULUS_PC" },
		{ "STEAM", "PLATFORM_STEAM" },
		{ "PSVR", "PLATFORM_PSVR" },
		{ "PICO", "PLATFORM_PICO" },
		{ "QUEST", "PLATFORM_QUEST" }
	};

	private const string VISUALS_SCREEN_GOLD_NAME_TAG_KEY = "VISUALS_SCREEN_GOLD_NAME_TAG";

	private const string VOICE_CHAT_SCREEN_MUTE_ALL_KEY = "VOICE_CHAT_SCREEN_MUTE_ALL";

	private const string ENABLED_COLOR = "#85ffa5";

	private const string DISABLED_COLOR = "\"RED\"";

	private const string FAMILY_PORTAL_URL = "k-id.com/code";

	private float _updateAttemptCooldown = 15f;

	private float _nextUpdateAttemptTime;

	private bool _waitingForUpdatedSession;

	private EKidScreenState _currentScreenState = EKidScreenState.Show_OTP;

	private string[] _interestedPermissionNames = new string[3] { "custom-username", "voice-chat", "join-groups" };

	private const string LANG_SCREEN_TITLE_KEY = "LANG_SCREEN_TITLE";

	private const string LANG_SCREEN_INSTRUCTIONS_KEY = "LANG_SCREEN_INSTRUCTIONS";

	private const string LANG_SCREEN_CURRENT_LANGUAGE_KEY = "LANG_SCREEN_CURRENT_LANGUAGE";

	private StringBuilder _languagesDisplaySB = new StringBuilder();

	private Locale _previousLocalisationSetting;

	private static readonly Dictionary<string, string> s_localisedTextCache = new Dictionary<string, string>();

	[OnEnterPlay_SetNull]
	private static Locale s_localisedTextCacheLocale;

	public string versionMismatch => _versionMismatchText.Value;

	public string unableToConnect => _unableToConnectText.Value;

	public string[] allowedMapsToJoin { get; set; }

	public string version { get; private set; }

	public string VStumpRoomPrepend => virtualStumpRoomPrepend;

	public string VStumpRoomFullPrepend => virtualStumpRoomModePrefix + virtualStumpRoomPrepend;

	public ComputerState currentState
	{
		get
		{
			stateStack.TryPeek(out var result);
			return result;
		}
	}

	public string NameTagPlayerPref
	{
		get
		{
			if (PlayFabAuthenticator.instance == null)
			{
				Debug.LogError("Trying to access PlayFab Authenticator Instance, but it is null. Will use a shared key for the nametag instead");
				return "nameTagsOn";
			}
			return "nameTagsOn-" + PlayFabAuthenticator.instance.GetPlayFabPlayerId();
		}
	}

	public bool NametagsEnabled { get; private set; }

	public RedemptionResult RedemptionStatus
	{
		get
		{
			return redemptionResult;
		}
		set
		{
			redemptionResult = value;
			UpdateScreen();
		}
	}

	public string RedemptionCode
	{
		get
		{
			return redemptionCode;
		}
		set
		{
			redemptionCode = value;
		}
	}

	public DateTimeOffset? RedemptionRestrictionTime { get; set; }

	public static bool OptionColorsEnabled
	{
		get
		{
			return s_optionColorsEnabled;
		}
		set
		{
			if (s_optionColorsEnabled != value)
			{
				s_optionColorsEnabled = value;
				onKeyLabelColorsChanged?.Invoke();
			}
		}
	}

	private bool IsRoomLimitedInRanked => limitOnlineScreens;

	public bool IsScreenTextFullScreen
	{
		get
		{
			if (currentState != 0 && currentState != ComputerState.Snake)
			{
				return screenText.IsFailed;
			}
			return true;
		}
	}

	private void InitializeAutoMuteState()
	{
		switch (PlayerPrefs.GetInt("autoMute", 1))
		{
		case 0:
			autoMuteType = "OFF";
			break;
		case 1:
			autoMuteType = "MODERATE";
			break;
		case 2:
			autoMuteType = "AGGRESSIVE";
			break;
		}
	}

	private void ProcessAutoMuteState(GorillaKeyboardBindings buttonPressed)
	{
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.option1:
			autoMuteType = "AGGRESSIVE";
			PlayerPrefs.SetInt("autoMute", 2);
			PlayerPrefs.Save();
			RigContainer.RefreshAllRigVoices();
			break;
		case GorillaKeyboardBindings.option2:
			autoMuteType = "MODERATE";
			PlayerPrefs.SetInt("autoMute", 1);
			PlayerPrefs.Save();
			RigContainer.RefreshAllRigVoices();
			break;
		case GorillaKeyboardBindings.option3:
			autoMuteType = "OFF";
			PlayerPrefs.SetInt("autoMute", 0);
			PlayerPrefs.Save();
			RigContainer.RefreshAllRigVoices();
			break;
		}
		UpdateScreen();
	}

	private void AutomuteScreen()
	{
		string fallback = "AUTOMOD AUTOMATICALLY MUTES PLAYERS WHEN THEY JOIN YOUR ROOM IF A LOT OF OTHER PLAYERS HAVE MUTED THEM";
		string localisedText = GetLocalisedText("AUTOMOD_SCREEN_INTRO", fallback, "Automute Screen");
		screenText.Set(localisedText);
		fallback = "\nPRESS OPTION 1 FOR AGGRESSIVE MUTING\nPRESS OPTION 2 FOR MODERATE MUTING\nPRESS OPTION 3 TO TURN AUTOMOD OFF";
		localisedText = GetLocalisedText("AUTOMOD_SCREEN_OPTIONS", fallback, "Automute Screen");
		TryColorSentences(localisedText, out var colored, AutomodAggressiveColor, AutomodModerateColor, AutomodOffColor);
		screenText.Append(colored);
		fallback = "\n\nCURRENT AUTOMOD LEVEL: ";
		localisedText = GetLocalisedText("AUTOMOD_SCREEN_CURRENT", fallback, "Automute Screen");
		screenText.Append(localisedText.TrailingSpace());
		string key = "AUTOMOD_OFF";
		switch (autoMuteType)
		{
		case "OFF":
			key = "AUTOMOD_OFF";
			break;
		case "MODERATE":
			key = "AUTOMOD_MODERATE";
			break;
		case "AGGRESSIVE":
			key = "AUTOMOD_AGGRESSIVE";
			break;
		}
		localisedText = GetLocalisedText(key, autoMuteType, "Automute Screen");
		screenText.Append(localisedText);
	}

	private void InitializeColorState()
	{
		redValue = PlayerPrefs.GetFloat("redValue", 0f);
		greenValue = PlayerPrefs.GetFloat("greenValue", 0f);
		blueValue = PlayerPrefs.GetFloat("blueValue", 0f);
		colorCursorLine = 0;
		GorillaTagger.Instance.UpdateColor(redValue, greenValue, blueValue);
	}

	private void ProcessColorState(GorillaKeyboardBindings buttonPressed)
	{
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.option1:
			colorCursorLine = 0;
			return;
		case GorillaKeyboardBindings.option2:
			colorCursorLine = 1;
			return;
		case GorillaKeyboardBindings.option3:
			colorCursorLine = 2;
			return;
		case GorillaKeyboardBindings.enter:
			return;
		}
		if (buttonPressed < GorillaKeyboardBindings.up)
		{
			switch (colorCursorLine)
			{
			case 0:
				redValue = (float)buttonPressed / 9f;
				PlayerPrefs.SetFloat("redValue", redValue);
				break;
			case 1:
				greenValue = (float)buttonPressed / 9f;
				PlayerPrefs.SetFloat("greenValue", greenValue);
				break;
			case 2:
				blueValue = (float)buttonPressed / 9f;
				PlayerPrefs.SetFloat("blueValue", blueValue);
				break;
			}
			GorillaTagger.Instance.UpdateColor(redValue, greenValue, blueValue);
			PlayerPrefs.Save();
			if (NetworkSystem.Instance.InRoom)
			{
				GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, redValue, greenValue, blueValue);
			}
		}
	}

	private void ColorScreen()
	{
		string localisedText = GetLocalisedText("COLOR_SELECT_INTRO", "USE THE OPTIONS BUTTONS TO SELECT THE COLOR TO UPDATE, THEN PRESS 0-9 TO SET A NEW VALUE.", "Color Screen");
		screenText.Set(localisedText);
		localisedText = GetLocalisedText("COLOR_RED", "RED", "Color Screen");
		screenText.Append("\n\n");
		screenText.Append(localisedText);
		screenText.Append(Mathf.FloorToInt(redValue * 9f) + ((colorCursorLine == 0) ? "<--" : ""));
		localisedText = GetLocalisedText("COLOR_GREEN", "GREEN", "Color Screen");
		screenText.Append("\n\n");
		screenText.Append(localisedText);
		screenText.Append(Mathf.FloorToInt(greenValue * 9f) + ((colorCursorLine == 1) ? "<--" : ""));
		localisedText = GetLocalisedText("COLOR_BLUE", "BLUE", "Color Screen");
		screenText.Append("\n\n");
		screenText.Append(localisedText);
		screenText.Append(Mathf.FloorToInt(blueValue * 9f) + ((colorCursorLine == 2) ? "<--" : ""));
	}

	public void UpdateColor(float red, float green, float blue)
	{
		redValue = Mathf.Clamp(red, 0f, 1f);
		greenValue = Mathf.Clamp(green, 0f, 1f);
		blueValue = Mathf.Clamp(blue, 0f, 1f);
	}

	private void InitializeCreditsState()
	{
	}

	private void ProcessCreditsState(GorillaKeyboardBindings buttonPressed)
	{
		if (buttonPressed == GorillaKeyboardBindings.enter)
		{
			creditsView.ProcessButtonPress(buttonPressed);
		}
	}

	private void CreditsScreen()
	{
		screenText.Set(creditsView.GetScreenText());
	}

	public DateTime GetServerTime()
	{
		return startupTime + TimeSpan.FromSeconds(Time.realtimeSinceStartup);
	}

	public void AddServerTime(int m)
	{
		startupTime = startupTime.AddMinutes(m);
		if (ScheduledEventManager.Instance != null)
		{
			ScheduledEventManager.Instance.AddMinutes(m);
		}
	}

	private static bool IsValidVStumpModePrefix(char c)
	{
		if (c != 'A' && c != 'B')
		{
			return c == 'C';
		}
		return true;
	}

	public bool IsVStumpRoomName(string roomName)
	{
		if (string.IsNullOrEmpty(roomName) || string.IsNullOrEmpty(virtualStumpRoomPrepend))
		{
			return false;
		}
		if (roomName.Length > 1 && IsValidVStumpModePrefix(roomName[0]))
		{
			return roomName[1] == virtualStumpRoomPrepend[0];
		}
		return false;
	}

	public void SetVStumpRoomModePrefix(string prefix)
	{
		virtualStumpRoomModePrefix = ((prefix != null && prefix.Length == 1 && IsValidVStumpModePrefix(prefix[0])) ? prefix : "C");
	}

	public string StripVStumpRoomPrefix(string roomName)
	{
		if (string.IsNullOrEmpty(roomName) || string.IsNullOrEmpty(virtualStumpRoomPrepend))
		{
			return roomName;
		}
		int num = roomName.IndexOf(virtualStumpRoomPrepend, StringComparison.Ordinal);
		if (num < 0)
		{
			return roomName;
		}
		return roomName.Substring(num + virtualStumpRoomPrepend.Length);
	}

	public string GetVStumpRoomDisplayName(string roomName)
	{
		if (!IsVStumpRoomName(roomName))
		{
			return roomName;
		}
		return roomName[0] switch
		{
			'A' => "WEST" + roomName.Substring(1), 
			'B' => "EAST" + roomName.Substring(1), 
			_ => roomName.Substring(1), 
		};
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			hasInstance = true;
		}
		else if (instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		version = Application.version;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("==== GORILLA TAG - VERSION: " + version + ", BUILD NUMBER: " + buildCode + ", BUILD DATE: " + buildDate + " ====\n.\n");
		string[] startupFace = StartupFace;
		foreach (string text in startupFace)
		{
			stringBuilder.Append(".    ").Append(text.TrimEnd()).Append('\n');
		}
		stringBuilder.Append(".\n.\n");
		Debug.Log(stringBuilder.ToString());
		_activeOrderList = OrderList;
		defaultUpdateCooldown = updateCooldown;
	}

	private void Start()
	{
		GTDev.Log("Computer Init");
		Initialize();
	}

	public void OnEnable()
	{
		KIDManager.RegisterSessionUpdatedCallback_VoiceChat(SetVoiceChatBySafety);
		KIDManager.RegisterSessionUpdatedCallback_CustomUsernames(OnKIDSessionUpdated_CustomNicknames);
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		LoadingRoutine = null;
		KIDManager.UnregisterSessionUpdatedCallback_VoiceChat(SetVoiceChatBySafety);
		KIDManager.UnregisterSessionUpdatedCallback_CustomUsernames(OnKIDSessionUpdated_CustomNicknames);
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	protected void OnDestroy()
	{
		if (instance == this)
		{
			hasInstance = false;
			instance = null;
		}
		KIDManager.UnregisterSessionUpdateCallback_AnyPermission(OnSessionUpdate_GorillaComputer);
		if (initialized)
		{
			GameEvents.OnGorrillaKeyboardButtonPressedEvent.RemoveListener(PressButton);
			RoomSystem.JoinedRoomEvent -= new Action(OnFirstJoinedRoom_IncrementSessionCount);
			RoomSystem.JoinedRoomEvent -= new Action(UpdateScreen);
			RoomSystem.LeftRoomEvent -= new Action(UpdateScreen);
			RoomSystem.PlayerJoinedEvent -= new Action<NetPlayer>(PlayerCountChangedCallback);
			RoomSystem.PlayerLeftEvent -= new Action<NetPlayer>(PlayerCountChangedCallback);
			LocalisationManager.UnregisterOnLanguageChanged(OnLanguageChanged_RefreshNames);
			LocalisationManager.UnregisterOnLanguageChanged(OnLanguageChanged);
		}
	}

	private void OnLanguageChanged_RefreshNames()
	{
		RefreshFunctionNames();
		UpdateGameModeText();
	}

	public void SliceUpdate()
	{
		UpdateStartupScreensaver();
		UpdateStartupScreenAnimation();
		UpdateSnake();
		if ((internetFailure && Time.realtimeSinceStartup < lastCheckedWifi + checkIfConnectedSeconds) || (!internetFailure && Time.realtimeSinceStartup < lastCheckedWifi + checkIfDisconnectedSeconds))
		{
			if (!internetFailure && isConnectedToMaster && Time.realtimeSinceStartup > lastUpdateTime + updateCooldown)
			{
				lastUpdateTime = Time.realtimeSinceStartup;
				UpdateScreen();
			}
			return;
		}
		lastCheckedWifi = Time.realtimeSinceStartup;
		if (!CheckInternetConnection())
		{
			string fallback = "NO WIFI OR LAN CONNECTION DETECTED.";
			string localisedText = GetLocalisedText("NO_CONNECTION", fallback, "Wifi Issue");
			UpdateFailureText(localisedText);
			internetFailure = true;
		}
		else if (internetFailure)
		{
			internetFailure = false;
			if (generalFailureText != null)
			{
				UpdateFailureText(generalFailureText);
				return;
			}
			RestoreFromFailureState();
			UpdateScreen();
		}
		else if (isConnectedToMaster && Time.realtimeSinceStartup > lastUpdateTime + updateCooldown)
		{
			lastUpdateTime = Time.realtimeSinceStartup;
			UpdateScreen();
		}
	}

	private void Initialize()
	{
		GameEvents.OnGorrillaKeyboardButtonPressedEvent.AddListener(PressButton);
		RoomSystem.JoinedRoomEvent += new Action(OnFirstJoinedRoom_IncrementSessionCount);
		RoomSystem.JoinedRoomEvent += new Action(UpdateScreen);
		RoomSystem.LeftRoomEvent += new Action(UpdateScreen);
		RoomSystem.PlayerJoinedEvent += new Action<NetPlayer>(PlayerCountChangedCallback);
		RoomSystem.PlayerLeftEvent += new Action<NetPlayer>(PlayerCountChangedCallback);
		LocalisationManager.RegisterOnLanguageChanged(OnLanguageChanged_RefreshNames);
		RefreshFunctionNames();
		InitializeRoomScreens();
		InitializeStrings();
		InitializeAllRoomStates();
		UpdateScreen();
		byte[] bytes = new byte[1] { Convert.ToByte(64) };
		virtualStumpRoomPrepend = Encoding.ASCII.GetString(bytes);
		initialized = true;
	}

	private void InitializeRoomScreens()
	{
		screenText.Initialize(computerScreenRenderer.materials, wrongVersionMaterial, GameEvents.ScreenTextChangedEvent, GameEvents.ScreenTextMaterialsEvent);
		functionSelectText.Initialize(computerScreenRenderer.materials, wrongVersionMaterial, GameEvents.FunctionSelectTextChangedEvent);
	}

	private void InitializeStrings()
	{
		roomToJoin = string.Empty;
		currentName = string.Empty;
		savedName = string.Empty;
	}

	private void InitializeAllRoomStates()
	{
		SwitchState(ComputerState.Startup);
		InitializeLanguageState();
		InitializeNameState();
		InitializeRoomState();
		InitializeTurnState();
		InitializeStartupState();
		InitializeQueueState();
		InitializeMicState();
		InitializeGroupState();
		InitializeVoiceState();
		InitializeAutoMuteState();
		InitializeGameMode();
		InitializeVisualsState();
		InitializeCreditsState();
		InitializeTimeState();
		InitializeSupportState();
		InitializeTroopState();
		InitializeKIdState();
		InitializeRedeemState();
	}

	private void InitializeStartupState()
	{
	}

	public void InitializeGameMode(string gameMode)
	{
		leftHanded = PlayerPrefs.GetInt("leftHanded", 0) == 1;
		OnModeSelectButtonPress(gameMode, leftHanded);
		GameModePages.SetSelectedGameModeShared(gameMode);
		didInitializeGameMode = true;
	}

	private void InitializeGameMode()
	{
		if (!didInitializeGameMode)
		{
			sessionCount = PlayerPrefs.GetInt("sessionCount", -1);
			string text = PlayerPrefs.GetString("currentGameModePostSI");
			if (sessionCount == -1)
			{
				sessionCount = ((text.Length != 0) ? 100 : 0);
				PlayerPrefs.SetInt("sessionCount", sessionCount);
				text = GameModeType.Infection.ToString();
				PlayerPrefs.SetString("currentGameModePostSI", text);
				PlayerPrefs.Save();
			}
			GameModeType gameModeType;
			try
			{
				gameModeType = Enum.Parse<GameModeType>(text, ignoreCase: true);
			}
			catch
			{
				gameModeType = GameModeType.SuperInfect;
				text = GameModeType.SuperInfect.ToString();
			}
			if (!GameMode.GameModeZoneMapping.AllModes.Contains(gameModeType) || gameModeType == GameModeType.None || gameModeType == GameModeType.Count)
			{
				GTDev.Log("[GT/GorillaComputer]  InitializeGameMode: Falling back to default game mode " + $"\"{GameModeType.SuperInfect}\" because stored game mode \"{gameModeType}\" is not available in any zone.");
				PlayerPrefs.SetString("currentGameModePostSI", GameModeType.SuperInfect.ToString());
				PlayerPrefs.Save();
				text = GameModeType.SuperInfect.ToString();
			}
			leftHanded = PlayerPrefs.GetInt("leftHanded", 0) == 1;
			OnModeSelectButtonPress(text, leftHanded);
			GameModePages.SetSelectedGameModeShared(text);
		}
	}

	private bool CheckInternetConnection()
	{
		return Application.internetReachability != NetworkReachability.NotReachable;
	}

	public void OnConnectedToMasterStuff()
	{
		if (!isConnectedToMaster)
		{
			isConnectedToMaster = true;
			GorillaServer.Instance.ReturnCurrentVersion(new ReturnCurrentVersionRequest
			{
				CurrentVersion = NetworkSystemConfig.AppVersionStripped,
				UpdatedSynchTest = includeUpdatedServerSynchTest
			}, OnReturnCurrentVersion, OnErrorShared);
			if (startupMillis == 0L && !tryGetTimeAgain)
			{
				GetCurrentTime();
			}
			bool safety = PlayFabAuthenticator.instance.GetSafety();
			if (!KIDManager.KidEnabledAndReady && !KIDManager.HasSession)
			{
				SetComputerSettingsBySafety(safety, new ComputerState[4]
				{
					ComputerState.Voice,
					ComputerState.AutoMute,
					ComputerState.Name,
					ComputerState.Group
				}, shouldHide: false);
			}
		}
	}

	private void OnReturnCurrentVersion(ExecuteFunctionResult result)
	{
		JsonObject jsonObject = (JsonObject)result.FunctionResult;
		if (jsonObject != null)
		{
			if (jsonObject.TryGetValue("SynchTime", out var value))
			{
				GTDev.Log("message value is: " + (string)value);
			}
			if (jsonObject.TryGetValue("Fail", out value) && (bool)value)
			{
				GeneralFailureMessage(versionMismatch);
				return;
			}
			if (jsonObject.TryGetValue("ResultCode", out value) && (ulong)value != 0L)
			{
				GeneralFailureMessage(versionMismatch);
				return;
			}
			if (jsonObject.TryGetValue("QueueStats", out value))
			{
				JsonObject jsonObject2 = (JsonObject)value;
				GTDev.Log("QueueStats: " + jsonObject2);
				if (jsonObject2.TryGetValue("TopTroops", out value))
				{
					topTroops.Clear();
					foreach (object item in (JsonArray)value)
					{
						topTroops.Add(item.ToString());
					}
				}
				if (jsonObject2.TryGetValue("TopVstumpMapIds", out value))
				{
					topVstumpMaps.Clear();
					foreach (object item2 in (JsonArray)value)
					{
						topVstumpMaps.Add(item2.ToString());
					}
				}
			}
			if (jsonObject.TryGetValue("BannedUsers", out value))
			{
				usersBanned = int.Parse((string)value);
			}
			UpdateScreen();
		}
		else
		{
			GeneralFailureMessage(versionMismatch);
		}
	}

	public void PressButton(GorillaKeyboardBindings buttonPressed)
	{
		if (buttonPressed == GorillaKeyboardBindings.none || screenText.IsFailed)
		{
			return;
		}
		lastKeyPressTime = Time.time;
		nameCheckRateLimited = false;
		if (currentState == ComputerState.Startup)
		{
			ProcessStartupState(buttonPressed);
			UpdateScreen();
			return;
		}
		if (currentState == ComputerState.Snake)
		{
			ProcessSnakeState(buttonPressed);
			UpdateScreen();
			return;
		}
		if (currentState != ComputerState.Room)
		{
			ResetSnakeSecretCode();
		}
		RequestTroopPopulation();
		bool flag = true;
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.up:
			flag = false;
			DecreaseState();
			break;
		case GorillaKeyboardBindings.down:
			flag = false;
			IncreaseState();
			break;
		}
		if (flag)
		{
			switch (currentState)
			{
			case ComputerState.Language:
				ProcessLanguageState(buttonPressed);
				break;
			case ComputerState.Room:
				ProcessRoomState(buttonPressed);
				break;
			case ComputerState.Name:
				ProcessNameState(buttonPressed);
				break;
			case ComputerState.Turn:
				ProcessTurnState(buttonPressed);
				break;
			case ComputerState.Mic:
				ProcessMicState(buttonPressed);
				break;
			case ComputerState.Queue:
				ProcessQueueState(buttonPressed);
				break;
			case ComputerState.Group:
				ProcessGroupState(buttonPressed);
				break;
			case ComputerState.Voice:
				ProcessVoiceState(buttonPressed);
				break;
			case ComputerState.AutoMute:
				ProcessAutoMuteState(buttonPressed);
				break;
			case ComputerState.Credits:
				ProcessCreditsState(buttonPressed);
				break;
			case ComputerState.Support:
				ProcessSupportState(buttonPressed);
				break;
			case ComputerState.Visuals:
				ProcessVisualsState(buttonPressed);
				break;
			case ComputerState.NameWarning:
				ProcessNameWarningState(buttonPressed);
				break;
			case ComputerState.Troop:
				ProcessTroopState(buttonPressed);
				break;
			case ComputerState.KID:
				ProcessKIdState(buttonPressed);
				break;
			case ComputerState.Redemption:
				ProcessRedemptionState(buttonPressed);
				break;
			}
		}
		UpdateScreen();
	}

	public void OnModeSelectButtonPress(string gameMode, bool leftHand)
	{
		lastPressedGameMode = gameMode;
		lastPressedGameModeType = (GameModeType)GameMode.gameModeKeyByName.GetValueOrDefault(gameMode, 11);
		PlayerPrefs.SetString("currentGameModePostSI", gameMode);
		if (leftHand != leftHanded)
		{
			PlayerPrefs.SetInt("leftHanded", leftHand ? 1 : 0);
			leftHanded = leftHand;
		}
		PlayerPrefs.Save();
		if (FriendshipGroupDetection.Instance.IsInParty)
		{
			FriendshipGroupDetection.Instance.SendRequestPartyGameMode(gameMode);
		}
		else
		{
			SetGameModeWithoutButton(gameMode);
		}
	}

	public void SetGameModeWithoutButton(string gameMode)
	{
		currentGameMode.Value = gameMode;
		UpdateGameModeText();
		PhotonNetworkController.Instance.UpdateTriggerScreens();
	}

	public void CompQueueUnlockButtonPress()
	{
		allowedInCompetitive = true;
		PlayerPrefs.SetInt("allowedInCompetitive", 1);
		PlayerPrefs.Save();
		if (RankedProgressionManager.Instance != null)
		{
			RankedProgressionManager.Instance.RequestUnlockCompetitiveQueue(unlock: true);
		}
	}

	private void SwitchState(ComputerState newState, bool clearStack = true)
	{
		if (currentComputerState == ComputerState.Mic && currentComputerState != newState)
		{
			updateCooldown = defaultUpdateCooldown;
		}
		else if (newState == ComputerState.Mic)
		{
			updateCooldown = micUpdateCooldown;
		}
		if (previousComputerState != currentComputerState)
		{
			previousComputerState = currentComputerState;
		}
		currentComputerState = newState;
		if (LoadingRoutine != null)
		{
			StopCoroutine(LoadingRoutine);
			LoadingRoutine = null;
		}
		if (clearStack)
		{
			stateStack.Clear();
		}
		stateStack.Push(newState);
	}

	private void PopState()
	{
		currentComputerState = previousComputerState;
		if (stateStack.Count <= 1)
		{
			Debug.LogError("Can't pop into an empty stack");
			return;
		}
		stateStack.Pop();
		UpdateScreen();
	}

	private void SwitchToLoadingState()
	{
		SwitchState(ComputerState.Loading, clearStack: false);
	}

	private void ProcessStartupState(GorillaKeyboardBindings buttonPressed)
	{
		SwitchState(GetState(currentStateIndex));
	}

	public void JoinDefaultQueue()
	{
		JoinQueue("DEFAULT");
	}

	private bool IsValidTroopName(string troop)
	{
		if (!string.IsNullOrEmpty(troop) && troop.Length <= 12)
		{
			if (!allowedInCompetitive)
			{
				return troop != "COMPETITIVE";
			}
			return true;
		}
		return false;
	}

	private string GetQueueNameForTroop(string troop)
	{
		return troop;
	}

	public void UpdateScreen()
	{
		if (NetworkSystem.Instance != null && !NetworkSystem.Instance.WrongVersion)
		{
			UpdateFunctionScreen();
			switch (currentState)
			{
			case ComputerState.Startup:
				StartupScreen();
				break;
			case ComputerState.Language:
				LanguageScreen();
				break;
			case ComputerState.Room:
				RoomScreen();
				break;
			case ComputerState.Name:
				NameScreen();
				break;
			case ComputerState.Turn:
				TurnScreen();
				break;
			case ComputerState.Queue:
				QueueScreen();
				break;
			case ComputerState.Mic:
				MicScreen();
				break;
			case ComputerState.Group:
				GroupScreen();
				break;
			case ComputerState.Voice:
				VoiceScreen();
				break;
			case ComputerState.AutoMute:
				AutomuteScreen();
				break;
			case ComputerState.Visuals:
				VisualsScreen();
				break;
			case ComputerState.Credits:
				CreditsScreen();
				break;
			case ComputerState.Time:
				TimeScreen();
				break;
			case ComputerState.Support:
				SupportScreen();
				break;
			case ComputerState.NameWarning:
				NameWarningScreen();
				break;
			case ComputerState.Loading:
				LoadingScreen();
				break;
			case ComputerState.Troop:
				TroopScreen();
				break;
			case ComputerState.KID:
				KIdScreen();
				break;
			case ComputerState.Redemption:
				RedemptionScreen();
				break;
			case ComputerState.Snake:
				SnakeScreen();
				break;
			}
		}
		UpdateGameModeText();
		UpdateKeyLabelColors();
	}

	private void LoadingScreen()
	{
		string result;
		if (LoadingRoutine == null)
		{
			string fallback = "LOADING";
			result = GetLocalisedText("LOADING_SCREEN", fallback, "Loading Screen").TrimEnd('.', '…', ' ');
			LoadingRoutine = StartCoroutine(LoadingScreenLocal());
		}
		IEnumerator LoadingScreenLocal()
		{
			int dotsCount = 0;
			while (currentState == ComputerState.Loading)
			{
				dotsCount = dotsCount % 3 + 1;
				screenText.Set(result);
				for (int i = 0; i < dotsCount; i++)
				{
					screenText.Append(". ");
				}
				yield return waitLoadingDotStep;
			}
			LoadingRoutine = null;
		}
	}

	private void LimitedOnlineFunctionalityScreen()
	{
		string fallback = "NOT AVAILABLE IN RANKED PLAY";
		string localisedText = GetLocalisedText("LIMITED_ONLINE_FUNC", fallback, "Limited Functionality Screen");
		screenText.Set(localisedText);
	}

	private void NameCheckRateLimitedMessage()
	{
		string fallback = "\n\nTOO MANY NAME CHECKS!\nWAIT A MINUTE AND TRY AGAIN.";
		string localisedText = GetLocalisedText("NAME_CHECK_RATE_LIMITED", fallback, "Name Check Rate Limited");
		screenText.Append(localisedText);
	}

	private void UpdateGameModeText()
	{
		string fallback = "CURRENT MODE";
		string localisedText = GetLocalisedText("CURRENT_MODE", fallback, "Limited Functionality Screen");
		string text2;
		if (!NetworkSystem.Instance.InRoom || GorillaGameManager.instance == null)
		{
			string text = localisedText;
			fallback = "-NOT IN ROOM-";
			localisedText = GetLocalisedText("NOT_IN_ROOM", fallback, "Limited Functionality Screen");
			text2 = text + localisedText;
		}
		else
		{
			text2 = localisedText + "\n" + GorillaGameManager.instance.GameModeName();
		}
		if (currentGameModeText.Value != text2)
		{
			currentGameModeText.Value = text2;
		}
	}

	private void UpdateFunctionScreen()
	{
		int stateIndex = GetStateIndex(currentState);
		if (functionListTextDirty || stateIndex != functionListTextIndex || MaxVisibleFunctions != functionListTextMaxVisible)
		{
			functionListTextDirty = false;
			functionListTextIndex = stateIndex;
			functionListTextMaxVisible = MaxVisibleFunctions;
			functionSelectText.Set(GetOrderListForScreen(currentState));
		}
	}

	private void CheckAutoBanListForRoomName(string nameToCheck)
	{
		SwitchToLoadingState();
		CheckForBadRoomName(nameToCheck);
	}

	private void CheckAutoBanListForPlayerName(string nameToCheck)
	{
		SwitchToLoadingState();
		CheckForBadPlayerName(nameToCheck);
	}

	private void CheckAutoBanListForTroopName(string nameToCheck)
	{
		if (IsValidTroopName(troopToJoin))
		{
			SwitchToLoadingState();
			CheckForBadTroopName(nameToCheck);
		}
	}

	private void CheckForBadRoomName(string nameToCheck)
	{
		GorillaServer.Instance.CheckForBadName(new CheckForBadNameRequest
		{
			name = nameToCheck,
			forRoom = true,
			forTroop = false
		}, OnRoomNameChecked, OnErrorNameCheck);
	}

	private void CheckForBadPlayerName(string nameToCheck)
	{
		GorillaServer.Instance.CheckForBadName(new CheckForBadNameRequest
		{
			name = nameToCheck,
			forRoom = false,
			forTroop = false
		}, OnPlayerNameChecked, OnErrorNameCheck);
	}

	private void CheckForBadTroopName(string nameToCheck)
	{
		GorillaServer.Instance.CheckForBadName(new CheckForBadNameRequest
		{
			name = nameToCheck,
			forRoom = false,
			forTroop = true
		}, OnTroopNameChecked, OnErrorNameCheck);
	}

	private void OnRoomNameChecked(ExecuteFunctionResult result)
	{
		if (((JsonObject)result.FunctionResult).TryGetValue("result", out var value))
		{
			switch ((NameCheckResult)int.Parse(value.ToString()))
			{
			case NameCheckResult.Success:
				if (FriendshipGroupDetection.Instance.IsInParty && !FriendshipGroupDetection.Instance.IsPartyWithinCollider(friendJoinCollider))
				{
					FriendshipGroupDetection.Instance.LeaveParty();
				}
				if (playerInVirtualStump)
				{
					if (CustomMapManager.IsInFeaturedMode())
					{
						CustomMapManager.PrepareFeaturedMapReloadOnRoomChange();
					}
					CustomMapManager.UnloadMap(returnToSinglePlayerIfInPublic: false);
				}
				networkController.AttemptToJoinSpecificRoom(roomToJoin, FriendshipGroupDetection.Instance.IsInParty ? JoinType.ForceJoinWithParty : JoinType.Solo);
				break;
			case NameCheckResult.Warning:
				roomToJoin = "";
				roomToJoin += (playerInVirtualStump ? VStumpRoomFullPrepend : "");
				SwitchToWarningState();
				break;
			case NameCheckResult.Ban:
				roomToJoin = "";
				roomToJoin += (playerInVirtualStump ? VStumpRoomFullPrepend : "");
				GorillaGameManager.ForceStopGame_DisconnectAndDestroy();
				break;
			}
		}
		if (currentState == ComputerState.Loading)
		{
			PopState();
		}
	}

	private void OnPlayerNameChecked(ExecuteFunctionResult result)
	{
		if (((JsonObject)result.FunctionResult).TryGetValue("result", out var value))
		{
			switch ((NameCheckResult)int.Parse(value.ToString()))
			{
			case NameCheckResult.Success:
				NetworkSystem.Instance.SetMyNickName(currentName);
				CustomMapsTerminal.RequestDriverNickNameRefresh();
				break;
			case NameCheckResult.Warning:
				NetworkSystem.Instance.SetMyNickName("gorilla");
				CustomMapsTerminal.RequestDriverNickNameRefresh();
				currentName = "gorilla";
				SwitchToWarningState();
				break;
			case NameCheckResult.Ban:
				NetworkSystem.Instance.SetMyNickName("gorilla");
				CustomMapsTerminal.RequestDriverNickNameRefresh();
				currentName = "gorilla";
				GorillaGameManager.ForceStopGame_DisconnectAndDestroy();
				break;
			}
		}
		SetLocalNameTagText(currentName);
		savedName = currentName;
		PlayerPrefs.SetString("playerName", currentName);
		PlayerPrefs.Save();
		if (NetworkSystem.Instance.InRoom)
		{
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, redValue, greenValue, blueValue);
		}
		if (currentState == ComputerState.Loading)
		{
			PopState();
		}
	}

	private void OnTroopNameChecked(ExecuteFunctionResult result)
	{
		if (((JsonObject)result.FunctionResult).TryGetValue("result", out var value))
		{
			switch ((NameCheckResult)int.Parse(value.ToString()))
			{
			case NameCheckResult.Success:
				JoinTroop(troopToJoin);
				break;
			case NameCheckResult.Warning:
				troopToJoin = string.Empty;
				SwitchToWarningState();
				break;
			case NameCheckResult.Ban:
				troopToJoin = string.Empty;
				GorillaGameManager.ForceStopGame_DisconnectAndDestroy();
				break;
			}
		}
		if (currentState == ComputerState.Loading)
		{
			PopState();
		}
	}

	private void OnErrorNameCheck(PlayFabError error)
	{
		nameCheckRateLimited = IsNameCheckRateLimited(error);
		if (currentState == ComputerState.Loading)
		{
			PopState();
		}
		OnErrorShared(error);
	}

	private static bool IsNameCheckRateLimited(PlayFabError error)
	{
		if (error.ErrorMessage != null)
		{
			return error.ErrorMessage.Contains("HTTP status TooManyRequests");
		}
		return false;
	}

	public bool CheckAutoBanListForName(string nameToCheck)
	{
		nameToCheck = nameToCheck.ToLower();
		nameToCheck = new string(Array.FindAll(nameToCheck.ToCharArray(), (char c) => char.IsLetterOrDigit(c)));
		string[] array = anywhereTwoWeek;
		foreach (string value in array)
		{
			if (nameToCheck.IndexOf(value) >= 0)
			{
				return false;
			}
		}
		array = anywhereOneWeek;
		foreach (string value2 in array)
		{
			if (nameToCheck.IndexOf(value2) >= 0 && !nameToCheck.Contains("fagol"))
			{
				return false;
			}
		}
		array = exactOneWeek;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == nameToCheck)
			{
				return false;
			}
		}
		return true;
	}

	public void UpdateFailureText(string failMessage)
	{
		GorillaScoreboardTotalUpdater.instance.SetOfflineFailureText(failMessage);
		PhotonNetworkController.Instance.UpdateTriggerScreens();
		screenText.EnableFailedState(BuildFailureScreen(failMessage));
		functionSelectText.EnableFailedState(failMessage);
		UpdateKeyLabelColors();
	}

	private void RestoreFromFailureState()
	{
		GorillaScoreboardTotalUpdater.instance.ClearOfflineFailureText();
		PhotonNetworkController.Instance.UpdateTriggerScreens();
		screenText.DisableFailedState();
		functionSelectText.DisableFailedState();
	}

	public void GeneralFailureMessage(string failMessage)
	{
		generalFailureText = failMessage;
		isConnectedToMaster = false;
		NetworkSystem.Instance.SetWrongVersion();
		UpdateFailureText(failMessage);
		UpdateScreen();
	}

	private static void OnErrorShared(PlayFabError error)
	{
		if (error.Error == PlayFabErrorCode.NotAuthenticated)
		{
			PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
		}
		else if (error.Error == PlayFabErrorCode.AccountBanned)
		{
			GorillaGameManager.ForceStopGame_DisconnectAndDestroy();
		}
		if (error.ErrorMessage == "The account making this request is currently banned")
		{
			using (Dictionary<string, List<string>>.Enumerator enumerator = error.ErrorDetails.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					KeyValuePair<string, List<string>> current = enumerator.Current;
					if (current.Value[0] != "Indefinite")
					{
						instance.GeneralFailureMessage("YOUR ACCOUNT " + PlayFabAuthenticator.instance.GetPlayFabPlayerId() + " HAS BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: " + current.Key + "\nHOURS LEFT: " + (int)((DateTime.Parse(current.Value[0]) - DateTime.UtcNow).TotalHours + 1.0));
					}
					else
					{
						instance.GeneralFailureMessage("YOUR ACCOUNT " + PlayFabAuthenticator.instance.GetPlayFabPlayerId() + " HAS BEEN BANNED INDEFINITELY.\nREASON: " + current.Key);
					}
				}
				return;
			}
		}
		if (!(error.ErrorMessage == "The IP making this request is currently banned"))
		{
			return;
		}
		using Dictionary<string, List<string>>.Enumerator enumerator = error.ErrorDetails.GetEnumerator();
		if (enumerator.MoveNext())
		{
			KeyValuePair<string, List<string>> current2 = enumerator.Current;
			if (current2.Value[0] != "Indefinite")
			{
				instance.GeneralFailureMessage("THIS IP HAS BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: " + current2.Key + "\nHOURS LEFT: " + (int)((DateTime.Parse(current2.Value[0]) - DateTime.UtcNow).TotalHours + 1.0));
			}
			else
			{
				instance.GeneralFailureMessage("THIS IP HAS BEEN BANNED INDEFINITELY.\nREASON: " + current2.Key);
			}
		}
	}

	private void DecreaseState()
	{
		int num = currentStateIndex;
		currentStateIndex--;
		if (GetState(currentStateIndex) == ComputerState.Time)
		{
			currentStateIndex--;
		}
		if (currentStateIndex < 0)
		{
			if (!LoopFunctionSelection)
			{
				currentStateIndex = num;
				return;
			}
			currentStateIndex = FunctionsCount - 1;
		}
		SwitchState(GetState(currentStateIndex));
	}

	private void IncreaseState()
	{
		int num = currentStateIndex;
		currentStateIndex++;
		if (GetState(currentStateIndex) == ComputerState.Time)
		{
			currentStateIndex++;
		}
		if (currentStateIndex >= FunctionsCount)
		{
			if (!LoopFunctionSelection)
			{
				currentStateIndex = num;
				return;
			}
			currentStateIndex = 0;
		}
		SwitchState(GetState(currentStateIndex));
	}

	public ComputerState GetState(int index)
	{
		try
		{
			return _activeOrderList[index].State;
		}
		catch
		{
			return _activeOrderList[0].State;
		}
	}

	public int GetStateIndex(ComputerState state)
	{
		for (int i = 0; i < _activeOrderList.Count; i++)
		{
			if (_activeOrderList[i].State == state)
			{
				return i;
			}
		}
		return -1;
	}

	public string GetOrderListForScreen(ComputerState currentState)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int stateIndex = GetStateIndex(currentState);
		bool flag = MaxVisibleFunctions > 0 && FunctionsCount > MaxVisibleFunctions;
		int num = 0;
		int num2 = FunctionsCount;
		if (flag)
		{
			num2 = Mathf.Min(MaxVisibleFunctions, FunctionsCount);
			UpdateFunctionScrollOffset(stateIndex, num2);
			num = _functionScrollOffset;
			if (num > 0)
			{
				stringBuilder.Append(ScrollUpIndicator);
			}
			stringBuilder.Append("\n");
		}
		int num3 = num + num2 - 1;
		for (int i = num; i <= num3; i++)
		{
			stringBuilder.Append(FunctionNames[i]);
			if (i == stateIndex)
			{
				stringBuilder.Append(Pointer);
			}
			if (i < num3)
			{
				stringBuilder.Append("\n");
			}
		}
		if (flag && num3 < FunctionsCount - 1)
		{
			stringBuilder.Append("\n").Append(ScrollDownIndicator);
		}
		return stringBuilder.ToString();
	}

	private void UpdateFunctionScrollOffset(int currentIndex, int visibleCount)
	{
		if (currentIndex < 0)
		{
			currentIndex = 0;
		}
		if (currentIndex < _functionScrollOffset)
		{
			_functionScrollOffset = currentIndex;
		}
		else if (currentIndex >= _functionScrollOffset + visibleCount)
		{
			_functionScrollOffset = currentIndex - visibleCount + 1;
		}
		_functionScrollOffset = Mathf.Clamp(_functionScrollOffset, 0, FunctionsCount - visibleCount);
	}

	private void GetCurrentTime()
	{
		tryGetTimeAgain = true;
		PlayFabClientAPI.GetTime(new GetTimeRequest(), OnGetTimeSuccess, OnGetTimeFailure);
	}

	private void OnGetTimeSuccess(GetTimeResult result)
	{
		startupMillis = (long)(TimeSpan.FromTicks(result.Time.Ticks).TotalMilliseconds - (double)(Time.realtimeSinceStartup * 1000f));
		startupTime = result.Time - TimeSpan.FromSeconds(Time.realtimeSinceStartup);
		OnServerTimeUpdated?.Invoke();
	}

	private void OnGetTimeFailure(PlayFabError error)
	{
		startupMillis = (long)(TimeSpan.FromTicks(DateTime.UtcNow.Ticks).TotalMilliseconds - (double)(Time.realtimeSinceStartup * 1000f));
		startupTime = DateTime.UtcNow - TimeSpan.FromSeconds(Time.realtimeSinceStartup);
		OnServerTimeUpdated?.Invoke();
		if (error.Error == PlayFabErrorCode.NotAuthenticated)
		{
			PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
		}
		else if (error.Error == PlayFabErrorCode.AccountBanned)
		{
			GorillaGameManager.ForceStopGame_DisconnectAndDestroy();
		}
	}

	private void PlayerCountChangedCallback(NetPlayer player)
	{
		UpdateScreen();
	}

	private static void OnFirstJoinedRoom_IncrementSessionCount()
	{
		RoomSystem.JoinedRoomEvent -= new Action(OnFirstJoinedRoom_IncrementSessionCount);
		sessionCount++;
		PlayerPrefs.SetInt("sessionCount", sessionCount);
		PlayerPrefs.Save();
	}

	public void SetNameBySafety(bool isSafety)
	{
		if (isSafety)
		{
			PlayerPrefs.SetString("playerNameBackup", currentName);
			currentName = "gorilla" + UnityEngine.Random.Range(0, 9999).ToString().PadLeft(4, '0');
			savedName = currentName;
			NetworkSystem.Instance.SetMyNickName(currentName);
			SetLocalNameTagText(currentName);
			PlayerPrefs.SetString("playerName", currentName);
			PlayerPrefs.Save();
			if (NetworkSystem.Instance.InRoom)
			{
				GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, redValue, greenValue, blueValue);
			}
		}
	}

	public void SetLocalNameTagText(string newName)
	{
		VRRig.LocalRig.SetNameTagText(newName);
	}

	public void SetComputerSettingsBySafety(bool isSafety, ComputerState[] toFilterOut, bool shouldHide)
	{
		_activeOrderList = OrderList;
		if (!isSafety)
		{
			_activeOrderList = OrderList;
			if (_filteredStates.Count > 0 && toFilterOut.Length != 0)
			{
				for (int i = 0; i < toFilterOut.Length; i++)
				{
					if (_filteredStates.Contains(toFilterOut[i]))
					{
						_filteredStates.Remove(toFilterOut[i]);
					}
				}
			}
		}
		else if (shouldHide)
		{
			for (int j = 0; j < toFilterOut.Length; j++)
			{
				if (!_filteredStates.Contains(toFilterOut[j]))
				{
					_filteredStates.Add(toFilterOut[j]);
				}
			}
		}
		if (_filteredStates.Count > 0)
		{
			int k = 0;
			for (int num = _activeOrderList.Count; k < num; k++)
			{
				if (_filteredStates.Contains(_activeOrderList[k].State))
				{
					_activeOrderList.RemoveAt(k);
					k--;
					num--;
				}
			}
		}
		FunctionsCount = _activeOrderList.Count;
		FunctionNames.Clear();
		functionListTextDirty = true;
		_activeOrderList.ForEach(delegate(StateOrderItem s)
		{
			string text = s.GetName();
			if (text.Length > highestCharacterCount)
			{
				highestCharacterCount = text.Length;
			}
			FunctionNames.Add(text);
		});
		for (int l = 0; l < FunctionsCount; l++)
		{
			int num2 = highestCharacterCount - FunctionNames[l].Length;
			for (int m = 0; m < num2; m++)
			{
				FunctionNames[l] += " ";
			}
		}
		UpdateScreen();
	}

	public void KID_SetVoiceChatSettingOnStart(bool voiceChatEnabled, Permission.ManagedByEnum managedBy, bool hasOptedInPreviously)
	{
		if (managedBy != Permission.ManagedByEnum.PROHIBITED)
		{
			SetVoice(voiceChatEnabled, !hasOptedInPreviously);
		}
	}

	private void SetVoice(bool setting, bool saveSetting = true)
	{
		voiceChatOn = (setting ? "TRUE" : "FALSE");
		if (setting && !KIDManager.CheckFeatureOptIn(EKIDFeatures.Voice_Chat).hasOptedInPreviously)
		{
			KIDManager.SetFeatureOptIn(EKIDFeatures.Voice_Chat, optedIn: true);
			KIDManager.SendOptInPermissions();
		}
		if (saveSetting)
		{
			PlayerPrefs.SetString("voiceChatOn", voiceChatOn);
			PlayerPrefs.Save();
		}
	}

	public bool CheckVoiceChatEnabled()
	{
		return voiceChatOn == "TRUE";
	}

	private void SetVoiceChatBySafety(bool voiceChatEnabled, Permission.ManagedByEnum managedBy)
	{
		bool isSafety = !voiceChatEnabled;
		SetComputerSettingsBySafety(isSafety, new ComputerState[3]
		{
			ComputerState.Voice,
			ComputerState.AutoMute,
			ComputerState.Mic
		}, shouldHide: false);
		string value = PlayerPrefs.GetString("voiceChatOn", "");
		if (KIDManager.KidEnabledAndReady)
		{
			Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Voice_Chat);
			if (permissionDataByFeature != null)
			{
				(bool, bool) tuple = KIDManager.CheckFeatureOptIn(EKIDFeatures.Voice_Chat, permissionDataByFeature);
				if (tuple.Item1 && !tuple.Item2)
				{
					value = "FALSE";
				}
			}
			else
			{
				Debug.LogErrorFormat("[KID] Could not find permission data for [" + EKIDFeatures.Voice_Chat.ToStandardisedString() + "]");
			}
		}
		switch (managedBy)
		{
		case Permission.ManagedByEnum.GUARDIAN:
			if (KIDManager.GetPermissionDataByFeature(EKIDFeatures.Voice_Chat).Enabled)
			{
				if (string.IsNullOrEmpty(value))
				{
					voiceChatOn = "TRUE";
				}
				else
				{
					voiceChatOn = value;
				}
			}
			else
			{
				voiceChatOn = "FALSE";
			}
			break;
		case Permission.ManagedByEnum.PROHIBITED:
			voiceChatOn = "FALSE";
			break;
		case Permission.ManagedByEnum.PLAYER:
			if (string.IsNullOrEmpty(value))
			{
				voiceChatOn = (voiceChatEnabled ? "TRUE" : "FALSE");
			}
			else
			{
				voiceChatOn = value;
			}
			break;
		}
		RigContainer.RefreshAllRigVoices();
		GTDev.Log("[KID] On Session Update - Voice Chat Permission changed - Has enabled voiceChat? [" + voiceChatEnabled + "]");
	}

	public void SetNametagSetting(bool setting, Permission.ManagedByEnum managedBy, bool hasOptedInPreviously)
	{
		switch (managedBy)
		{
		case Permission.ManagedByEnum.PROHIBITED:
			break;
		case Permission.ManagedByEnum.GUARDIAN:
		{
			int @int = PlayerPrefs.GetInt(NameTagPlayerPref, 1);
			setting = setting && @int == 1;
			UpdateNametagSetting(setting, saveSetting: false);
			break;
		}
		default:
			setting = PlayerPrefs.GetInt(NameTagPlayerPref, setting ? 1 : 0) == 1;
			UpdateNametagSetting(setting, !hasOptedInPreviously && setting);
			break;
		}
	}

	public static void RegisterOnNametagSettingChanged(Action<bool> callback)
	{
		onNametagSettingChangedAction = (Action<bool>)Delegate.Combine(onNametagSettingChangedAction, callback);
	}

	public static void UnregisterOnNametagSettingChanged(Action<bool> callback)
	{
		onNametagSettingChangedAction = (Action<bool>)Delegate.Remove(onNametagSettingChangedAction, callback);
	}

	private void UpdateNametagSetting(bool newSettingValue, bool saveSetting = true)
	{
		if (newSettingValue)
		{
			KIDManager.SetFeatureOptIn(EKIDFeatures.Custom_Nametags, optedIn: true);
		}
		NametagsEnabled = newSettingValue;
		NetworkSystem.Instance.SetMyNickName(NametagsEnabled ? savedName : NetworkSystem.Instance.GetMyDefaultName());
		if (NetworkSystem.Instance.InRoom)
		{
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, redValue, greenValue, blueValue);
		}
		onNametagSettingChangedAction?.Invoke(NametagsEnabled);
		if (saveSetting)
		{
			int value = (NametagsEnabled ? 1 : 0);
			PlayerPrefs.SetInt(NameTagPlayerPref, value);
			PlayerPrefs.Save();
		}
	}

	public void SetInVirtualStump(bool inVirtualStump)
	{
		GrowingSnowballThrowable.ForceAOEEnabled = inVirtualStump;
		playerInVirtualStump = inVirtualStump;
		roomToJoin = (playerInVirtualStump ? (VStumpRoomFullPrepend + roomToJoin) : StripVStumpRoomPrefix(roomToJoin));
	}

	public bool IsPlayerInVirtualStump()
	{
		return playerInVirtualStump;
	}

	public void SetLimitOnlineScreens(bool isLimited)
	{
		limitOnlineScreens = isLimited;
		UpdateScreen();
	}

	private void InitializeGroupState()
	{
		groupMapJoin = PlayerPrefs.GetString("groupMapJoin", "FOREST");
		groupMapJoinIndex = PlayerPrefs.GetInt("groupMapJoinIndex", 0);
		allowedMapsToJoin = friendJoinCollider.myAllowedMapsToJoin;
	}

	public void RegisterPrimaryJoinTrigger(GorillaNetworkJoinTrigger trigger)
	{
		primaryTriggersByZone[trigger.networkZone] = trigger;
	}

	private GorillaNetworkJoinTrigger GetSelectedMapJoinTrigger()
	{
		string text = groupMapJoin.ToLowerInvariant();
		if (Array.IndexOf(allowedMapsToJoin, text) < 0)
		{
			text = allowedMapsToJoin[Mathf.Min(allowedMapsToJoin.Length - 1, groupMapJoinIndex)];
		}
		primaryTriggersByZone.TryGetValue(text, out var value);
		return value;
	}

	public GorillaNetworkJoinTrigger GetJoinTriggerForZone(string zone)
	{
		primaryTriggersByZone.TryGetValue(zone, out var value);
		return value;
	}

	public GorillaNetworkJoinTrigger GetJoinTriggerFromFullGameModeString(string gameModeString)
	{
		foreach (KeyValuePair<string, GorillaNetworkJoinTrigger> item in primaryTriggersByZone)
		{
			if (gameModeString.StartsWith(item.Key))
			{
				return item.Value;
			}
		}
		return null;
	}

	public void OnGroupJoinButtonPress(int mapJoinIndex, GorillaFriendCollider chosenFriendJoinCollider)
	{
		GTDev.Log("On Group button press. Map:" + mapJoinIndex + " - collider: " + chosenFriendJoinCollider.name);
		if (mapJoinIndex >= allowedMapsToJoin.Length)
		{
			roomNotAllowed = true;
			currentStateIndex = 0;
			SwitchState(GetState(currentStateIndex));
			return;
		}
		GorillaNetworkJoinTrigger selectedMapJoinTrigger = GetSelectedMapJoinTrigger();
		if (FriendshipGroupDetection.Instance.IsInParty)
		{
			if (selectedMapJoinTrigger != null && selectedMapJoinTrigger.CanPartyJoin())
			{
				PhotonNetworkController.Instance.AttemptToJoinPublicRoom(selectedMapJoinTrigger, JoinType.ForceJoinWithParty);
				currentStateIndex = 0;
				SwitchState(GetState(currentStateIndex));
			}
			else
			{
				UpdateScreen();
			}
		}
		else if (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.SessionIsPrivate)
		{
			PhotonNetworkController.Instance.SetFriendIDList(chosenFriendJoinCollider.playerIDsCurrentlyTouching);
			GTDev.Log("Friend IDs: " + string.Join(", ", networkController.FriendIDList));
			PhotonNetworkController.Instance.shuffler = UnityEngine.Random.Range(0, 99).ToString().PadLeft(2, '0') + UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
			PhotonNetworkController.Instance.keyStr = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
			RoomSystem.SendNearbyFollowCommand(chosenFriendJoinCollider, PhotonNetworkController.Instance.shuffler, PhotonNetworkController.Instance.keyStr);
			PhotonNetwork.SendAllOutgoingCommands();
			PhotonNetworkController.Instance.AttemptToJoinPublicRoom(selectedMapJoinTrigger, JoinType.JoinWithNearby);
			currentStateIndex = 0;
			SwitchState(GetState(currentStateIndex));
		}
	}

	private void ProcessGroupState(GorillaKeyboardBindings buttonPressed)
	{
		if (!limitOnlineScreens)
		{
			switch (buttonPressed)
			{
			case GorillaKeyboardBindings.one:
				SetGroupMapJoin("FOREST", 0);
				break;
			case GorillaKeyboardBindings.two:
				SetGroupMapJoin("CANYON", 1);
				break;
			case GorillaKeyboardBindings.three:
				SetGroupMapJoin("CITY", 2);
				break;
			case GorillaKeyboardBindings.enter:
				OnGroupJoinButtonPress(Mathf.Min(allowedMapsToJoin.Length - 1, groupMapJoinIndex), friendJoinCollider);
				break;
			}
			roomFull = false;
		}
	}

	private void SetGroupMapJoin(string groupMap, int groupMapIndex)
	{
		groupMapJoin = groupMap;
		groupMapJoinIndex = groupMapIndex;
		PlayerPrefs.SetString("groupMapJoin", groupMapJoin);
		PlayerPrefs.SetInt("groupMapJoinIndex", groupMapJoinIndex);
		PlayerPrefs.Save();
	}

	private void GroupScreen()
	{
		if (limitOnlineScreens)
		{
			LimitedOnlineFunctionalityScreen();
			return;
		}
		string text = "";
		string text2 = "";
		string text3 = ((allowedMapsToJoin.Length > 1) ? groupMapJoin : allowedMapsToJoin[0].ToUpper());
		string text4 = "";
		if (allowedMapsToJoin.Length > 1)
		{
			text = "\n\nUSE NUMBER KEYS TO SELECT DESTINATION\n1: FOREST, 2: CANYON, 3: CITY.";
			text2 = GetLocalisedText("GROUP_SCREEN_DESTINATIONS", text, "Groups Screen");
			text4 = HighlightNumberedEntries(HighlightNumberKeys(text2));
		}
		text = "\n\nACTIVE ZONE WILL BE:";
		text2 = GetLocalisedText("GROUP_SCREEN_ACTIVE_ZONES", text, "Groups Screen");
		string text5 = text2.TrailingSpace();
		text5 = text5 + text3 + text4;
		if (FriendshipGroupDetection.Instance.IsInParty)
		{
			GorillaNetworkJoinTrigger selectedMapJoinTrigger = GetSelectedMapJoinTrigger();
			string text6 = "";
			if (!selectedMapJoinTrigger.CanPartyJoin())
			{
				text = "\n\n<color=red>CANNOT JOIN BECAUSE YOUR GROUP IS NOT HERE</color>";
				text2 = GetLocalisedText("GROUP_SCREEN_CANNOT_JOIN", text, "Groups Screen");
				text6 = text2;
			}
			text = "PRESS ENTER TO JOIN A PUBLIC GAME WITH YOUR FRIENDSHIP GROUP.";
			text2 = GetLocalisedText("GROUP_SCREEN_ENTER_PARTY", text, "Groups Screen");
			screenText.Set(text2);
			text5 += text6;
			screenText.Append(text5);
		}
		else
		{
			text = "PRESS ENTER TO JOIN A PUBLIC GAME AND BRING EVERYONE IN THIS ROOM WITH YOU.";
			text2 = GetLocalisedText("GROUP_SCREEN_ENTER_NOPARTY", text, "Groups Screen");
			screenText.Set(text2);
			screenText.Append(text5);
		}
	}

	private bool IsKeyLabelColorActive(GorillaKeyboardBindings binding)
	{
		switch (currentState)
		{
		case ComputerState.Queue:
			if (binding == GorillaKeyboardBindings.option3)
			{
				return allowedInCompetitive;
			}
			return true;
		case ComputerState.Visuals:
			return binding switch
			{
				GorillaKeyboardBindings.option3 => SubscriptionManager.IsSubscriptionFeatureAvailable(SubscriptionManager.SubscriptionFeatures.IOBT), 
				GorillaKeyboardBindings.option4 => SubscriptionManager.IsLocalSubscribed(), 
				_ => true, 
			};
		case ComputerState.Troop:
			return IsTroopOptionShown(binding);
		default:
			return true;
		}
	}

	private bool IsTroopOptionShown(GorillaKeyboardBindings binding)
	{
		if (limitOnlineScreens || !IsValidTroopName(troopName) || !KIDManager.HasPermissionToUseFeature(EKIDFeatures.Groups) || !KIDManager.HasPermissionToUseFeature(EKIDFeatures.Multiplayer))
		{
			return false;
		}
		return binding switch
		{
			GorillaKeyboardBindings.option1 => !troopQueueActive, 
			GorillaKeyboardBindings.option2 => troopQueueActive, 
			_ => true, 
		};
	}

	public static void RegisterOnKeyLabelColorsChanged(Action callback)
	{
		onKeyLabelColorsChanged = (Action)Delegate.Combine(onKeyLabelColorsChanged, callback);
	}

	public static void UnregisterOnKeyLabelColorsChanged(Action callback)
	{
		onKeyLabelColorsChanged = (Action)Delegate.Remove(onKeyLabelColorsChanged, callback);
	}

	public bool TryGetKeyLabelColor(GorillaKeyboardBindings binding, out Color color)
	{
		color = default(Color);
		if (!s_optionColorsEnabled || screenText.IsFailed)
		{
			return false;
		}
		if (!s_optionKeyColorsEnabled)
		{
			return false;
		}
		if (s_pageKeyLabelColors.TryGetValue(currentState, out var value) && value.TryGetValue(binding, out color))
		{
			return IsKeyLabelColorActive(binding);
		}
		return false;
	}

	private void UpdateKeyLabelColors()
	{
		bool isFailed = screenText.IsFailed;
		ulong coloredKeys = GetColoredKeys();
		if (keyLabelColorsState != currentState || keyLabelColorsFailed != isFailed || keyLabelColorsActiveKeys != coloredKeys)
		{
			keyLabelColorsState = currentState;
			keyLabelColorsFailed = isFailed;
			keyLabelColorsActiveKeys = coloredKeys;
			onKeyLabelColorsChanged?.Invoke();
		}
	}

	private ulong GetColoredKeys()
	{
		if (!s_pageKeyLabelColors.TryGetValue(currentState, out var value))
		{
			return 0uL;
		}
		ulong num = 0uL;
		foreach (GorillaKeyboardBindings key in value.Keys)
		{
			if (IsKeyLabelColorActive(key))
			{
				num |= (ulong)(1L << (int)key);
			}
		}
		return num;
	}

	private static bool TryColorSentences(string text, out string colored, params Color[] colors)
	{
		if (!s_optionColorsEnabled)
		{
			colored = text;
			return false;
		}
		if (s_coloredSentencesCache.TryGetValue(text, out colored))
		{
			return colored != text;
		}
		colored = text;
		s_coloredSentencesCache[text] = text;
		string text2 = text.Trim();
		if (text2.Length == 0)
		{
			return false;
		}
		List<string> list = SplitNonEmpty(text2.Split('\n'));
		if (list.Count != colors.Length)
		{
			list = SplitNonEmpty(s_sentenceEnd.Split(text2));
		}
		if (list.Count != colors.Length)
		{
			return false;
		}
		string optionWord = FindOptionWord(s_optionNumber.Matches(text2));
		int num = text.IndexOf(text2, StringComparison.Ordinal);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(text, 0, num);
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append('\n');
			}
			AppendOptionHighlighted(stringBuilder, list[i], colors[i], optionWord);
		}
		stringBuilder.Append(text, num + text2.Length, text.Length - num - text2.Length);
		colored = stringBuilder.ToString();
		s_coloredSentencesCache[text] = colored;
		return true;
	}

	private static bool TryColorOptionNumbers(string text, out string colored, params (int number, Color color)[] options)
	{
		if (!s_optionColorsEnabled)
		{
			colored = text;
			return false;
		}
		if (s_coloredSentencesCache.TryGetValue(text, out colored))
		{
			return colored != text;
		}
		colored = text;
		s_coloredSentencesCache[text] = text;
		MatchCollection matchCollection = s_optionNumber.Matches(text);
		string text2 = FindOptionWord(matchCollection);
		if (text2 == null)
		{
			return false;
		}
		List<(Match, Color)> list = new List<(Match, Color)>();
		foreach (Match item in matchCollection)
		{
			if (!(item.Groups[1].Value != text2))
			{
				int number2 = int.Parse(item.Groups[2].Value);
				int num = Array.FindIndex(options, ((int number, Color color) o) => o.number == number2);
				if (num < 0)
				{
					return false;
				}
				list.Add((item, options[num].color));
			}
		}
		for (int i = 0; i < options.Length; i++)
		{
			var (number, _) = options[i];
			if (list.FindAll(((Match match, Color color) f) => int.Parse(f.match.Groups[2].Value) == number).Count != 1)
			{
				return false;
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num2 = 0;
		foreach (var (match2, color) in list)
		{
			stringBuilder.Append(text, num2, match2.Index - num2);
			AppendColored(stringBuilder, match2.Value, color);
			num2 = match2.Index + match2.Length;
		}
		stringBuilder.Append(text, num2, text.Length - num2);
		colored = stringBuilder.ToString();
		s_coloredSentencesCache[text] = colored;
		return true;
	}

	private static string FindOptionWord(MatchCollection matches)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		string text = null;
		foreach (Match match in matches)
		{
			string value = match.Groups[1].Value;
			dictionary.TryGetValue(value, out var value2);
			value2 = (dictionary[value] = value2 + 1);
			if (text == null || value2 > dictionary[text])
			{
				text = value;
			}
		}
		return text;
	}

	private static void AppendOptionHighlighted(StringBuilder builder, string line, Color color, string optionWord)
	{
		if (optionWord != null && color.a > 0f)
		{
			foreach (Match item in s_optionNumber.Matches(line))
			{
				if (!(item.Groups[1].Value != optionWord))
				{
					builder.Append(line, 0, item.Index);
					AppendColored(builder, item.Value, color);
					builder.Append(line, item.Index + item.Length, line.Length - item.Index - item.Length);
					return;
				}
			}
		}
		builder.Append(line);
	}

	private static string HighlightPhrase(string text, string[] phrases)
	{
		if (!s_optionColorsEnabled || string.IsNullOrEmpty(text))
		{
			return text;
		}
		foreach (string text2 in phrases)
		{
			int num = text.IndexOf(text2, StringComparison.Ordinal);
			if (num >= 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(text, 0, num);
				AppendColored(stringBuilder, text2, OptionTextHighlightColor);
				stringBuilder.Append(text, num + text2.Length, text.Length - num - text2.Length);
				return stringBuilder.ToString();
			}
		}
		return text;
	}

	private static string HighlightNumberKeys(string text)
	{
		return HighlightPhrase(text, s_numberKeysPhrases);
	}

	public static string HighlightEnterKey(string text)
	{
		return HighlightPhrase(text, s_enterKeyPhrases);
	}

	private static string HighlightVolumeRange(string text)
	{
		return HighlightPhrase(text, s_volumeRangePhrases);
	}

	private static string HighlightNumberedEntries(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		return s_numberedEntry.Replace(text, (Match match) => HighlightNumber(match.Groups[1].Value));
	}

	private static string HighlightNumber(string number)
	{
		if (!s_optionColorsEnabled)
		{
			return number + ")";
		}
		StringBuilder stringBuilder = new StringBuilder();
		AppendColored(stringBuilder, number + ")", OptionTextHighlightColor);
		return stringBuilder.ToString();
	}

	private static void AppendColored(StringBuilder builder, string text, Color color)
	{
		if (color.a <= 0f)
		{
			builder.Append(text);
		}
		else
		{
			builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(OptionTextHighlightColor)).Append('>')
				.Append(text)
				.Append("</color>");
		}
	}

	private static List<string> SplitNonEmpty(string[] pieces)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < pieces.Length; i++)
		{
			string text = pieces[i].Trim();
			if (text.Length > 0)
			{
				list.Add(text);
			}
		}
		return list;
	}

	private void InitializeMicState()
	{
		pttType = PlayerPrefs.GetString("pttType", "OPEN MIC");
		if (pttType == "ALL CHAT")
		{
			pttType = "OPEN MIC";
			PlayerPrefs.SetString("pttType", pttType);
			PlayerPrefs.Save();
		}
	}

	private void ProcessMicState(GorillaKeyboardBindings buttonPressed)
	{
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.option1:
			pttType = "OPEN MIC";
			PlayerPrefs.SetString("pttType", pttType);
			PlayerPrefs.Save();
			break;
		case GorillaKeyboardBindings.option2:
			pttType = "PUSH TO TALK";
			PlayerPrefs.SetString("pttType", pttType);
			PlayerPrefs.Save();
			break;
		case GorillaKeyboardBindings.option3:
			pttType = "PUSH TO MUTE";
			PlayerPrefs.SetString("pttType", pttType);
			PlayerPrefs.Save();
			break;
		}
	}

	private void RefreshMicAvailability()
	{
		if (!(Time.realtimeSinceStartup < lastMicAvailabilityCheckTime + 1f))
		{
			lastMicAvailabilityCheckTime = Time.realtimeSinceStartup;
			micDeviceFound = micPermissionGranted && Microphone.devices.Length != 0;
		}
	}

	private static string GetMicInputTestUnavailableReason()
	{
		if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom)
		{
			return "NOT AVAILABLE. JOIN A ROOM TO TEST YOUR MIC.";
		}
		if (!(NetworkSystem.Instance.LocalRecorder is GTRecorder { IsRecording: not false }))
		{
			return "NOT AVAILABLE. MIC IS NOT RECORDING.";
		}
		return null;
	}

	private void MicScreen()
	{
		if (KIDManager.GetPermissionDataByFeature(EKIDFeatures.Voice_Chat).ManagedBy == Permission.ManagedByEnum.PROHIBITED)
		{
			MicScreen_KIdProhibited();
			return;
		}
		RefreshMicAvailability();
		bool flag = false;
		string text = "";
		if (!micDeviceFound)
		{
			flag = true;
			text = "NO MICROPHONE DETECTED";
		}
		if (flag)
		{
			string localisedText = GetLocalisedText("MIC_SCREEN_MIC_DISABLED", "MIC DISABLED: ", "Mic Screen");
			screenText.Set(localisedText.TrailingSpace() + text);
			return;
		}
		string fallback = "PRESS OPTION 1 = ALL CHAT.\nPRESS OPTION 2 = PUSH TO TALK.\nPRESS OPTION 3 = PUSH TO MUTE.";
		string localisedText2 = GetLocalisedText("MIC_SCREEN_OPTIONS", fallback, "Mic Screen");
		TryColorSentences(localisedText2, out var colored, MicAllChatColor, MicPushToTalkColor, MicPushToMuteColor);
		screenText.Set(colored);
		fallback = "\n\nCURRENT MIC SETTING:";
		localisedText2 = GetLocalisedText("MIC_SCREEN_CURRENT", fallback, "Mic Screen");
		screenText.Append(localisedText2.TrailingSpace());
		string key = "";
		switch (pttType)
		{
		case "PUSH TO MUTE":
			key = "PUSH_TO_MUTE_MIC";
			break;
		case "PUSH TO TALK":
			key = "PUSH_TO_TALK_MIC";
			break;
		case "OPEN MIC":
			key = "OPEN_MIC";
			break;
		case "ALL CHAT":
			key = "OPEN_MIC";
			break;
		}
		localisedText2 = GetLocalisedText(key, pttType, "Mic Screen");
		screenText.Append(localisedText2);
		if (pttType == "PUSH TO MUTE")
		{
			fallback = "- MIC IS OPEN.\n- HOLD ANY FACE BUTTON TO MUTE.";
			localisedText2 = GetLocalisedText("MIC_SCREEN_PUSH_TO_MUTE_TOOLTIP", fallback, "Mic Screen");
			screenText.Append("\n" + localisedText2.Trim() + "\n\n");
		}
		else if (pttType == "PUSH TO TALK")
		{
			fallback = "- MIC IS MUTED.\n- HOLD ANY FACE BUTTON TO TALK.";
			localisedText2 = GetLocalisedText("MIC_SCREEN_PUSH_TO_TALK_TOOLTIP", fallback, "Mic Screen");
			screenText.Append("\n" + localisedText2.Trim() + "\n\n");
		}
		else
		{
			screenText.Append("\n\n\n");
		}
		string micInputTestUnavailableReason = GetMicInputTestUnavailableReason();
		if (micInputTestUnavailableReason != null)
		{
			micInputTestTimer = 0f;
			lastMicInputTestTime = Time.realtimeSinceStartup;
			fallback = "INPUT TEST: ";
			localisedText2 = GetLocalisedText("MIC_SCREEN_INPUT_TEST_LABEL", fallback, "Mic Screen");
			screenText.Append(localisedText2.TrimStart('\n'));
			screenText.Append(micInputTestUnavailableReason);
			return;
		}
		if (speakerLoudness == null)
		{
			speakerLoudness = GorillaTagger.Instance.offlineVRRig.GetComponent<GorillaSpeakerLoudness>();
		}
		if (!(speakerLoudness != null))
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = Mathf.Min(realtimeSinceStartup - lastMicInputTestTime, 1f);
		lastMicInputTestTime = realtimeSinceStartup;
		float num2 = Mathf.Sqrt(speakerLoudness.LoudnessNormalized);
		if (num2 <= 0.01f)
		{
			micInputTestTimer += num;
		}
		else
		{
			micInputTestTimer = 0f;
		}
		if (pttType != "OPEN MIC")
		{
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool num3 = ControllerInputPoller.PrimaryButtonPress(XRNode.RightHand);
			flag2 = ControllerInputPoller.SecondaryButtonPress(XRNode.RightHand);
			flag3 = ControllerInputPoller.PrimaryButtonPress(XRNode.LeftHand);
			flag4 = ControllerInputPoller.SecondaryButtonPress(XRNode.LeftHand);
			bool flag5 = num3 || flag2 || flag3 || flag4;
			if (flag5 && pttType == "PUSH TO MUTE")
			{
				fallback = "INPUT TEST: ";
				localisedText2 = GetLocalisedText("MIC_SCREEN_INPUT_TEST_LABEL", fallback, "Mic Screen");
				screenText.Append(localisedText2.TrimStart('\n'));
				return;
			}
			if (!flag5 && pttType == "PUSH TO TALK")
			{
				fallback = "INPUT TEST: ";
				localisedText2 = GetLocalisedText("MIC_SCREEN_INPUT_TEST_LABEL", fallback, "Mic Screen");
				screenText.Append(localisedText2.TrimStart('\n'));
				return;
			}
		}
		if (micInputTestTimer >= micInputTestTimerThreshold)
		{
			fallback = "NO MIC INPUT DETECTED. CHECK MIC SETTINGS IN THE OPERATING SYSTEM.";
			localisedText2 = GetLocalisedText("MIC_SCREEN_INPUT_TEST_NO_MIC", fallback, "Mic Screen");
			screenText.Append(localisedText2);
			return;
		}
		fallback = "INPUT TEST: ";
		localisedText2 = GetLocalisedText("MIC_SCREEN_INPUT_TEST_LABEL", fallback, "Mic Screen");
		screenText.Append(localisedText2.TrimStart('\n'));
		for (int i = 0; i < Mathf.FloorToInt(num2 * 50f); i++)
		{
			screenText.Append("|");
		}
	}

	private void InitializeNameState()
	{
		int @int = PlayerPrefs.GetInt("nameTagsOn", -1);
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Custom_Nametags);
		switch (permissionDataByFeature.ManagedBy)
		{
		case Permission.ManagedByEnum.PLAYER:
			if (@int == -1)
			{
				NametagsEnabled = permissionDataByFeature.Enabled;
			}
			else
			{
				NametagsEnabled = @int > 0;
			}
			break;
		case Permission.ManagedByEnum.GUARDIAN:
			NametagsEnabled = permissionDataByFeature.Enabled && @int > 0;
			break;
		case Permission.ManagedByEnum.PROHIBITED:
			NametagsEnabled = false;
			break;
		}
		savedName = PlayerPrefs.GetString("playerName", "gorilla");
		NetworkSystem.Instance.SetMyNickName(savedName);
		currentName = savedName;
		VRRigCache.Instance.localRig.Rig.UpdateName();
		exactOneWeek = exactOneWeekFile.text.Split('\n');
		anywhereOneWeek = anywhereOneWeekFile.text.Split('\n');
		anywhereTwoWeek = anywhereTwoWeekFile.text.Split('\n');
		for (int i = 0; i < exactOneWeek.Length; i++)
		{
			exactOneWeek[i] = exactOneWeek[i].ToLower().TrimEnd('\r', '\n');
		}
		for (int j = 0; j < anywhereOneWeek.Length; j++)
		{
			anywhereOneWeek[j] = anywhereOneWeek[j].ToLower().TrimEnd('\r', '\n');
		}
		for (int k = 0; k < anywhereTwoWeek.Length; k++)
		{
			anywhereTwoWeek[k] = anywhereTwoWeek[k].ToLower().TrimEnd('\r', '\n');
		}
	}

	private void SwitchToWarningState()
	{
		warningConfirmationInputString = string.Empty;
		SwitchState(ComputerState.NameWarning, clearStack: false);
	}

	public void ProcessNameState(GorillaKeyboardBindings buttonPressed)
	{
		if (!KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags))
		{
			return;
		}
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.enter:
			if (currentName != savedName && currentName != "" && NametagsEnabled)
			{
				CheckAutoBanListForPlayerName(currentName);
			}
			break;
		case GorillaKeyboardBindings.delete:
			if (currentName.Length > 0 && NametagsEnabled)
			{
				currentName = currentName.Substring(0, currentName.Length - 1);
			}
			break;
		case GorillaKeyboardBindings.option1:
			UpdateNametagSetting(!NametagsEnabled);
			break;
		default:
		{
			if (NametagsEnabled && currentName.Length < 12 && buttonPressed.TryGetTypedText(out var text))
			{
				currentName += text;
			}
			break;
		}
		}
	}

	private void ProcessNameWarningState(GorillaKeyboardBindings buttonPressed)
	{
		string text;
		if (warningConfirmationInputString.ToLower() == "yes")
		{
			PopState();
		}
		else if (buttonPressed == GorillaKeyboardBindings.delete)
		{
			if (warningConfirmationInputString.Length > 0)
			{
				warningConfirmationInputString = warningConfirmationInputString.Substring(0, warningConfirmationInputString.Length - 1);
			}
		}
		else if (warningConfirmationInputString.Length < 3 && buttonPressed.TryGetTypedText(out text))
		{
			warningConfirmationInputString += text;
		}
	}

	private void NameWarningScreen()
	{
		string fallback = "<color=red>WARNING: PLEASE CHOOSE A BETTER NAME\n\nENTERING ANOTHER BAD NAME WILL RESULT IN A BAN</color>";
		string localisedText = GetLocalisedText("WARNING_SCREEN", fallback, "Name Warning Screen");
		screenText.Set(localisedText);
		if (warningConfirmationInputString.ToLower() == "yes")
		{
			fallback = "\n\nPRESS ANY KEY TO CONTINUE";
			localisedText = GetLocalisedText("WARNING_SCREEN_CONFIRMATION", fallback, "Name Warning Screen");
			screenText.Append(localisedText);
		}
		else
		{
			fallback = "\n\nTYPE 'YES' TO CONFIRM:";
			localisedText = GetLocalisedText("WARNING_SCREEN_TYPE_YES", fallback, "Name Warning Screen");
			screenText.Append(localisedText.TrailingSpace());
			screenText.Append(warningConfirmationInputString);
		}
	}

	private void NameScreen()
	{
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Custom_Nametags);
		if (KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags))
		{
			string fallback = "PRESS ENTER TO CHANGE YOUR NAME TO THE ENTERED NEW NAME.\n\n";
			string localisedText = GetLocalisedText("NAME_SCREEN", fallback, "Name Screen");
			screenText.Set(localisedText);
			fallback = "CURRENT NAME: ";
			localisedText = GetLocalisedText("CURRENT_NAME", fallback, "Name Screen");
			screenText.Append(localisedText.TrailingSpace());
			screenText.Append(savedName);
			if (NametagsEnabled)
			{
				fallback = "NEW NAME: ";
				localisedText = GetLocalisedText("NEW_NAME", fallback, "Name Screen");
				screenText.Append(localisedText.TrailingSpace());
				screenText.Append(currentName);
				if (nameCheckRateLimited)
				{
					NameCheckRateLimitedMessage();
				}
			}
			fallback = "PRESS OPTION 1 TO TOGGLE NAMETAGS.\nCURRENTLY NAMETAGS ARE: ";
			localisedText = GetLocalisedText("NAME_SCREEN_TOGGLE_NAMETAGS", fallback, "Name Screen");
			string key = (NametagsEnabled ? "ON_KEY" : "OFF_KEY");
			TryColorOptionNumbers(localisedText, out var colored, (1, NameNametagsColor));
			screenText.Append(colored.TrailingSpace());
			fallback = (NametagsEnabled ? "ON" : "OFF");
			localisedText = GetLocalisedText(key, fallback, "Name Screen");
			screenText.Append(localisedText);
		}
		else if (permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.PROHIBITED)
		{
			NameScreen_KIdProhibited();
		}
		else
		{
			NameScreen_Permission();
		}
	}

	private void InitializeQueueState()
	{
		currentQueue = PlayerPrefs.GetString("currentQueue", "DEFAULT");
		allowedInCompetitive = PlayerPrefs.GetInt("allowedInCompetitive", 0) == 1;
		if (!allowedInCompetitive && currentQueue == "COMPETITIVE")
		{
			PlayerPrefs.SetString("currentQueue", "DEFAULT");
			PlayerPrefs.Save();
			currentQueue = "DEFAULT";
		}
	}

	private void ProcessQueueState(GorillaKeyboardBindings buttonPressed)
	{
		if (limitOnlineScreens)
		{
			return;
		}
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.option1:
			JoinQueue("DEFAULT");
			break;
		case GorillaKeyboardBindings.option2:
			JoinQueue("MINIGAMES");
			break;
		case GorillaKeyboardBindings.option3:
			if (allowedInCompetitive)
			{
				JoinQueue("COMPETITIVE");
			}
			break;
		}
	}

	private void JoinQueue(string queueName, bool isTroopQueue = false)
	{
		currentQueue = queueName;
		troopQueueActive = isTroopQueue;
		currentTroopPopulation = -1;
		PlayerPrefs.SetString("currentQueue", currentQueue);
		PlayerPrefs.SetInt("troopQueueActive", troopQueueActive ? 1 : 0);
		PlayerPrefs.Save();
	}

	private void QueueScreen()
	{
		if (limitOnlineScreens)
		{
			LimitedOnlineFunctionalityScreen();
			return;
		}
		string fallback = "THIS OPTION AFFECTS WHO YOU PLAY WITH. DEFAULT IS FOR ANYONE TO PLAY NORMALLY. MINIGAMES IS FOR PEOPLE LOOKING TO PLAY WITH THEIR OWN MADE UP RULES.";
		string localisedText = GetLocalisedText("QUEUE_SCREEN", fallback, "Queue Screen");
		screenText.Set(localisedText.TrailingSpace());
		if (allowedInCompetitive)
		{
			fallback = "COMPETITIVE IS FOR PLAYERS WHO WANT TO PLAY THE GAME AND TRY AS HARD AS THEY CAN.";
			localisedText = GetLocalisedText("COMPETITIVE_DESC", fallback, "Queue Screen");
			screenText.Append(localisedText.TrailingSpace());
			fallback = "PRESS OPTION 1 FOR DEFAULT, OPTION 2 FOR MINIGAMES, OR OPTION 3 FOR COMPETITIVE.";
			localisedText = GetLocalisedText("QUEUE_SCREEN_ALL_QUEUES", fallback, "Queue Screen");
			TryColorOptionNumbers(localisedText, out var colored, (1, QueueDefaultColor), (2, QueueMinigamesColor), (3, QueueCompetitiveColor));
			screenText.Append(colored);
		}
		else
		{
			fallback = "BEAT THE OBSTACLE COURSE IN CITY TO ALLOW COMPETITIVE PLAY.";
			localisedText = GetLocalisedText("BEAT_OBSTACLE_COURSE", fallback, "Queue Screen");
			screenText.Append(localisedText.TrailingSpace());
			fallback = "PRESS OPTION 1 FOR DEFAULT, OR OPTION 2 FOR MINIGAMES.";
			localisedText = GetLocalisedText("QUEUE_SCREEN_DEFAULT_QUEUES", fallback, "Queue Screen");
			TryColorOptionNumbers(localisedText, out var colored2, (1, QueueDefaultColor), (2, QueueMinigamesColor));
			screenText.Append(colored2);
		}
		fallback = "\n\nCURRENT QUEUE:";
		localisedText = GetLocalisedText("CURRENT_QUEUE", fallback, "Queue Screen");
		screenText.Append(localisedText.TrailingSpace());
		string text = "DEFAULT_QUEUE";
		text = currentQueue switch
		{
			"COMPETITIVE" => "COMPETITIVE_QUEUE", 
			"MINIGAMES" => "MINIGAMES_QUEUE", 
			_ => "DEFAULT_QUEUE", 
		};
		fallback = currentQueue;
		localisedText = GetLocalisedText(text, fallback, "Queue Screen");
		screenText.Append(localisedText);
	}

	private void InitializeRedeemState()
	{
		RedemptionStatus = RedemptionResult.Empty;
	}

	private void ProcessRedemptionState(GorillaKeyboardBindings buttonPressed)
	{
		if (RedemptionStatus == RedemptionResult.Checking)
		{
			return;
		}
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.enter:
			if (redemptionCode != "")
			{
				if (redemptionCode.Length < 8)
				{
					RedemptionStatus = RedemptionResult.Invalid;
					break;
				}
				CodeRedemption.Instance.HandleCodeRedemption(redemptionCode);
				RedemptionStatus = RedemptionResult.Checking;
			}
			else if (RedemptionStatus != RedemptionResult.Success)
			{
				RedemptionStatus = RedemptionResult.Empty;
			}
			break;
		case GorillaKeyboardBindings.delete:
			if (redemptionCode.Length > 0)
			{
				redemptionCode = redemptionCode.Substring(0, redemptionCode.Length - 1);
			}
			break;
		default:
		{
			if (redemptionCode.Length < 8 && buttonPressed.TryGetTypedText(out var text))
			{
				redemptionCode += text;
			}
			break;
		}
		}
	}

	private void RedemptionScreen()
	{
		string fallback = "TYPE REDEMPTION CODE AND PRESS ENTER";
		string localisedText = GetLocalisedText("REDEMPTION_INTRO", fallback, "Redemption Screen");
		screenText.Set(localisedText);
		fallback = "\n\nCODE: " + redemptionCode;
		localisedText = GetLocalisedText("REDEMPTION_CODE_LABEL", fallback, "Redemption Screen");
		screenText.Append(localisedText.TrailingSpace());
		screenText.Append(redemptionCode);
		switch (RedemptionStatus)
		{
		case RedemptionResult.Invalid:
			fallback = "\n\nINVALID CODE";
			localisedText = GetLocalisedText("REDEMPTION_CODE_INVALID", fallback, "Redemption Screen");
			screenText.Append(localisedText);
			break;
		case RedemptionResult.Checking:
			fallback = "\n\nVALIDATING...";
			localisedText = GetLocalisedText("REDEMPTION_CODE_VALIDATING", fallback, "Redemption Screen");
			screenText.Append(localisedText);
			break;
		case RedemptionResult.AlreadyUsed:
			fallback = "\n\nCODE ALREADY CLAIMED";
			localisedText = GetLocalisedText("REDEMPTION_CODE_ALREADY_USED", fallback, "Redemption Screen");
			screenText.Append(localisedText);
			break;
		case RedemptionResult.TooEarly:
			fallback = "CODE IS NOT REDEEMABLE UNTIL";
			localisedText = GetLocalisedText("REDEMPTION_CODE_TOO_EARLY", fallback, "Redemption Screen");
			screenText.Append(RedemptionRestrictionTime.HasValue ? ("\n\n" + localisedText + "\n" + RedemptionRestrictionTime.Value.ToLocalTime().ToString("f").ToUpper()) : ("\n\n" + localisedText + "\n[MISSING]"));
			break;
		case RedemptionResult.TooLate:
			fallback = "CODE EXPIRED";
			localisedText = GetLocalisedText("REDEMPTION_CODE_TOO_LATE", fallback, "Redemption Screen");
			screenText.Append(RedemptionRestrictionTime.HasValue ? ("\n\n" + localisedText + "\n" + RedemptionRestrictionTime.Value.ToLocalTime().ToString("f").ToUpper()) : ("\n\n" + localisedText + "\n[MISSING]"));
			break;
		case RedemptionResult.AlreadyGranted:
			fallback = "ITEM ALREADY GRANTED";
			localisedText = GetLocalisedText("REDEMPTION_CODE_ALREADY_GRANTED", fallback, "Redemption Screen");
			screenText.Append("\n\n" + localisedText);
			break;
		case RedemptionResult.Success:
			fallback = "\n\nSUCCESSFULLY CLAIMED!";
			localisedText = GetLocalisedText("REDEMPTION_CODE_SUCCESS", fallback, "Redemption Screen");
			screenText.Append(localisedText);
			break;
		case RedemptionResult.Empty:
			break;
		}
	}

	private void InitializeRoomState()
	{
	}

	private void ProcessRoomState(GorillaKeyboardBindings buttonPressed)
	{
		if (TryEnterSnakeSecretCode(buttonPressed) || IsRoomLimitedInRanked)
		{
			return;
		}
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Groups) && KIDManager.HasPermissionToUseFeature(EKIDFeatures.Multiplayer);
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.option1:
			if (FriendshipGroupDetection.Instance.IsInParty)
			{
				if (FriendshipGroupDetection.Instance.IsPartyWithinCollider(friendJoinCollider))
				{
					OnGroupJoinButtonPress(0, friendJoinCollider);
					break;
				}
				FriendshipGroupDetection.Instance.LeaveParty();
				DisconnectAfterDelay(1f);
			}
			else
			{
				NetworkSystem.Instance.ReturnToSinglePlayer();
			}
			break;
		case GorillaKeyboardBindings.option2:
			RequestUpdatedPermissions();
			break;
		case GorillaKeyboardBindings.enter:
			if (flag && ((!playerInVirtualStump && roomToJoin != "") || (playerInVirtualStump && roomToJoin.Length > VStumpRoomFullPrepend.Length)))
			{
				CheckAutoBanListForRoomName(roomToJoin);
			}
			break;
		case GorillaKeyboardBindings.delete:
			if (flag && ((playerInVirtualStump && roomToJoin.Length > VStumpRoomFullPrepend.Length) || (!playerInVirtualStump && roomToJoin.Length > 0)))
			{
				roomToJoin = roomToJoin.Substring(0, roomToJoin.Length - 1);
			}
			break;
		default:
		{
			if (flag && roomToJoin.Length < 10 && buttonPressed.TryGetTypedText(out var text))
			{
				roomToJoin += text;
			}
			break;
		}
		case GorillaKeyboardBindings.option3:
			break;
		}
	}

	private async void DisconnectAfterDelay(float seconds)
	{
		await Task.Delay((int)(1000f * seconds));
		await NetworkSystem.Instance.ReturnToSinglePlayer();
	}

	private void RoomScreen()
	{
		if (IsRoomLimitedInRanked)
		{
			LimitedOnlineFunctionalityScreen();
			return;
		}
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Groups);
		Permission permissionDataByFeature2 = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Multiplayer);
		bool item = KIDManager.CheckFeatureOptIn(EKIDFeatures.Multiplayer).hasOptedInPreviously;
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Groups) && KIDManager.HasPermissionToUseFeature(EKIDFeatures.Multiplayer) && item;
		screenText.Set("");
		string text = "";
		string text2 = "";
		if (flag)
		{
			text2 = "PRESS ENTER TO JOIN OR CREATE A CUSTOM ROOM WITH THE ENTERED CODE.";
			text = GetLocalisedText("ROOM_INTRO", text2, "Room Screen");
			screenText.Append(text.TrailingSpace());
		}
		text2 = "PRESS OPTION 1 TO DISCONNECT FROM THE CURRENT ROOM.";
		text = GetLocalisedText("ROOM_OPTION", text2, "Room Screen");
		TryColorOptionNumbers(text, out var colored, (1, RoomDisconnectColor));
		screenText.Append(colored.TrailingSpace());
		if (FriendshipGroupDetection.Instance.IsInParty)
		{
			if (FriendshipGroupDetection.Instance.IsPartyWithinCollider(friendJoinCollider))
			{
				text2 = "\nYOUR GROUP WILL TRAVEL WITH YOU.";
				text = GetLocalisedText("ROOM_GROUP_TRAVEL", text2, "Room Screen");
				screenText.Append(text.TrailingSpace());
			}
			else
			{
				text2 = "\n<color=red>YOU WILL LEAVE YOUR PARTY UNLESS YOU GATHER THEM HERE FIRST!</color> ";
				text = GetLocalisedText("ROOM_PARTY_WARNING", text2, "Room Screen");
				screenText.Append(text);
			}
		}
		text2 = "\n\nCURRENT ROOM:";
		text = GetLocalisedText("ROOM_TEXT_CURRENT_ROOM", text2, "Room Screen");
		screenText.Append(text.TrailingSpace());
		if (NetworkSystem.Instance.InRoom)
		{
			screenText.Append(GetVStumpRoomDisplayName(NetworkSystem.Instance.RoomName).TrailingSpace());
			if (NetworkSystem.Instance.SessionIsPrivate)
			{
				string text3 = GameMode.ActiveGameMode?.GameModeNameRoomLabel();
				if (!string.IsNullOrEmpty(text3))
				{
					screenText.Append(text3 ?? "");
				}
			}
			text2 = "\n\nPLAYERS IN ROOM:";
			text = GetLocalisedText("PLAYERS_IN_ROOM", text2, "Room Screen");
			screenText.Append(text.TrailingSpace());
			screenText.Append(NetworkSystem.Instance.RoomPlayerCount.ToString());
		}
		else
		{
			text2 = "-NOT IN ROOM-";
			text = GetLocalisedText("NOT_IN_ROOM", text2, "Room Screen");
			screenText.Append(text);
			text2 = "\n\nPLAYERS ONLINE:";
			text = GetLocalisedText("PLAYERS_ONLINE", text2, "Room Screen");
			screenText.Append(text.TrailingSpace());
			screenText.Append(HowManyMonke.ThisMany.ToString());
		}
		if (flag)
		{
			text2 = "\n\nROOM TO JOIN:";
			text = GetLocalisedText("ROOM_TO_JOIN", text2, "Room Screen");
			screenText.Append(text.TrailingSpace());
			screenText.Append(GetVStumpRoomDisplayName(roomToJoin));
			if (nameCheckRateLimited)
			{
				NameCheckRateLimitedMessage();
			}
			else if (roomFull)
			{
				text2 = "\n\nROOM FULL. JOIN ROOM FAILED.";
				text = GetLocalisedText("ROOM_FULL", text2, "Room Screen");
				screenText.Append(text);
			}
			else if (roomNotAllowed)
			{
				text2 = "\n\nCANNOT JOIN ROOM TYPE FROM HERE.";
				text = GetLocalisedText("ROOM_JOIN_NOT_ALLOWED", text2, "Room Screen");
				screenText.Append(text);
			}
		}
		else if (permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.PROHIBITED || permissionDataByFeature2.ManagedBy == Permission.ManagedByEnum.PROHIBITED)
		{
			RoomScreen_KIdProhibited();
		}
		else
		{
			RoomScreen_Permission();
		}
	}

	private void UpdateSnakeLayout()
	{
		float num = 1f;
		float b = 0.75f;
		TMP_FontAsset tMP_FontAsset = monospaceScreenFont;
		if (tMP_FontAsset != null && tMP_FontAsset.faceInfo.pointSize > 0f)
		{
			num = tMP_FontAsset.faceInfo.lineHeight / tMP_FontAsset.faceInfo.pointSize;
			if (tMP_FontAsset.characterLookupTable != null && tMP_FontAsset.characterLookupTable.TryGetValue(35u, out var value) && value.glyph != null)
			{
				b = value.glyph.metrics.horizontalAdvance / tMP_FontAsset.faceInfo.pointSize;
			}
		}
		float num2 = snakeScreenLineSpacing / 100f;
		float num3 = num + num2;
		float num4 = num * (float)snakeLineHeightPercent / 100f + num2;
		int num5 = Mathf.FloorToInt(15f * num3 / Mathf.Max(0.01f, num4) + 0.001f);
		snakeRows = Mathf.Max(5, num5 - 2);
		snakeVerticalStepScale = Mathf.Max(0.1f, num4 / Mathf.Max(0.01f, b));
		if (snakeGrid == null || snakeGrid.GetLength(0) != snakeRows)
		{
			snakeGrid = new char[snakeRows, 42];
		}
	}

	private float GetSnakeStepTime(Vector2Int direction)
	{
		if (direction.y == 0)
		{
			return snakeStepTime;
		}
		return snakeStepTime * snakeVerticalStepScale;
	}

	private bool TryEnterSnakeSecretCode(GorillaKeyboardBindings buttonPressed)
	{
		if (buttonPressed != SnakeSecretCode[snakeSecretCodeProgress])
		{
			snakeSecretCodeProgress = ((buttonPressed == SnakeSecretCode[0]) ? 1 : 0);
			return false;
		}
		snakeSecretCodeProgress++;
		if (snakeSecretCodeProgress < SnakeSecretCode.Length)
		{
			return false;
		}
		snakeSecretCodeProgress = 0;
		SwitchState(ComputerState.Snake);
		ResetSnakeGame();
		PlaySnakeSound(snakeOpenClip);
		return true;
	}

	private void ResetSnakeSecretCode()
	{
		snakeSecretCodeProgress = 0;
	}

	private void EndSnakeGame(bool crashed)
	{
		snakeGameState = SnakeGameState.GameOver;
		snakeLastScore = snakeScore;
		LoadSnakeBestScore();
		snakeNewBestScore = snakeScore > snakeBestScore;
		if (snakeNewBestScore)
		{
			snakeBestScore = snakeScore;
			PlayerPrefs.SetInt("snakeBestScore", snakeBestScore);
			PlayerPrefs.Save();
		}
		PlaySnakeGameOverSounds(crashed);
	}

	private void PlaySnakeSound(AudioClip clip)
	{
		if (!(clip == null))
		{
			if (snakeAudioSource == null)
			{
				snakeAudioSource = base.gameObject.AddComponent<AudioSource>();
				snakeAudioSource.playOnAwake = false;
				snakeAudioSource.spatialBlend = 0f;
			}
			snakeAudioSource.GTPlayOneShot(clip, snakeSoundVolume);
		}
	}

	private void PlaySnakeGameOverSounds(bool crashed)
	{
		StopSnakeGameOverSound();
		AudioClip clip = (snakeNewBestScore ? snakeNewBestClip : snakeGameOverClip);
		if (!crashed || snakeCrashClip == null)
		{
			PlaySnakeSound(clip);
			return;
		}
		PlaySnakeSound(snakeCrashClip);
		snakeGameOverSoundRoutine = StartCoroutine(PlaySnakeSoundAfter(clip, snakeCrashClip.length));
	}

	private IEnumerator PlaySnakeSoundAfter(AudioClip clip, float delay)
	{
		yield return new WaitForSeconds(delay);
		snakeGameOverSoundRoutine = null;
		if (currentState == ComputerState.Snake && snakeGameState == SnakeGameState.GameOver)
		{
			PlaySnakeSound(clip);
		}
	}

	private void StopSnakeGameOverSound()
	{
		if (snakeGameOverSoundRoutine != null)
		{
			StopCoroutine(snakeGameOverSoundRoutine);
			snakeGameOverSoundRoutine = null;
		}
	}

	private void LoadSnakeBestScore()
	{
		if (snakeBestScore < 0)
		{
			snakeBestScore = PlayerPrefs.GetInt("snakeBestScore", 0);
		}
	}

	private void ResetSnakeGame()
	{
		LoadSnakeBestScore();
		UpdateSnakeLayout();
		snakeBody.Clear();
		snakeBody.Add(new Vector2Int(21, snakeRows / 2));
		snakeDirection = Vector2Int.zero;
		snakeLastStepDirection = Vector2Int.zero;
		snakeScore = 0;
		snakeNewBestScore = false;
		snakeStepTime = snakeStartStepTime;
		snakeGameState = SnakeGameState.Waiting;
		SpawnSnakePellet();
	}

	private void ProcessSnakeState(GorillaKeyboardBindings buttonPressed)
	{
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.delete:
			ExitSnake();
			break;
		case GorillaKeyboardBindings.enter:
			if (snakeGameState == SnakeGameState.GameOver)
			{
				ResetSnakeGame();
			}
			break;
		case GorillaKeyboardBindings.W:
			TurnSnake(Vector2Int.down);
			break;
		case GorillaKeyboardBindings.S:
			TurnSnake(Vector2Int.up);
			break;
		case GorillaKeyboardBindings.A:
			TurnSnake(Vector2Int.left);
			break;
		case GorillaKeyboardBindings.D:
			TurnSnake(Vector2Int.right);
			break;
		}
	}

	private void ExitSnake()
	{
		StopSnakeGameOverSound();
		snakeGameState = SnakeGameState.Waiting;
		snakeBody.Clear();
		SwitchState(GetState(currentStateIndex));
	}

	private void TurnSnake(Vector2Int direction)
	{
		if (snakeGameState != SnakeGameState.GameOver && !(direction == -snakeLastStepDirection))
		{
			if (snakeGameState == SnakeGameState.Waiting)
			{
				snakeDirection = direction;
				snakeGameState = SnakeGameState.Playing;
				snakeLastStepTime = Time.time;
				PlaySnakeSound(snakeStartClip);
			}
			else
			{
				snakeDirection = direction;
			}
		}
	}

	private void UpdateSnake()
	{
		if (currentState == ComputerState.Snake)
		{
			if (snakeGameState == SnakeGameState.GameOver && snakeNewBestScore && IsSnakeNewBestVisible() != shownSnakeNewBest)
			{
				UpdateScreen();
			}
			else if (snakeGameState == SnakeGameState.Playing && !(Time.time < snakeLastStepTime + GetSnakeStepTime(snakeDirection)))
			{
				snakeLastStepTime = Time.time;
				StepSnake();
				UpdateScreen();
			}
		}
	}

	private void StepSnake()
	{
		Vector2Int vector2Int = snakeBody[0] + snakeDirection;
		snakeLastStepDirection = snakeDirection;
		bool flag = vector2Int.x < 0 || vector2Int.x >= 42 || vector2Int.y < 0 || vector2Int.y >= snakeRows;
		bool flag2 = vector2Int == snakePellet;
		int num = (flag2 ? snakeBody.Count : (snakeBody.Count - 1));
		bool flag3 = false;
		for (int i = 0; i < num; i++)
		{
			if (snakeBody[i] == vector2Int)
			{
				flag3 = true;
				break;
			}
		}
		if (flag || flag3)
		{
			EndSnakeGame(crashed: true);
			return;
		}
		snakeBody.Insert(0, vector2Int);
		if (!flag2)
		{
			snakeBody.RemoveAt(snakeBody.Count - 1);
			return;
		}
		snakeScore++;
		PlaySnakeSound(snakeEatClip);
		snakeStepTime = Mathf.Max(snakeMinStepTime, snakeStepTime * snakeStepTimeMultiplier);
		if (!SpawnSnakePellet())
		{
			EndSnakeGame(crashed: false);
		}
	}

	private bool SpawnSnakePellet()
	{
		int num = 42 * snakeRows - snakeBody.Count;
		if (num <= 0)
		{
			return false;
		}
		int num2 = UnityEngine.Random.Range(0, num);
		for (int i = 0; i < snakeRows; i++)
		{
			for (int j = 0; j < 42; j++)
			{
				Vector2Int item = new Vector2Int(j, i);
				if (!snakeBody.Contains(item) && num2-- == 0)
				{
					snakePellet = item;
					return true;
				}
			}
		}
		return false;
	}

	private void SnakeScreen()
	{
		if (snakeBody.Count == 0)
		{
			ResetSnakeGame();
		}
		for (int i = 0; i < snakeRows; i++)
		{
			for (int j = 0; j < 42; j++)
			{
				snakeGrid[i, j] = ' ';
			}
		}
		snakeGrid[snakePellet.y, snakePellet.x] = 'O';
		foreach (Vector2Int item in snakeBody)
		{
			if (item.x >= 0 && item.x < 42 && item.y >= 0 && item.y < snakeRows)
			{
				snakeGrid[item.y, item.x] = '■';
			}
		}
		snakeHighlightRow = -1;
		if (snakeGameState == SnakeGameState.GameOver)
		{
			WriteSnakeGridCentred(snakeRows / 2 - 2, "GAME OVER");
			if (snakeNewBestScore)
			{
				shownSnakeNewBest = IsSnakeNewBestVisible();
				int row = snakeRows / 2 - 1;
				if (shownSnakeNewBest)
				{
					snakeHighlightRow = row;
					snakeHighlightStart = WriteSnakeGridCentred(row, "NEW HIGH SCORE");
					snakeHighlightLength = "NEW HIGH SCORE".Length;
				}
				else
				{
					WriteSnakeGridCentred(row, new string(' ', "NEW HIGH SCORE".Length));
				}
			}
			WriteSnakeGridCentred(snakeRows / 2 + 1, "ENTER: RESTART");
			WriteSnakeGridCentred(snakeRows / 2 + 2, "DELETE: EXIT");
		}
		string text = " " + snakeScore + " ";
		snakeScreen.Clear();
		snakeScreen.Append("<line-height=").Append(snakeLineHeightPercent).Append("%>");
		string text2 = " BEST: " + snakeBestScore + "  LAST: " + snakeLastScore + " ";
		snakeScreen.Append('#');
		snakeScreen.Append(text2);
		snakeScreen.Append('#', Mathf.Max(1, 44 - text2.Length - text.Length - 2));
		snakeScreen.Append(text);
		snakeScreen.Append('#');
		for (int k = 0; k < snakeRows; k++)
		{
			snakeScreen.Append('\n');
			snakeScreen.Append('#');
			for (int l = 0; l < 42; l++)
			{
				if (k == snakeHighlightRow && l == snakeHighlightStart)
				{
					snakeScreen.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(SnakePelletColor)).Append('>');
					int num = 0;
					while (num < snakeHighlightLength && l < 42)
					{
						snakeScreen.Append(snakeGrid[k, l]);
						num++;
						l++;
					}
					snakeScreen.Append("</color>");
					l--;
				}
				else
				{
					char c = snakeGrid[k, l];
					if (c == 'O' && k == snakePellet.y && l == snakePellet.x)
					{
						AppendColored(snakeScreen, "O", SnakePelletColor);
					}
					else
					{
						snakeScreen.Append(c);
					}
				}
			}
			snakeScreen.Append('#');
		}
		snakeScreen.Append('\n');
		string text3 = ((snakeGameState == SnakeGameState.Waiting) ? " WASD: MOVE  DELETE: EXIT " : " DELETE: EXIT ");
		int num2 = (44 - text3.Length) / 2;
		snakeScreen.Append('#', num2);
		snakeScreen.Append(text3);
		snakeScreen.Append('#', 44 - num2 - text3.Length);
		screenText.Set(snakeScreen.ToString());
	}

	private int WriteSnakeGridCentred(int row, string text)
	{
		int num = Mathf.Max(0, (42 - text.Length) / 2);
		for (int i = 0; i < text.Length && num + i < 42; i++)
		{
			snakeGrid[row, num + i] = text[i];
		}
		return num;
	}

	private bool IsSnakeNewBestVisible()
	{
		float num = Mathf.Max(0.1f, snakeNewBestBlinkTime);
		return Time.time % (num * 2f) < num;
	}

	private int GetStartupFaceFrame()
	{
		float num = Mathf.Max(0.1f, startupFaceFrameTime);
		return Mathf.Min((int)(Time.time % GetStartupFaceCycleLength() / num), StartupFaceLookCycle.Length - 1);
	}

	private float GetStartupFaceCycleLength()
	{
		return Mathf.Max(0.1f, startupFaceFrameTime) * (float)StartupFaceLookCycle.Length + Mathf.Max(0f, startupFacePauseTime);
	}

	private char GetStartupFaceEye()
	{
		int num = (int)(Time.time / GetStartupFaceCycleLength());
		if (num == startupFaceEyeCycle)
		{
			return startupFaceEye;
		}
		startupFaceEyeCycle = num;
		startupFaceEye = 'O';
		float value = UnityEngine.Random.value;
		for (int i = 0; i < StartupFaceRareEyes.Length; i++)
		{
			if (value < startupFaceRareEyeChance * (float)(i + 1))
			{
				startupFaceEye = StartupFaceRareEyes[i];
				break;
			}
		}
		return startupFaceEye;
	}

	private bool IsStartupPressKeyVisible()
	{
		float num = Mathf.Max(0.1f, startupPressKeyFlashInterval);
		return Time.time % num >= Mathf.Clamp(startupPressKeyFlashOffTime, 0f, num);
	}

	private void UpdateStartupScreensaver()
	{
		if (currentState == ComputerState.Startup || currentState == ComputerState.Loading || currentState == ComputerState.NameWarning || currentState == ComputerState.Snake || screenText.IsFailed)
		{
			lastKeyPressTime = Time.time;
		}
		else if (!(Time.time - lastKeyPressTime < startupScreensaverTime))
		{
			SwitchState(ComputerState.Startup);
			UpdateScreen();
		}
	}

	private void UpdateStartupScreenAnimation()
	{
		if (currentState == ComputerState.Startup && !internetFailure && (GetStartupFaceFrame() != shownStartupFaceFrame || IsStartupPressKeyVisible() != shownStartupPressKey))
		{
			UpdateScreen();
		}
	}

	private void GetStartupScreenText(out string header, out string players, out string pressKey)
	{
		header = "GORILLA TAG - " + version.ToUpper();
		players = GetLocalisedText("STARTUP_PLAYERS_ONLINE", "{playersOnline} PLAYERS ONLINE", "Start Up").Trim().Replace("{playersOnline}", HowManyMonke.ThisMany.ToString());
		pressKey = GetLocalisedText("WARNING_SCREEN_CONFIRMATION", "PRESS ANY KEY TO CONTINUE", "Start Up").Trim();
	}

	private static int GetBorderedInnerWidth(string header, string players, string pressKey)
	{
		return Mathf.Max(40, header.Length, players.Length, pressKey.Length, StartupFace[0].Length);
	}

	private static string GetStartupPlayersLine(string players, int width)
	{
		int num = width - players.Length - " ".Length;
		DateTime now = DateTime.Now;
		for (int i = 0; i < StartupDateTimeFormats.Length; i++)
		{
			string text = now.ToString(StartupDateTimeFormats[i], CultureInfo.InvariantCulture);
			if (text.Length <= num)
			{
				return players + (" " + text).PadLeft(width - players.Length);
			}
		}
		return players.PadRight(width);
	}

	private string GetStartupRoomLine(int width)
	{
		string text;
		if (NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom)
		{
			text = GetLocalisedText("ROOM_TEXT_CURRENT_ROOM", "CURRENT ROOM:", "Start Up").Trim().TrailingSpace() + GetVStumpRoomDisplayName(NetworkSystem.Instance.RoomName);
			if (NetworkSystem.Instance.SessionIsPrivate)
			{
				string text2 = GameMode.ActiveGameMode?.GameModeNameRoomLabel();
				if (!string.IsNullOrEmpty(text2))
				{
					text = text + " " + text2;
				}
			}
		}
		else
		{
			text = GetLocalisedText("NOT_IN_ROOM", "-NOT IN ROOM-", "Start Up").Trim();
		}
		if (text.Length <= width)
		{
			return text.PadRight(width);
		}
		return text.Substring(0, width);
	}

	private void StartupScreen()
	{
		GetStartupScreenText(out var header, out var players, out var pressKey);
		int borderedInnerWidth = GetBorderedInnerWidth(header, players, pressKey);
		string value = new string('=', borderedInnerWidth + 4);
		borderedScreen.Clear();
		borderedScreen.Append(value);
		borderedLineCount = 1;
		AppendBorderedLine(header.PadRight(borderedInnerWidth));
		AppendBorderedLine(GetStartupPlayersLine(players, borderedInnerWidth));
		AppendBorderedLine(GetStartupRoomLine(borderedInnerWidth));
		AppendBorderedLine(string.Empty.PadRight(borderedInnerWidth));
		shownStartupFaceFrame = GetStartupFaceFrame();
		AppendBorderedFace(StartupFaceLookCycle[shownStartupFaceFrame].Replace('O', GetStartupFaceEye()), borderedInnerWidth);
		if (KIDManager.GetActiveAccountStatus() == AgeStatusType.DIGITALMINOR)
		{
			foreach (string item in WrapBorderedText(GetLocalisedText("STARTUP_MANAGED", "YOU ARE PLAYING ON A MANAGED ACCOUNT. SOME SETTINGS MAY BE DISABLED WITHOUT PARENT OR GUARDIAN APPROVAL", "Start Up").Trim(), borderedInnerWidth))
			{
				AppendBorderedLine(CentreBorderedText(item, borderedInnerWidth));
			}
			AppendBorderedLine(string.Empty.PadRight(borderedInnerWidth));
		}
		while (borderedLineCount < 13)
		{
			AppendBorderedLine(string.Empty.PadRight(borderedInnerWidth));
		}
		shownStartupPressKey = IsStartupPressKeyVisible();
		AppendBorderedLine(shownStartupPressKey ? CentreBorderedText(pressKey, borderedInnerWidth) : string.Empty.PadRight(borderedInnerWidth));
		borderedScreen.Append("\n");
		borderedScreen.Append(value);
		screenText.Set(borderedScreen.ToString());
	}

	private string BuildFailureScreen(string failMessage)
	{
		GetStartupScreenText(out var header, out var players, out var pressKey);
		int borderedInnerWidth = GetBorderedInnerWidth(header, players, pressKey);
		string value = new string('=', borderedInnerWidth + 4);
		borderedScreen.Clear();
		borderedScreen.Append(value);
		borderedLineCount = 1;
		string[] array = failMessage.Trim().Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			List<string> list = WrapBorderedText(array[i].Trim(), borderedInnerWidth);
			if (list.Count == 0)
			{
				AppendBorderedLine(string.Empty.PadRight(borderedInnerWidth));
			}
			foreach (string item in list)
			{
				AppendBorderedLine(item.PadRight(borderedInnerWidth));
			}
		}
		while (borderedLineCount < 15 - StartupFace.Length - 1)
		{
			AppendBorderedLine(string.Empty.PadRight(borderedInnerWidth));
		}
		AppendBorderedFace("       [ | (X) (X) | ]        ", borderedInnerWidth);
		borderedScreen.Append("\n");
		borderedScreen.Append(value);
		return borderedScreen.ToString();
	}

	private void AppendBorderedLine(string paddedContent)
	{
		borderedLineCount++;
		borderedScreen.Append("\n| ");
		borderedScreen.Append(paddedContent);
		borderedScreen.Append(" |");
	}

	private void AppendBorderedFace(string eyesLine, int innerWidth)
	{
		for (int i = 0; i < StartupFace.Length; i++)
		{
			string text = ((i == 4) ? eyesLine : StartupFace[i]);
			AppendBorderedLine(CentreBorderedText(text, innerWidth));
		}
	}

	private static string CentreBorderedText(string text, int width)
	{
		int num = Mathf.Max(0, (width - text.Length) / 2);
		return text.PadLeft(text.Length + num).PadRight(width);
	}

	private static List<string> WrapBorderedText(string text, int width)
	{
		List<string> list = new List<string>();
		string text2 = string.Empty;
		string[] array = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		foreach (string text3 in array)
		{
			if (text2.Length > 0 && text2.Length + 1 + text3.Length > width)
			{
				list.Add(text2);
				text2 = string.Empty;
			}
			text2 = ((text2.Length > 0) ? (text2 + " " + text3) : text3);
		}
		if (text2.Length > 0)
		{
			list.Add(text2);
		}
		return list;
	}

	private void InitializeSupportState()
	{
		displaySupport = false;
	}

	private void ProcessSupportState(GorillaKeyboardBindings buttonPressed)
	{
		if (buttonPressed == GorillaKeyboardBindings.enter)
		{
			displaySupport = true;
		}
	}

	private void SupportScreen()
	{
		screenText.Set("");
		if (displaySupport)
		{
			string text = PlayFabAuthenticator.instance.platform.ToString().ToUpper();
			string text2 = ((!(text == "PC")) ? text : "OCULUS PC");
			text = text2;
			text = GetLocalisedText(SupportPlatformKeys.TryGetValue(text, out var value) ? value : "UNKNOWN_PLATFORM", text, "Support Screen");
			string fallback = "SUPPORT";
			string localisedText = GetLocalisedText("SUPPORT_SCREEN_INTRO", fallback, "Support Screen");
			screenText.Append(localisedText);
			fallback = "\n\nPLAYER ID";
			localisedText = GetLocalisedText("SUPPORT_SCREEN_DETAILS_PLAYERID", fallback, "Support Screen");
			screenText.Append(localisedText + "  ");
			screenText.Append(PlayFabAuthenticator.instance.GetPlayFabPlayerId());
			fallback = "\nVERSION";
			localisedText = GetLocalisedText("SUPPORT_SCREEN_DETAILS_VERSION", fallback, "Support Screen");
			screenText.Append(localisedText + " ");
			screenText.Append(version.ToUpper());
			fallback = "\nPLATFORM";
			localisedText = GetLocalisedText("SUPPORT_SCREEN_DETAILS_PLATFORM", fallback, "Support Screen");
			screenText.Append(localisedText + " ");
			screenText.Append(text);
			fallback = "\nBUILD DATE";
			localisedText = GetLocalisedText("SUPPORT_SCREEN_DETAILS_BUILD_DATE", fallback, "Support Screen");
			screenText.Append(localisedText + " ");
			screenText.Append(buildDate);
			fallback = "\nSESSION ID";
			localisedText = GetLocalisedText("SUPPORT_SCREEN_DETAILS_MOTHERSHIP_SESSION_ID", fallback, "Support Screen");
			string sessionId = MothershipClientApiUnity.SessionId;
			string str = sessionId;
			int num = sessionId.LastIndexOf('-');
			if (num >= 0)
			{
				string text3 = sessionId.Substring(0, num);
				text2 = sessionId;
				int num2 = num + 1;
				str = text3 + "\n            " + text2.Substring(num2, text2.Length - num2);
			}
			screenText.Append(localisedText + " ");
			screenText.Append(str);
			if (KIDManager.KidEnabled)
			{
				fallback = "\nk-ID ACCOUNT TYPE:";
				localisedText = GetLocalisedText("SUPPORT_KID_ACCOUNT_TYPE", fallback, "Support Screen");
				screenText.Append(localisedText.TrailingSpace());
				screenText.Append(KIDManager.GetActiveAccountStatusNiceString().ToUpper());
			}
		}
		else
		{
			string fallback2 = "SUPPORT";
			string localisedText2 = GetLocalisedText("SUPPORT_SCREEN_INTRO", fallback2, "Support Screen");
			screenText.Append(localisedText2);
			fallback2 = "\n\nPRESS ENTER TO DISPLAY SUPPORT AND ACCOUNT INFORMATION";
			localisedText2 = GetLocalisedText("SUPPORT_SCREEN_INITIAL", fallback2, "Support Screen");
			screenText.Append(HighlightEnterKey(localisedText2));
			fallback2 = "\n\n\n\n<color=red>DO NOT SHARE ACCOUNT INFORMATION WITH ANYONE OTHER THAN ANOTHER AXIOM</color>";
			localisedText2 = GetLocalisedText("SUPPORT_SCREEN_INITIAL_WARNING", fallback2, "Support Screen");
			screenText.Append(localisedText2);
		}
	}

	private void InitializeTimeState()
	{
		BetterDayNightManager.instance.currentSetting = TimeSettings.Normal;
	}

	private void TimeScreen()
	{
		string fallback = "UPDATE TIME SETTINGS. (LOCALLY ONLY). \nPRESS OPTION 1 FOR NORMAL MODE. \nPRESS OPTION 2 FOR STATIC MODE. \nPRESS 1-10 TO CHANGE TIME OF DAY. \nCURRENT MODE: {currentSetting}.\nTIME OF DAY: {currentTimeOfDay}.\n";
		string localisedText = GetLocalisedText("TIME_SCREEN", fallback, "Support Screen");
		localisedText = localisedText.Replace("{currentSetting}", BetterDayNightManager.instance.currentSetting.ToString().ToUpper()).Replace("{currentTimeOfDay}", BetterDayNightManager.instance.currentTimeOfDay.ToUpper());
		screenText.Set(localisedText);
	}

	private void InitializeTroopState()
	{
		bool flag = false;
		troopToJoin = (troopName = PlayerPrefs.GetString("troopName", string.Empty));
		if (!rememberTroopQueueState)
		{
			bool num = PlayerPrefs.GetInt("troopQueueActive", 0) == 1;
			bool flag2 = currentQueue != "DEFAULT" && currentQueue != "COMPETITIVE" && currentQueue != "MINIGAMES";
			if (num || flag2)
			{
				currentQueue = "DEFAULT";
				PlayerPrefs.SetInt("troopQueueActive", 0);
				PlayerPrefs.SetString("currentQueue", currentQueue);
				PlayerPrefs.Save();
			}
		}
		troopQueueActive = PlayerPrefs.GetInt("troopQueueActive", 0) == 1;
		if (troopQueueActive && !IsValidTroopName(troopName))
		{
			troopQueueActive = false;
			PlayerPrefs.SetInt("troopQueueActive", troopQueueActive ? 1 : 0);
			currentQueue = "DEFAULT";
			PlayerPrefs.SetString("currentQueue", currentQueue);
			flag = true;
		}
		if (troopQueueActive)
		{
			StartCoroutine(HandleInitialTroopQueueState());
		}
		if (flag)
		{
			PlayerPrefs.Save();
		}
	}

	private IEnumerator HandleInitialTroopQueueState()
	{
		GTDev.Log("HandleInitialTroopQueueState()");
		while (!PlayFabCloudScriptAPI.IsEntityLoggedIn())
		{
			yield return null;
		}
		RequestTroopPopulation();
		while (currentTroopPopulation < 0)
		{
			yield return null;
		}
		if (currentTroopPopulation < 2)
		{
			GTDev.Log("Low population - starting in DEFAULT queue");
			JoinDefaultQueue();
		}
	}

	public void JoinTroop(string newTroopName)
	{
		if (IsValidTroopName(newTroopName))
		{
			currentTroopPopulation = -1;
			troopName = newTroopName;
			PlayerPrefs.SetString("troopName", troopName);
			if (troopQueueActive)
			{
				currentQueue = GetQueueNameForTroop(troopName);
				PlayerPrefs.SetString("currentQueue", currentQueue);
			}
			PlayerPrefs.Save();
			JoinTroopQueue();
		}
	}

	public void JoinTroopQueue()
	{
		if (IsValidTroopName(troopName))
		{
			currentTroopPopulation = -1;
			JoinQueue(GetQueueNameForTroop(troopName), isTroopQueue: true);
			RequestTroopPopulation(forceUpdate: true);
		}
	}

	private void RequestTroopPopulation(bool forceUpdate = false)
	{
		if (!PlayFabCloudScriptAPI.IsEntityLoggedIn() || !(!hasRequestedInitialTroopPopulation || forceUpdate) || nextPopulationCheckTime > Time.realtimeSinceStartup)
		{
			return;
		}
		nextPopulationCheckTime = Time.realtimeSinceStartup + troopPopulationCheckCooldown;
		hasRequestedInitialTroopPopulation = true;
		GorillaServer.Instance.ReturnQueueStats(new ReturnQueueStatsRequest
		{
			queueName = troopName
		}, delegate(ExecuteFunctionResult result)
		{
			GTDev.Log("Troop pop received");
			if (((JsonObject)result.FunctionResult).TryGetValue("PlayerCount", out var value))
			{
				currentTroopPopulation = int.Parse(value.ToString());
				if (currentComputerState == ComputerState.Queue)
				{
					UpdateScreen();
				}
			}
			else
			{
				currentTroopPopulation = 0;
			}
		}, delegate(PlayFabError error)
		{
			Debug.LogError($"Error requesting troop population: {error}");
			currentTroopPopulation = -1;
		});
	}

	public void LeaveTroop()
	{
		if (IsValidTroopName(troopName))
		{
			troopToJoin = troopName;
		}
		currentTroopPopulation = -1;
		troopName = string.Empty;
		PlayerPrefs.SetString("troopName", troopName);
		if (troopQueueActive)
		{
			JoinDefaultQueue();
		}
		PlayerPrefs.Save();
	}

	public string GetCurrentTroop()
	{
		if (troopQueueActive)
		{
			return troopName;
		}
		return currentQueue;
	}

	public int GetCurrentTroopPopulation()
	{
		if (troopQueueActive)
		{
			return currentTroopPopulation;
		}
		return -1;
	}

	private void ProcessTroopState(GorillaKeyboardBindings buttonPressed)
	{
		if (limitOnlineScreens)
		{
			return;
		}
		bool num = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Groups);
		bool flag = IsValidTroopName(troopName);
		if (num)
		{
			switch (buttonPressed)
			{
			case GorillaKeyboardBindings.option1:
				JoinTroopQueue();
				break;
			case GorillaKeyboardBindings.option2:
				JoinDefaultQueue();
				break;
			case GorillaKeyboardBindings.option4:
				LeaveTroop();
				break;
			case GorillaKeyboardBindings.enter:
				if (!flag)
				{
					CheckAutoBanListForTroopName(troopToJoin);
				}
				break;
			case GorillaKeyboardBindings.delete:
				if (!flag && troopToJoin.Length > 0)
				{
					troopToJoin = troopToJoin.Substring(0, troopToJoin.Length - 1);
				}
				break;
			default:
			{
				if (!flag && troopToJoin.Length < 12 && buttonPressed.TryGetTypedText(out var text))
				{
					troopToJoin += text;
				}
				break;
			}
			}
			return;
		}
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.option2:
			if (_currentScreenState != 0)
			{
				ProcessScreen_SetupKID();
			}
			else
			{
				RequestUpdatedPermissions();
			}
			break;
		case GorillaKeyboardBindings.option3:
			if (_currentScreenState == EKidScreenState.Show_OTP)
			{
				ProcessScreen_SetupKID();
			}
			break;
		case GorillaKeyboardBindings.option1:
			break;
		}
	}

	private void TroopScreen()
	{
		if (limitOnlineScreens)
		{
			LimitedOnlineFunctionalityScreen();
			return;
		}
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Groups);
		Permission permissionDataByFeature2 = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Multiplayer);
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Groups) && KIDManager.HasPermissionToUseFeature(EKIDFeatures.Multiplayer);
		bool flag2 = IsValidTroopName(troopName);
		screenText.Set(string.Empty);
		string text = "";
		string text2 = "";
		if (flag)
		{
			text = "PLAY WITH A PERSISTENT GROUP ACROSS MULTIPLE ROOMS.";
			text2 = GetLocalisedText("TROOP_SCREEN_INTRO", text, "Troop Screen");
			screenText.Set(text2);
			if (!flag2)
			{
				text = " PRESS ENTER TO JOIN OR CREATE A TROOP.";
				text2 = GetLocalisedText("TROOP_SCREEN_INSTRUCTIONS", text, "Troop Screen");
				screenText.Append(text2);
			}
		}
		text = "\n\nCURRENT TROOP: ";
		text2 = GetLocalisedText("TROOP_SCREEN_CURRENT_TROOP", text, "Troop Screen");
		screenText.Append(text2.TrailingSpace());
		if (flag2)
		{
			screenText.Append(troopName ?? "");
			if (flag)
			{
				bool flag3 = currentTroopPopulation > -1;
				if (troopQueueActive)
				{
					text = "\n  -IN TROOP QUEUE-";
					text2 = GetLocalisedText("TROOP_SCREEN_IN_QUEUE", text, "Troop Screen");
					screenText.Append(text2);
					if (flag3)
					{
						text = "\n\nPLAYERS IN TROOP: ";
						text2 = GetLocalisedText("TROOP_SCREEN_PLAYERS_IN_TROOP", text, "Troop Screen");
						screenText.Append(text2.TrailingSpace());
						screenText.Append(Mathf.Max(1, currentTroopPopulation).ToString());
					}
					text = "\n\nPRESS OPTION 2 FOR DEFAULT QUEUE.";
					text2 = GetLocalisedText("TROOP_SCREEN_DEFAULT_QUEUE", text, "Troop Screen");
					TryColorOptionNumbers(text2, out text2, (2, TroopDefaultQueueColor));
					screenText.Append(text2);
				}
				else
				{
					text = "\n  -IN {currentQueue} QUEUE-";
					text2 = GetLocalisedText("TROOP_SCREEN_CURRENT_QUEUE", text, "Troop Screen");
					string text3 = "DEFAULT_QUEUE";
					text3 = currentQueue switch
					{
						"MINIGAMES" => "MINIGAMES_QUEUE", 
						"COMPETITIVE" => "COMPETITIVE_QUEUE", 
						_ => "DEFAULT_QUEUE", 
					};
					text = currentQueue;
					string localisedText = GetLocalisedText(text3, text, "Troop Screen");
					text2 = text2.Replace("{currentQueue}", localisedText);
					screenText.Append(text2);
					if (flag3)
					{
						text = "\n\nPLAYERS IN TROOP: ";
						text2 = GetLocalisedText("TROOP_SCREEN_PLAYERS_IN_TROOP", text, "Troop Screen");
						screenText.Append(text2.TrailingSpace());
						screenText.Append(Mathf.Max(1, currentTroopPopulation).ToString());
					}
					text = "\n\nPRESS OPTION 1 FOR TROOP QUEUE.";
					text2 = GetLocalisedText("TROOP_SCREEN_TROOP_QUEUE", text, "Troop Screen");
					TryColorOptionNumbers(text2, out text2, (1, TroopQueueColor));
					screenText.Append(text2);
				}
				text = "\nPRESS OPTION 4 TO LEAVE YOUR TROOP.";
				text2 = GetLocalisedText("TROOP_SCREEN_LEAVE", text, "Troop Screen");
				TryColorOptionNumbers(text2, out text2, (4, TroopLeaveColor));
				screenText.Append(text2);
			}
		}
		else
		{
			text = "-NOT IN TROOP-";
			text2 = GetLocalisedText("TROOP_SCREEN_NOT_IN_TROOP", text, "Troop Screen");
			screenText.Append(text2);
		}
		if (flag)
		{
			if (!flag2)
			{
				text = "\n\nTROOP TO JOIN: ";
				text2 = GetLocalisedText("TROOP_SCREEN_JOIN_TROOP", text, "Troop Screen");
				screenText.Append(text2.TrailingSpace());
				screenText.Append(troopToJoin);
				if (nameCheckRateLimited)
				{
					NameCheckRateLimitedMessage();
				}
			}
		}
		else if (permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.PROHIBITED || permissionDataByFeature2.ManagedBy == Permission.ManagedByEnum.PROHIBITED)
		{
			TroopScreen_KIdProhibited();
		}
		else
		{
			TroopScreen_Permission();
		}
	}

	private void InitializeTurnState()
	{
		GorillaSnapTurn.LoadSettingsFromPlayerPrefs();
	}

	private void ProcessTurnState(GorillaKeyboardBindings buttonPressed)
	{
		if (buttonPressed < GorillaKeyboardBindings.up)
		{
			GorillaSnapTurn.UpdateAndSaveTurnFactor((int)buttonPressed);
			return;
		}
		string text = string.Empty;
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.option1:
			text = "SNAP";
			break;
		case GorillaKeyboardBindings.option2:
			text = "SMOOTH";
			break;
		case GorillaKeyboardBindings.option3:
			text = "NONE";
			break;
		}
		if (text.Length > 0)
		{
			GorillaSnapTurn.UpdateAndSaveTurnType(text);
		}
	}

	private void TurnScreen()
	{
		string fallback = "PRESS OPTION 1 TO USE SNAP TURN. PRESS OPTION 2 TO USE SMOOTH TURN. PRESS OPTION 3 TO USE NO ARTIFICIAL TURNING.";
		string text = "";
		string localisedText = GetLocalisedText("TURN_SCREEN", fallback, "Turn Screen");
		text = ((!TryColorSentences(localisedText, out var colored, TurnSnapColor, TurnSmoothColor, TurnNoTurnColor)) ? (text + localisedText.TrailingSpace()) : (text + colored + "\n\n"));
		fallback = "PRESS THE NUMBER KEYS TO CHOOSE A TURNING SPEED.";
		localisedText = GetLocalisedText("TURN_SCREEN_TURNING_SPEED", fallback, "Turn Screen");
		text = text + HighlightNumberKeys(localisedText) + "\n";
		fallback = "\n CURRENT TURN TYPE: ";
		localisedText = GetLocalisedText("TURN_SCREEN_TURN_TYPE", fallback, "Turn Screen");
		text += localisedText.TrailingSpace();
		string key = "TURN_TYPE_NO_TURN";
		switch (GorillaSnapTurn.CachedSnapTurnRef.turnType)
		{
		case "SNAP":
			key = "TURN_TYPE_SNAP_TURN";
			break;
		case "SMOOTH":
			key = "TURN_TYPE_SMOOTH_TURN";
			break;
		case "NONE":
			key = "TURN_TYPE_NO_TURN";
			break;
		default:
			Debug.LogError("[LOCALIZATION::GORILLA_COMPUTER::TURN] Could not match [" + GorillaSnapTurn.CachedSnapTurnRef.turnType + "] to any case. Defaulting to NO_TURN");
			break;
		}
		localisedText = GetLocalisedText(key, GorillaSnapTurn.CachedSnapTurnRef.turnType, "Turn Screen");
		text += localisedText;
		fallback = "\nCURRENT TURN SPEED: ";
		localisedText = GetLocalisedText("TURN_SCREEN_TURN_SPEED", fallback, "Turn Screen");
		text += localisedText.TrailingSpace();
		text += GorillaSnapTurn.CachedSnapTurnRef.turnFactor;
		screenText.Set(text);
	}

	private void InitializeVisualsState()
	{
		disableParticles = PlayerPrefs.GetString("disableParticles", "FALSE") == "TRUE";
		GorillaTagger.Instance.ShowCosmeticParticles(!disableParticles);
		instrumentVolume = PlayerPrefs.GetFloat("instrumentVolume", 0.1f);
		iobtMode = SubscriptionManager.GetSubscriptionSettingBool(SubscriptionManager.SubscriptionFeatures.IOBT);
	}

	private void ProcessVisualsState(GorillaKeyboardBindings buttonPressed)
	{
		if (buttonPressed < GorillaKeyboardBindings.up)
		{
			instrumentVolume = (float)buttonPressed / 50f;
			PlayerPrefs.SetFloat("instrumentVolume", instrumentVolume);
			PlayerPrefs.Save();
			return;
		}
		switch (buttonPressed)
		{
		case GorillaKeyboardBindings.option1:
			disableParticles = false;
			PlayerPrefs.SetString("disableParticles", "FALSE");
			PlayerPrefs.Save();
			GorillaTagger.Instance.ShowCosmeticParticles(!disableParticles);
			break;
		case GorillaKeyboardBindings.option2:
			disableParticles = true;
			PlayerPrefs.SetString("disableParticles", "TRUE");
			PlayerPrefs.Save();
			GorillaTagger.Instance.ShowCosmeticParticles(!disableParticles);
			break;
		case GorillaKeyboardBindings.option3:
			if (SubscriptionManager.IsSubscriptionFeatureAvailable(SubscriptionManager.SubscriptionFeatures.IOBT) && GorillaIK.playerIK != null && GorillaIK.playerIK.CanUpdateIK())
			{
				SubscriptionManager.SetSubscriptionSettingValue(SubscriptionManager.SubscriptionFeatures.IOBT, (!iobtMode) ? 1 : 0);
				iobtMode = SubscriptionManager.GetSubscriptionSettingBool(SubscriptionManager.SubscriptionFeatures.IOBT);
				GorillaIK.playerIK.DelayedUpdateIK(iobtMode);
			}
			break;
		case GorillaKeyboardBindings.option4:
			if (SubscriptionManager.IsLocalSubscribed())
			{
				ToggleGoldNameTag();
			}
			break;
		case GorillaKeyboardBindings.Q:
			if (SubscriptionManager.IsSubscriptionFeatureAvailable(SubscriptionManager.SubscriptionFeatures.IOBT) && GorillaIK.playerIK != null)
			{
				GorillaIK.playerIK.CalibrateLeanOffset();
			}
			break;
		case GorillaKeyboardBindings.delete:
			if (SubscriptionManager.IsSubscriptionFeatureAvailable(SubscriptionManager.SubscriptionFeatures.IOBT) && GorillaIK.playerIK != null)
			{
				GorillaIK.playerIK.ResetLeanOffset();
			}
			break;
		}
	}

	private static void ToggleGoldNameTag()
	{
		bool subscriptionSettingBool = SubscriptionManager.GetSubscriptionSettingBool(SubscriptionManager.SubscriptionFeatures.GoldenName);
		SubscriptionManager.SetSubscriptionSettingValue(SubscriptionManager.SubscriptionFeatures.GoldenName, (!subscriptionSettingBool) ? 1 : 0);
		VRRig.LocalRig.OnSubscriptionData();
		if (GorillaScoreboardTotalUpdater.instance != null)
		{
			GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
		}
	}

	private void VisualsScreen()
	{
		string fallback = "UPDATE ITEMS SETTINGS.";
		string localisedText = GetLocalisedText("VISUALS_SCREEN_INTRO", fallback, "Visuals Screen");
		screenText.Set(localisedText.TrailingSpace());
		fallback = "PRESS OPTION 1 TO ENABLE ITEM PARTICLES. PRESS OPTION 2 TO DISABLE PARTICLES. PRESS 1-10 TO CHANGE INSTRUMENT VOLUME FOR OTHER PLAYERS.";
		localisedText = GetLocalisedText("VISUALS_SCREEN_OPTIONS", fallback, "Visuals Screen");
		TryColorSentences(localisedText, out var colored, ItemsParticlesOnColor, ItemsParticlesOffColor, Color.clear);
		screenText.Append(HighlightVolumeRange(colored));
		fallback = "\n\nITEM PARTICLES ON:";
		localisedText = GetLocalisedText("VISUALS_SCREEN_CURRENT", fallback, "Visuals Screen");
		screenText.Append(localisedText.TrailingSpace());
		string text = (disableParticles ? "FALSE" : "TRUE");
		localisedText = GetLocalisedText(text, text, "Visuals Screen");
		screenText.Append(localisedText);
		fallback = "\nINSTRUMENT VOLUME:";
		localisedText = GetLocalisedText("VISUALS_SCREEN_VOLUME", fallback, "Visuals Screen");
		screenText.Append(localisedText.TrailingSpace());
		screenText.Append(Mathf.CeilToInt(instrumentVolume * 50f).ToString());
		if (SubscriptionManager.IsSubscriptionFeatureAvailable(SubscriptionManager.SubscriptionFeatures.IOBT))
		{
			IobtSettingsScreen();
		}
		if (SubscriptionManager.IsLocalSubscribed())
		{
			fallback = "PRESS OPTION 4 TO TOGGLE GOLD NAME TAG:";
			localisedText = GetLocalisedText("VISUALS_SCREEN_GOLD_NAME_TAG", fallback, "Visuals Screen");
			TryColorOptionNumbers(localisedText, out localisedText, (4, ItemsGoldNameTagColor));
			screenText.Append("\n\n" + localisedText.TrailingSpace());
			text = (SubscriptionManager.GetSubscriptionSettingBool(SubscriptionManager.SubscriptionFeatures.GoldenName) ? "TRUE" : "FALSE");
			screenText.Append(GetLocalisedText(text, text, "Visuals Screen"));
		}
	}

	private void IobtSettingsScreen()
	{
		TryColorOptionNumbers("\n\nPRESS OPTION 3 TO TOGGLE IOBT:", out var colored, (3, ItemsIobtColor));
		screenText.Append(colored.TrailingSpace());
		iobtMode = SubscriptionManager.GetSubscriptionSettingBool(SubscriptionManager.SubscriptionFeatures.IOBT);
		string obj = (iobtMode ? "TRUE" : "FALSE");
		colored = GetLocalisedText(obj, obj, "Visuals Screen");
		screenText.Append(colored);
		colored = HighlightPhrase(HighlightPhrase("\n-PRESS Q TO CALIBRATE / DELETE TO RESET", s_calibrateKeyPhrase), s_resetKeyPhrase);
		screenText.Append(colored);
	}

	private void InitializeVoiceState()
	{
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Voice_Chat);
		string @string = PlayerPrefs.GetString("voiceChatOn", "");
		string defaultValue = "FALSE";
		switch (permissionDataByFeature.ManagedBy)
		{
		case Permission.ManagedByEnum.PLAYER:
			defaultValue = ((!string.IsNullOrEmpty(@string)) ? @string : (permissionDataByFeature.Enabled ? "TRUE" : "FALSE"));
			break;
		case Permission.ManagedByEnum.GUARDIAN:
			if (permissionDataByFeature.Enabled)
			{
				@string = (string.IsNullOrEmpty(@string) ? "FALSE" : @string);
				defaultValue = @string;
			}
			else
			{
				defaultValue = "FALSE";
			}
			break;
		case Permission.ManagedByEnum.PROHIBITED:
			defaultValue = "FALSE";
			break;
		}
		voiceChatOn = PlayerPrefs.GetString("voiceChatOn", defaultValue);
	}

	private void MuteAllVoices()
	{
		voiceChatOn = "OFF";
		PlayerPrefs.SetString("voiceChatOn", voiceChatOn);
		PlayerPrefs.Save();
	}

	private void ProcessVoiceState(GorillaKeyboardBindings buttonPressed)
	{
		if (KIDManager.HasPermissionToUseFeature(EKIDFeatures.Voice_Chat))
		{
			switch (buttonPressed)
			{
			case GorillaKeyboardBindings.option1:
				SetVoice(setting: true);
				break;
			case GorillaKeyboardBindings.option2:
				SetVoice(setting: false);
				break;
			case GorillaKeyboardBindings.option4:
				MuteAllVoices();
				break;
			}
		}
		else
		{
			switch (buttonPressed)
			{
			case GorillaKeyboardBindings.option2:
				if (_currentScreenState != 0)
				{
					ProcessScreen_SetupKID();
				}
				else
				{
					RequestUpdatedPermissions();
				}
				break;
			case GorillaKeyboardBindings.option3:
				if (_currentScreenState != EKidScreenState.Show_OTP)
				{
					return;
				}
				ProcessScreen_SetupKID();
				break;
			}
		}
		RigContainer.RefreshAllRigVoices();
	}

	private void VoiceScreen()
	{
		Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Voice_Chat);
		if (KIDManager.HasPermissionToUseFeature(EKIDFeatures.Voice_Chat))
		{
			string fallback = "CHOOSE WHICH TYPE OF VOICE YOU WANT TO HEAR AND SPEAK.";
			string localisedText = GetLocalisedText("VOICE_CHAT_SCREEN_INTRO", fallback, "Voice Screen");
			screenText.Set(localisedText);
			fallback = "\nPRESS OPTION 1 = HUMAN VOICES.\nPRESS OPTION 2 = MONKE VOICES.";
			localisedText = GetLocalisedText("VOICE_CHAT_SCREEN_OPTIONS", fallback, "Voice Screen");
			TryColorSentences(localisedText, out var colored, VoiceHumanColor, VoiceMonkeColor);
			screenText.Append(colored);
			fallback = "PRESS OPTION 4 = MUTE ALL VOICES.";
			localisedText = GetLocalisedText("VOICE_CHAT_SCREEN_MUTE_ALL", fallback, "Voice Screen");
			TryColorSentences(localisedText, out colored, VoiceMuteAllColor);
			screenText.Append("\n" + colored.Trim());
			fallback = "\n\nVOICE TYPE:";
			localisedText = GetLocalisedText("VOICE_CHAT_SCREEN_CURRENT", fallback, "Voice Screen");
			screenText.Append(localisedText.TrailingSpace());
			string key = ((voiceChatOn == "TRUE") ? "VOICE_OPTION_HUMAN" : ((voiceChatOn == "FALSE") ? "VOICE_OPTION_MONKE" : "VOICE_OPTION_OFF"));
			fallback = ((voiceChatOn == "TRUE") ? "HUMAN" : ((voiceChatOn == "FALSE") ? "MONKE" : "OFF"));
			localisedText = GetLocalisedText(key, fallback, "Voice Screen");
			screenText.Append(localisedText);
		}
		else if (permissionDataByFeature.ManagedBy == Permission.ManagedByEnum.PROHIBITED)
		{
			VoiceScreen_KIdProhibited();
		}
		else
		{
			VoiceScreen_Permission();
		}
	}

	private void InitializeKIdState()
	{
		KIDManager.RegisterSessionUpdateCallback_AnyPermission(OnSessionUpdate_GorillaComputer);
	}

	private void UpdateKidState()
	{
		_currentScreenState = EKidScreenState.Ready;
	}

	private void RequestUpdatedPermissions()
	{
		if (KIDManager.KidEnabledAndReady && !_waitingForUpdatedSession && !(Time.realtimeSinceStartup < _nextUpdateAttemptTime))
		{
			_waitingForUpdatedSession = true;
			UpdateSession();
		}
	}

	private async void UpdateSession()
	{
		_nextUpdateAttemptTime = Time.realtimeSinceStartup + _updateAttemptCooldown;
		await KIDManager.UpdateSession();
		_waitingForUpdatedSession = false;
	}

	private void OnSessionUpdate_GorillaComputer()
	{
		UpdateKidState();
		UpdateScreen();
	}

	private void ProcessScreen_SetupKID()
	{
		if (!KIDManager.KidEnabledAndReady)
		{
			Debug.LogError("[KID] Unable to start k-ID Flow. Kid is disabled");
		}
	}

	private bool GuardianConsentMessage(string setupKIDButtonName, string featureDescription)
	{
		string fallback = "PARENT/GUARDIAN PERMISSION REQUIRED TO ";
		string localisedText = GetLocalisedText("KID_PERMISSION_NEEDED", fallback, "K-ID Permission Screen");
		screenText.Append(localisedText);
		screenText.Append(featureDescription + "!");
		if (_waitingForUpdatedSession)
		{
			fallback = "\n\nWAITING FOR PARENT/GUARDIAN CONSENT!";
			localisedText = GetLocalisedText("KID_WAITING_PERMISSION", fallback, "K-ID Permission Screen");
			screenText.Append(localisedText);
			return true;
		}
		if (Time.realtimeSinceStartup >= _nextUpdateAttemptTime)
		{
			fallback = "\n\nPRESS OPTION 2 TO REFRESH PERMISSIONS!";
			localisedText = GetLocalisedText("KID_REFRESH_PERMISSIONS", fallback, "K-ID Permission Screen");
			screenText.Append(localisedText);
		}
		else
		{
			fallback = "CHECK AGAIN IN {time} SECONDS!";
			localisedText = GetLocalisedText("KID_CHECK_AGAIN_COOLDOWN", fallback, "K-ID Permission Screen");
			localisedText = localisedText.Replace("{time}", ((int)(_nextUpdateAttemptTime - Time.realtimeSinceStartup)).ToString());
			screenText.Append(localisedText);
		}
		return false;
	}

	private void ProhibitedMessage(string verb)
	{
		_ = "\n\nYOU ARE NOT ALLOWED TO " + verb + " IN YOUR JURISDICTION.";
		string localisedText = GetLocalisedText("KID_PROHIBITED_MESSAGE", "SET CUSTOM NICKNAMES", "Name Screen");
		localisedText = localisedText.Replace("{verb}", verb);
		screenText.Append(localisedText);
	}

	private void RoomScreen_Permission()
	{
		if (!KIDManager.KidEnabled)
		{
			string fallback = "YOU CANNOT USE THE PRIVATE ROOM FEATURE RIGHT NOW";
			string localisedText = GetLocalisedText("ROOM_SCREEN_DISABLED", fallback, "Room Screen");
			screenText.Set(localisedText);
		}
		else
		{
			screenText.Set("");
			string fallback2 = "CREATE OR JOIN PRIVATE ROOMS";
			string localisedText2 = GetLocalisedText("ROOM_SCREEN_KID_PROHIBITED_VERB", fallback2, "Room Screen");
			GuardianConsentMessage("OPTION 3", localisedText2);
		}
	}

	private void RoomScreen_KIdProhibited()
	{
		string fallback = "CREATE OR JOIN PRIVATE ROOMS";
		string localisedText = GetLocalisedText("ROOM_SCREEN_KID_PROHIBITED_VERB", fallback, "Room Screen");
		ProhibitedMessage(localisedText);
	}

	private void VoiceScreen_Permission()
	{
		string fallback = "VOICE TYPE: \"MONKE\"\n\n";
		string localisedText = GetLocalisedText("VOICE_SCREEN_KID_CURRENT_VOICE", fallback, "Voice Screen");
		screenText.Set(localisedText);
		if (!KIDManager.KidEnabled)
		{
			fallback = "YOU CANNOT USE THE HUMAN VOICE TYPE FEATURE RIGHT NOW";
			localisedText = GetLocalisedText("VOICE_SCREEN_DISABLED", fallback, "Voice Screen");
			screenText.Append(localisedText);
		}
		else
		{
			fallback = "ENABLE HUMAN VOICE CHAT";
			localisedText = GetLocalisedText("MIC_SCREEN_GUARDIAN_FEATURE_DESC", fallback, "Voice Screen");
			GuardianConsentMessage("OPTION 3", localisedText);
		}
	}

	private void VoiceScreen_KIdProhibited()
	{
		string fallback = "USE THE VOICE CHAT";
		string localisedText = GetLocalisedText("VOICE_SCREEN_KID_PROHIBITED_VERB", fallback, "Voice Screen");
		ProhibitedMessage(localisedText);
	}

	private void MicScreen_Permission()
	{
		screenText.Set("");
		string fallback = "ENABLE HUMAN VOICE CHAT";
		string localisedText = GetLocalisedText("MIC_SCREEN_GUARDIAN_FEATURE_DESC", fallback, "Mic Screen");
		GuardianConsentMessage("OPTION 3", localisedText);
	}

	private void MicScreen_KIdProhibited()
	{
		VoiceScreen_KIdProhibited();
	}

	private void NameScreen_Permission()
	{
		if (!KIDManager.KidEnabled)
		{
			string fallback = "YOU CANNOT USE THE CUSTOM NICKNAME FEATURE RIGHT NOW";
			string localisedText = GetLocalisedText("NAME_SCREEN_DISABLED", fallback, "K-ID Permission Screen");
			screenText.Append(localisedText);
		}
		else
		{
			screenText.Set("");
			string localisedText2 = GetLocalisedText("NAME_SCREEN_KID_PROHIBITED_VERB", "SET CUSTOM NICKNAMES", "Name Screen");
			GuardianConsentMessage("OPTION 3", localisedText2);
		}
	}

	private void NameScreen_KIdProhibited()
	{
		string localisedText = GetLocalisedText("NAME_SCREEN_KID_PROHIBITED_VERB", "SET CUSTOM NICKNAMES", "Name Screen");
		ProhibitedMessage(localisedText);
	}

	private void OnKIDSessionUpdated_CustomNicknames(bool showCustomNames, Permission.ManagedByEnum managedBy)
	{
		bool flag = (showCustomNames || managedBy == Permission.ManagedByEnum.PLAYER) && managedBy != Permission.ManagedByEnum.PROHIBITED;
		SetComputerSettingsBySafety(!flag, new ComputerState[1] { ComputerState.Name }, shouldHide: false);
		int @int = PlayerPrefs.GetInt(NameTagPlayerPref, -1);
		bool flag2 = @int > 0;
		switch (managedBy)
		{
		case Permission.ManagedByEnum.GUARDIAN:
			NametagsEnabled = showCustomNames && (flag2 || @int == -1);
			break;
		case Permission.ManagedByEnum.PLAYER:
			if (showCustomNames)
			{
				NametagsEnabled = @int == -1 || flag2;
			}
			else
			{
				NametagsEnabled = @int != -1 && flag2;
			}
			break;
		case Permission.ManagedByEnum.PROHIBITED:
			NametagsEnabled = false;
			break;
		}
		if (NametagsEnabled)
		{
			NetworkSystem.Instance.SetMyNickName(savedName);
		}
		onNametagSettingChangedAction?.Invoke(NametagsEnabled);
	}

	private void TroopScreen_Permission()
	{
		screenText.Set("");
		if (!KIDManager.KidEnabled)
		{
			string localisedText = GetLocalisedText("TROOP_SCREEN_DISABLED", "YOU CANNOT USE THE TROOPS FEATURE RIGHT NOW", "Name Screen");
			screenText.Append(localisedText);
		}
		else
		{
			string localisedText2 = GetLocalisedText("TROOP_SCREEN_KID_DESC", "JOIN TROOPS", "Name Screen");
			GuardianConsentMessage("OPTION 3", localisedText2);
		}
	}

	private void TroopScreen_KIdProhibited()
	{
		string localisedText = GetLocalisedText("TROOP_SCREEN_KID_PROHIBITED_VERB", "CREATE OR JOIN TROOPS", "Name Screen");
		ProhibitedMessage(localisedText);
	}

	private void ProcessKIdState(GorillaKeyboardBindings buttonPressed)
	{
		if (buttonPressed == GorillaKeyboardBindings.option1 && _currentScreenState == EKidScreenState.Ready)
		{
			RequestUpdatedPermissions();
		}
	}

	private void KIdScreen()
	{
		if (KIDManager.KidEnabledAndReady)
		{
			if (!KIDManager.HasSession)
			{
				GuardianConsentMessage("OPTION 3", "");
			}
			else
			{
				KIdScreen_DisplayPermissions();
			}
		}
	}

	private void KIdScreen_DisplayPermissions()
	{
		AgeStatusType activeAccountStatus = KIDManager.GetActiveAccountStatus();
		string text = ((!KIDManager.InitialisationSuccessful) ? "NOT READY" : activeAccountStatus.ToString());
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("k-ID Account Status:\t" + text);
		if (activeAccountStatus == (AgeStatusType)0)
		{
			stringBuilder.AppendLine("\nPress 'OPTION 1' to get permissions!");
			screenText.Set(stringBuilder.ToString());
			return;
		}
		if (_waitingForUpdatedSession)
		{
			stringBuilder.AppendLine("\nWAITING FOR PARENT/GUARDIAN CONSENT!");
			screenText.Set(stringBuilder.ToString());
			return;
		}
		stringBuilder.AppendLine("\nPermissions:");
		List<Permission> allPermissionsData = KIDManager.GetAllPermissionsData();
		int count = allPermissionsData.Count;
		int num = 1;
		for (int i = 0; i < count; i++)
		{
			if (_interestedPermissionNames.Contains(allPermissionsData[i].Name))
			{
				string text2 = (allPermissionsData[i].Enabled ? "<color=#85ffa5>" : "<color=\"RED\">");
				stringBuilder.AppendLine("[" + num + "] " + text2 + allPermissionsData[i].Name + "</color>");
				num++;
			}
		}
		stringBuilder.AppendLine("\nTO REFRESH PERMISSIONS PRESS OPTION 1!");
		screenText.Set(stringBuilder.ToString());
	}

	private string GetLocalisedLanguageScreen()
	{
		return GetLanguageScreenLocalisation();
	}

	private void GetLangaugesList(ref string langStr)
	{
		_languagesDisplaySB.Clear();
		int maxLength = 12;
		int num = 3;
		int num2 = 0;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<int, Locale> allBinding in LocalisationManager.GetAllBindings())
		{
			num2++;
			string text = LocalisationManager.LocaleToFriendlyString(allBinding.Value).ToUpper();
			string value = HighlightNumber(allBinding.Key.ToString()) + " " + text;
			stringBuilder.Append(value);
			int remainingChars = GetRemainingChars(text, maxLength);
			stringBuilder.Append(' ', remainingChars);
			if (num2 >= num)
			{
				_languagesDisplaySB.AppendLine(stringBuilder.ToString());
				stringBuilder.Clear();
				num2 = 0;
			}
		}
		_languagesDisplaySB.AppendLine(stringBuilder.ToString());
		langStr = langStr + _languagesDisplaySB.ToString() + "\n";
	}

	private int GetRemainingChars(string value, int maxLength)
	{
		int num = 0;
		if (value == "日本語")
		{
			return (LocalisationManager.CurrentLanguage.Identifier.Code == "ja") ? 7 : 7;
		}
		return Mathf.Clamp(maxLength - value.Length, 0, maxLength);
	}

	private string GetLanguageScreenLocalisation()
	{
		string text = "";
		string localisedText = GetLocalisedText("LANG_SCREEN_TITLE", "CHOOSE YOUR LANGUAGE\n", "Language Screen");
		text += localisedText;
		GetLangaugesList(ref text);
		localisedText = GetLocalisedText("LANG_SCREEN_INSTRUCTIONS", "PRESS NUMBER KEYS TO CHOOSE A LANGUAGE\n", "Language Screen");
		text += HighlightNumberKeys(localisedText);
		localisedText = GetLocalisedText("LANG_SCREEN_CURRENT_LANGUAGE", "CURRENT LANGUAGE: ", "Language Screen");
		return text + localisedText.TrailingSpace() + LocalisationManager.LocaleToFriendlyString().ToUpper();
	}

	private static string GetLocalisedText(string key, string fallback, string context)
	{
		Locale currentLanguage = LocalisationManager.CurrentLanguage;
		if (s_localisedTextCacheLocale != currentLanguage)
		{
			s_localisedTextCache.Clear();
			s_localisedTextCacheLocale = currentLanguage;
		}
		if (key != null && s_localisedTextCache.TryGetValue(key, out var value))
		{
			return value;
		}
		if (!LocalisationManager.TryGetKeyForCurrentLocale(key, out var result, fallback))
		{
			return result;
		}
		if (key != null)
		{
			s_localisedTextCache[key] = result;
		}
		return result;
	}

	private void InitializeLanguageState()
	{
		_previousLocalisationSetting = LocalisationManager.CurrentLanguage;
		LocalisationManager.RegisterOnLanguageChanged(OnLanguageChanged);
	}

	private void LanguageScreen()
	{
		screenText.Set(GetLocalisedLanguageScreen());
	}

	private void ProcessLanguageState(GorillaKeyboardBindings buttonPressed)
	{
		if (buttonPressed.FromNumberBindingToInt(out var result) && LocalisationManager.TryGetLocaleBinding(result, out var loc))
		{
			LocalisationManager.Instance.OnLanguageButtonPressed(loc.Identifier.Code, saveLanguage: true);
			RefreshFunctionNames();
		}
	}

	private void OnLanguageChanged()
	{
		if (_previousLocalisationSetting == LocalisationManager.CurrentLanguage)
		{
			Debug.Log("[LOCALISATION::GORILLA_COMPUTER] Language changed, but no different to previous setting [" + _previousLocalisationSetting.ToString() + "]");
			return;
		}
		_previousLocalisationSetting = LocalisationManager.CurrentLanguage;
		RefreshFunctionNames();
	}

	private void RefreshFunctionNames()
	{
		FunctionNames.Clear();
		functionListTextDirty = true;
		FunctionsCount = OrderList.Count;
		highestCharacterCount = int.MinValue;
		OrderList.ForEach(delegate(StateOrderItem s)
		{
			string text = s.GetName();
			if (text.Length > highestCharacterCount)
			{
				highestCharacterCount = text.Length;
			}
			FunctionNames.Add(text);
		});
		for (int i = 0; i < FunctionsCount; i++)
		{
			int num = highestCharacterCount - FunctionNames[i].Length;
			for (int j = 0; j < num; j++)
			{
				FunctionNames[i] += " ";
			}
		}
	}
}

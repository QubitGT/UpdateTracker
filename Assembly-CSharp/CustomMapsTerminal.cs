using System;
using System.Collections.Generic;
using GorillaNetworking;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Modio.Mods;
using TMPro;
using UnityEngine;

[AddComponentMenu("GorillaTag/Virtual Stump/Custom Maps Terminal")]
public class CustomMapsTerminal : MonoBehaviour
{
	public enum ScreenType
	{
		Invalid = -1,
		TerminalControlPrompt,
		AvailableMods,
		InstalledMods,
		FavoriteMods,
		SubscribedMods,
		SearchMods,
		ModDetails,
		FeaturedMods,
		DevMaps
	}

	[SerializeField]
	private CustomMapsAccessScreen controlAccessScreen;

	[SerializeField]
	private CustomMapsAccessScreen detailsAccessScreen;

	[SerializeField]
	private CustomMapsListScreen modListScreen;

	[SerializeField]
	private CustomMapsDetailsScreen modDetailsScreen;

	[SerializeField]
	private CustomMapsDisplayScreen modDisplayScreen;

	[SerializeField]
	private CustomMapsSearchScreen modSearchScreen;

	[SerializeField]
	private CustomMapsFeaturedScreen modFeaturedScreen;

	[SerializeField]
	private CustomMapsTerminalControlButton terminalControlButton;

	[SerializeField]
	private TMP_Text terminalControllerLabelText;

	[SerializeField]
	private TMP_Text terminalControllerText;

	[Header("Tent terminal")]
	[SerializeField]
	private bool isTentTerminal;

	[SerializeField]
	private GameObject[] tentVisuals;

	[SerializeField]
	private CustomMapsDevTerminal devScreen;

	[SerializeField]
	private CustomMapsScreenButton devButton;

	public const int NO_DRIVER_ID = -2;

	private static CustomMapsTerminal instance;

	private static bool hasInstance;

	private static readonly List<CustomMapsTerminal> enabledTerminals = new List<CustomMapsTerminal>();

	private bool devButtonShowsDevPage;

	private bool showingConnecting;

	private bool? tentVisible;

	private static bool openDevScreenOnControl;

	private static Mod atriumMap;

	private static long localModDetailsID = -1L;

	private static long cachedModDetailsID = -1L;

	private static int localDriverID = -1;

	private static int cachedLocalPlayerID = -1;

	private static ScreenType localCurrentScreen = ScreenType.Invalid;

	private static ScreenType cachedCurrentScreen = ScreenType.Invalid;

	private static ScreenType previousScreen = ScreenType.Invalid;

	private const float MaxRoomSwitchWaitSeconds = 15f;

	private const string ConnectingText = "CONNECTING TO YOUR ROOM...";

	private static float roomSwitchStartTime = -1f;

	public static int LocalPlayerID => NetworkSystem.Instance.LocalPlayer.ActorNumber;

	public static long LocalModDetailsID => localModDetailsID;

	public static int CurrentScreen => (int)localCurrentScreen;

	public static ScreenType PreviousScreen => previousScreen;

	public static bool IsDriver => localDriverID == LocalPlayerID;

	public static bool IsLocalOnly
	{
		get
		{
			if (NetworkSystem.Instance.InRoom)
			{
				if (!NetworkSystem.Instance.SessionIsPrivate)
				{
					return !ZoneManagement.IsInZone(GTZone.customMaps);
				}
				return false;
			}
			return true;
		}
	}

	public bool IsTentTerminal => isTentTerminal;

	private static bool HasFeaturedScreen
	{
		get
		{
			if (hasInstance)
			{
				return instance.modFeaturedScreen != null;
			}
			return false;
		}
	}

	private static ScreenType DefaultBrowseScreen
	{
		get
		{
			if (!HasFeaturedScreen)
			{
				return ScreenType.AvailableMods;
			}
			return ScreenType.FeaturedMods;
		}
	}

	private static bool? TentTerminalAllowed()
	{
		NetworkSystem networkSystem = NetworkSystem.Instance;
		if (networkSystem == null)
		{
			return false;
		}
		if (networkSystem.InRoom)
		{
			return networkSystem.SessionIsPrivate;
		}
		if (networkSystem.netState == NetSystemState.Connecting || networkSystem.netState == NetSystemState.Disconnecting || CustomMapManager.WaitingForRoomJoin)
		{
			return null;
		}
		return false;
	}

	private static bool IsSwitchingRooms()
	{
		if (!CustomMapManager.WaitingForRoomJoin && (!(NetworkSystem.Instance != null) || !NetworkSystem.Instance.InRoom || !VirtualStumpSerializer.IsWaitingForRoomInit()))
		{
			roomSwitchStartTime = -1f;
			return false;
		}
		if (roomSwitchStartTime < 0f)
		{
			roomSwitchStartTime = Time.unscaledTime;
		}
		return Time.unscaledTime - roomSwitchStartTime < 15f;
	}

	private static void ShowFeaturedScreenOrGallery()
	{
		if (!hasInstance)
		{
			return;
		}
		if (HasFeaturedScreen)
		{
			instance.modListScreen.Hide();
			instance.modFeaturedScreen.Show();
			return;
		}
		if (localCurrentScreen == ScreenType.FeaturedMods)
		{
			localCurrentScreen = ScreenType.AvailableMods;
		}
		if (previousScreen == ScreenType.FeaturedMods)
		{
			previousScreen = ScreenType.AvailableMods;
		}
		instance.modListScreen.Show();
	}

	private void Awake()
	{
		if (!hasInstance || !isTentTerminal)
		{
			instance = this;
			hasInstance = true;
		}
	}

	private void Start()
	{
		if (localCurrentScreen == ScreenType.Invalid)
		{
			localDriverID = -2;
			localCurrentScreen = ScreenType.TerminalControlPrompt;
			previousScreen = ScreenType.TerminalControlPrompt;
		}
		controlAccessScreen.Show();
		detailsAccessScreen.Show();
		modListScreen.Hide();
		modFeaturedScreen?.Hide();
		modDetailsScreen.Hide();
		if (devScreen != null)
		{
			devScreen.Hide();
		}
		ModIOManager.OnModIOLoggedIn.AddListener(OnModIOLoggedIn);
		ModIOManager.OnModIOLoggedOut.AddListener(OnModIOLoggedOut);
		NetworkSystem.Instance.OnMultiplayerStarted += new Action(OnJoinedRoom);
		NetworkSystem.Instance.OnReturnedToSinglePlayer += new Action(OnReturnedToSinglePlayer);
		if (isTentTerminal)
		{
			NetworkSystem.Instance.OnJoinedRoomEvent += new Action(UpdateTentVisibility);
			NetworkSystem.Instance.OnReturnedToSinglePlayer += new Action(UpdateTentVisibility);
			if (instance == this)
			{
				SyncWithSharedState();
			}
		}
	}

	private void OnEnable()
	{
		if (!enabledTerminals.Contains(this))
		{
			enabledTerminals.Add(this);
		}
		if (isTentTerminal)
		{
			BecomeActiveTerminal();
			UpdateTentVisibility();
		}
	}

	private void OnDisable()
	{
		enabledTerminals.Remove(this);
		if (isTentTerminal)
		{
			atriumMap = null;
		}
		if (hasInstance && !(instance != this))
		{
			if (isTentTerminal)
			{
				ReleaseLocalTerminalControl();
			}
			if (enabledTerminals.Count > 0)
			{
				enabledTerminals[enabledTerminals.Count - 1].BecomeActiveTerminal();
			}
		}
	}

	private void LateUpdate()
	{
		RefreshConnectingMessage();
		if (isTentTerminal)
		{
			UpdateTentVisibility();
			if (hasInstance && !(instance != this))
			{
				RefreshDevButton();
			}
		}
	}

	private void RefreshConnectingMessage()
	{
		bool flag = localDriverID == -2 && localCurrentScreen == ScreenType.TerminalControlPrompt;
		bool flag2 = flag && IsSwitchingRooms();
		if (flag2)
		{
			controlAccessScreen.DisplayError("CONNECTING TO YOUR ROOM...");
		}
		else if (showingConnecting && flag)
		{
			controlAccessScreen.Reset();
			controlAccessScreen.Show();
		}
		showingConnecting = flag2;
	}

	private void UpdateTentVisibility()
	{
		if (!isTentTerminal)
		{
			return;
		}
		bool? flag = TentTerminalAllowed();
		if (!flag.HasValue && tentVisible.HasValue)
		{
			return;
		}
		bool valueOrDefault = flag.GetValueOrDefault();
		if (valueOrDefault == tentVisible)
		{
			return;
		}
		tentVisible = valueOrDefault;
		if (tentVisuals != null)
		{
			GameObject[] array = tentVisuals;
			foreach (GameObject gameObject in array)
			{
				if (gameObject != null && gameObject.activeSelf != valueOrDefault)
				{
					gameObject.SetActive(valueOrDefault);
				}
			}
		}
		if (!valueOrDefault && hasInstance && instance == this)
		{
			ReleaseLocalTerminalControl();
		}
	}

	private void RefreshDevButton()
	{
		if (!(devButton == null))
		{
			bool flag = !IsLocked();
			bool flag2 = devScreen != null && devScreen.IsAvailable && (IsDriver || (flag && !IsSwitchingRooms())) && localCurrentScreen != ScreenType.ModDetails;
			if (devButton.gameObject.activeSelf != flag2)
			{
				devButton.gameObject.SetActive(flag2);
			}
			bool flag3 = localCurrentScreen == ScreenType.DevMaps;
			if (flag2 && flag3 != devButtonShowsDevPage)
			{
				devButtonShowsDevPage = flag3;
				devButton.SetButtonActive(flag3);
			}
		}
	}

	private void OnDestroy()
	{
		ModIOManager.OnModIOLoggedIn.RemoveListener(OnModIOLoggedIn);
		ModIOManager.OnModIOLoggedOut.RemoveListener(OnModIOLoggedOut);
		NetworkSystem.Instance.OnMultiplayerStarted -= new Action(OnJoinedRoom);
		NetworkSystem.Instance.OnReturnedToSinglePlayer -= new Action(OnReturnedToSinglePlayer);
		if (isTentTerminal)
		{
			NetworkSystem.Instance.OnJoinedRoomEvent -= new Action(UpdateTentVisibility);
			NetworkSystem.Instance.OnReturnedToSinglePlayer -= new Action(UpdateTentVisibility);
		}
	}

	public static void ShowDetailsScreen(Mod mod)
	{
		previousScreen = localCurrentScreen;
		localCurrentScreen = ScreenType.ModDetails;
		localModDetailsID = mod.Id;
		if (!instance.isTentTerminal)
		{
			atriumMap = mod;
		}
		HideDevScreen();
		instance.modListScreen.Hide();
		instance.modFeaturedScreen?.Hide();
		instance.controlAccessScreen.Hide();
		instance.detailsAccessScreen.Hide();
		instance.modDetailsScreen.Show();
		instance.modDetailsScreen.SetModProfile(mod);
		instance.modDisplayScreen.Show();
		instance.modDisplayScreen.SetModProfile(mod);
		instance.modSearchScreen.Hide();
		SendTerminalStatus();
	}

	public static void ReturnFromDetailsScreen()
	{
		HideDevScreen();
		if (!instance.isTentTerminal)
		{
			atriumMap = null;
		}
		ScreenType screenType = previousScreen;
		if (screenType == ScreenType.ModDetails || screenType == ScreenType.Invalid || screenType == ScreenType.TerminalControlPrompt || screenType == ScreenType.DevMaps)
		{
			localCurrentScreen = DefaultBrowseScreen;
			previousScreen = DefaultBrowseScreen;
		}
		else
		{
			localCurrentScreen = previousScreen;
		}
		switch (localCurrentScreen)
		{
		case ScreenType.TerminalControlPrompt:
			instance.modListScreen.Hide();
			instance.modFeaturedScreen?.Hide();
			instance.modDetailsScreen.Hide();
			instance.modDisplayScreen.Hide();
			instance.modSearchScreen.Hide();
			instance.controlAccessScreen.Show();
			instance.detailsAccessScreen.Show();
			break;
		case ScreenType.FeaturedMods:
			ShowFeaturedScreenOrGallery();
			instance.modSearchScreen.Hide();
			instance.modDetailsScreen.Hide();
			instance.modDisplayScreen.Hide();
			instance.controlAccessScreen.Hide();
			instance.detailsAccessScreen.Show();
			break;
		case ScreenType.AvailableMods:
		case ScreenType.InstalledMods:
		case ScreenType.FavoriteMods:
		case ScreenType.SubscribedMods:
			instance.modListScreen.Show();
			instance.modFeaturedScreen?.Hide();
			instance.modSearchScreen.Hide();
			instance.modDetailsScreen.Hide();
			instance.modDisplayScreen.Hide();
			instance.controlAccessScreen.Hide();
			instance.detailsAccessScreen.Show();
			break;
		case ScreenType.SearchMods:
			instance.modListScreen.Hide();
			instance.modFeaturedScreen?.Hide();
			instance.modSearchScreen.ReturnFromDetailsScreen();
			instance.modDetailsScreen.Hide();
			instance.modDisplayScreen.Hide();
			instance.controlAccessScreen.Hide();
			instance.detailsAccessScreen.Show();
			break;
		}
		SendTerminalStatus();
	}

	public static void ShowListScreen()
	{
		if (hasInstance)
		{
			previousScreen = localCurrentScreen;
			localCurrentScreen = ScreenType.AvailableMods;
			HideDevScreen();
			instance.modFeaturedScreen?.Hide();
			instance.modSearchScreen.Hide();
			instance.modDetailsScreen.Hide();
			instance.modDisplayScreen.Hide();
			instance.controlAccessScreen.Hide();
			instance.detailsAccessScreen.SetDetailsScreenForDriver();
			instance.detailsAccessScreen.Show();
			instance.modListScreen.SwapListDisplay(CustomMapsListScreen.ListScreenState.AvailableMods);
			instance.modListScreen.Show();
			SendTerminalStatus();
		}
	}

	public static void ShowFeaturedScreen()
	{
		if (hasInstance)
		{
			previousScreen = localCurrentScreen;
			localCurrentScreen = DefaultBrowseScreen;
			HideDevScreen();
			instance.modListScreen.Hide();
			instance.modSearchScreen.Hide();
			instance.modDetailsScreen.Hide();
			instance.modDisplayScreen.Hide();
			instance.controlAccessScreen.Hide();
			instance.detailsAccessScreen.SetDetailsScreenForDriver();
			instance.detailsAccessScreen.Show();
			ShowFeaturedScreenOrGallery();
			SendTerminalStatus();
		}
	}

	public static void ShowSearchScreen()
	{
		previousScreen = localCurrentScreen;
		localCurrentScreen = ScreenType.SearchMods;
		HideDevScreen();
		instance.modListScreen.Hide();
		instance.modFeaturedScreen?.Hide();
		instance.controlAccessScreen.Hide();
		instance.detailsAccessScreen.SetDetailsScreenForDriver();
		instance.detailsAccessScreen.Show();
		instance.modDetailsScreen.Hide();
		instance.modDisplayScreen.Hide();
		instance.modSearchScreen.Show();
		SendTerminalStatus();
	}

	public static void ReturnFromSearchScreen()
	{
		if (hasInstance)
		{
			localCurrentScreen = ScreenType.AvailableMods;
			previousScreen = ScreenType.AvailableMods;
			HideDevScreen();
			instance.modFeaturedScreen?.Hide();
			instance.modSearchScreen.Hide();
			instance.modDetailsScreen.Hide();
			instance.modDisplayScreen.Hide();
			instance.controlAccessScreen.Hide();
			instance.detailsAccessScreen.SetDetailsScreenForDriver();
			instance.detailsAccessScreen.Show();
			instance.modListScreen.SwapListDisplay(CustomMapsListScreen.ListScreenState.AvailableMods);
			instance.modListScreen.Show();
			SendTerminalStatus();
		}
	}

	public static void ShowDevScreen()
	{
		if (hasInstance && instance.isTentTerminal && !(instance.devScreen == null) && instance.devScreen.IsAvailable && IsLocked() && IsDriver)
		{
			previousScreen = localCurrentScreen;
			localCurrentScreen = ScreenType.DevMaps;
			instance.modListScreen.Hide();
			instance.modFeaturedScreen?.Hide();
			instance.modSearchScreen.Hide();
			instance.modDetailsScreen.Hide();
			instance.modDisplayScreen.Hide();
			instance.controlAccessScreen.Hide();
			instance.detailsAccessScreen.Hide();
			instance.devScreen.Show();
			SendTerminalStatus();
		}
	}

	public static void ToggleDevScreen()
	{
		if (!hasInstance)
		{
			return;
		}
		if (!IsLocked())
		{
			if (!IsSwitchingRooms())
			{
				openDevScreenOnControl = true;
				instance.HandleTerminalControlButtonPressed();
			}
		}
		else if (localCurrentScreen == ScreenType.DevMaps)
		{
			ShowFeaturedScreen();
		}
		else
		{
			ShowDevScreen();
		}
	}

	public static void OnDevScreenUnavailable()
	{
		if (hasInstance && localCurrentScreen == ScreenType.DevMaps && IsDriver)
		{
			ShowFeaturedScreen();
		}
	}

	private static void HideDevScreen()
	{
		if (hasInstance && instance.devScreen != null)
		{
			instance.devScreen.Hide();
		}
	}

	public static void SendTerminalStatus()
	{
		if (hasInstance)
		{
			VirtualStumpSerializer.Instance.SendTerminalStatus();
		}
	}

	public static void ResetTerminalControl()
	{
		localDriverID = -2;
		instance.terminalControlButton.UnlockTerminalControl();
		ShowTerminalControlScreen();
	}

	public static void HandleTerminalControlStatusChangeRequest(bool lockedStatus, int playerID)
	{
		if (lockedStatus && playerID == -2)
		{
			return;
		}
		if (localDriverID == -2)
		{
			if (!lockedStatus)
			{
				return;
			}
		}
		else if (localDriverID != playerID)
		{
			return;
		}
		SetTerminalControlStatus(lockedStatus, playerID, sendRPC: true);
	}

	public static void SetTerminalControlStatus(bool isLocked, int driverID = -2, bool sendRPC = false)
	{
		GTDev.Log($"[CustomMapsTerminal::SetTerminalControlStatus] isLocked: {isLocked} | driverID: {driverID} | playerId {LocalPlayerID} | sendRPC: {sendRPC}");
		if (isLocked)
		{
			localDriverID = driverID;
			instance.terminalControlButton.LockTerminalControl();
			if (IsDriver)
			{
				HideTerminalControlScreens();
				if (openDevScreenOnControl)
				{
					openDevScreenOnControl = false;
					ShowDevScreen();
				}
			}
			else
			{
				openDevScreenOnControl = false;
				ShowTerminalControlScreen();
			}
		}
		else
		{
			openDevScreenOnControl = false;
			localDriverID = -2;
			instance.terminalControlButton.UnlockTerminalControl();
			ShowTerminalControlScreen();
		}
		if (sendRPC && !IsLocalOnly && NetworkSystem.Instance.IsMasterClient)
		{
			VirtualStumpSerializer.Instance.SetTerminalControlStatus(isLocked, localDriverID);
		}
	}

	public static void UpdateFromDriver(int currentScreen, long modDetailsID, int driverID)
	{
		if (!hasInstance)
		{
			return;
		}
		localDriverID = driverID;
		cachedModDetailsID = modDetailsID;
		localModDetailsID = modDetailsID;
		cachedCurrentScreen = (ScreenType)currentScreen;
		localCurrentScreen = (ScreenType)currentScreen;
		Debug.Log($"[CustomMapsTerminal::UpdateFromDriver] currentScreen {localCurrentScreen} modDetailsID {localModDetailsID}");
		if (localDriverID != -2)
		{
			RefreshDriverNickName();
		}
		switch (localCurrentScreen)
		{
		case ScreenType.TerminalControlPrompt:
		case ScreenType.AvailableMods:
		case ScreenType.InstalledMods:
		case ScreenType.FavoriteMods:
		case ScreenType.SubscribedMods:
		case ScreenType.SearchMods:
		case ScreenType.FeaturedMods:
		case ScreenType.DevMaps:
			ShowTerminalControlScreen();
			break;
		case ScreenType.ModDetails:
			ShowTerminalControlScreen();
			if (localModDetailsID > 0)
			{
				instance.detailsAccessScreen.Hide();
				instance.modDisplayScreen.Show();
				instance.modDisplayScreen.RetrieveModFromModIO(localModDetailsID);
			}
			break;
		}
	}

	private void UpdateControlScreenForDriver()
	{
		GTDev.Log($"[CustomMapsTerminal::UpdateScreenToMatchStatus] driverID: {localDriverID} " + $"| currentScreen: {localCurrentScreen} " + $"| previousScreen: {previousScreen} ");
		if (localCurrentScreen == ScreenType.DevMaps && (devScreen == null || !devScreen.IsAvailable))
		{
			localCurrentScreen = DefaultBrowseScreen;
		}
		if (localCurrentScreen != ScreenType.DevMaps && devScreen != null)
		{
			devScreen.Hide();
		}
		switch (localCurrentScreen)
		{
		case ScreenType.TerminalControlPrompt:
			break;
		case ScreenType.DevMaps:
			controlAccessScreen.Hide();
			detailsAccessScreen.Hide();
			modSearchScreen.Hide();
			modListScreen.Hide();
			modFeaturedScreen?.Hide();
			modDetailsScreen.Hide();
			modDisplayScreen.Hide();
			devScreen.Show();
			break;
		case ScreenType.FeaturedMods:
			controlAccessScreen.Hide();
			modSearchScreen.Hide();
			detailsAccessScreen.SetDetailsScreenForDriver();
			detailsAccessScreen.Show();
			ShowFeaturedScreenOrGallery();
			modDetailsScreen.Hide();
			modDisplayScreen.Hide();
			break;
		case ScreenType.AvailableMods:
		case ScreenType.InstalledMods:
		case ScreenType.FavoriteMods:
		case ScreenType.SubscribedMods:
			controlAccessScreen.Hide();
			modSearchScreen.Hide();
			detailsAccessScreen.SetDetailsScreenForDriver();
			detailsAccessScreen.Show();
			modFeaturedScreen?.Hide();
			modListScreen.Show();
			modDetailsScreen.Hide();
			modDisplayScreen.Hide();
			break;
		case ScreenType.ModDetails:
			controlAccessScreen.Hide();
			modSearchScreen.Hide();
			detailsAccessScreen.Hide();
			modListScreen.Hide();
			modFeaturedScreen?.Hide();
			modDetailsScreen.Show();
			modDetailsScreen.RetrieveModFromModIO(localModDetailsID);
			modDisplayScreen.Show();
			modDisplayScreen.RetrieveModFromModIO(localModDetailsID);
			break;
		case ScreenType.SearchMods:
			controlAccessScreen.Hide();
			modSearchScreen.Show();
			detailsAccessScreen.SetDetailsScreenForDriver();
			detailsAccessScreen.Show();
			modListScreen.Hide();
			modFeaturedScreen?.Hide();
			modDetailsScreen.Hide();
			modDisplayScreen.Hide();
			break;
		}
	}

	private void ValidateLocalStatus()
	{
		if (localDriverID != -2)
		{
			if (CustomMapLoader.IsMapLoaded())
			{
				localCurrentScreen = ScreenType.ModDetails;
				localModDetailsID = CustomMapLoader.LoadedMapModId;
				SendTerminalStatus();
			}
			else if (CustomMapManager.IsLoading())
			{
				localCurrentScreen = ScreenType.ModDetails;
				localModDetailsID = CustomMapManager.LoadingMapId;
				SendTerminalStatus();
			}
			else if (CustomMapManager.GetRoomMapId() != ModId.Null)
			{
				localCurrentScreen = ScreenType.ModDetails;
				localModDetailsID = CustomMapManager.GetRoomMapId()._id;
				SendTerminalStatus();
			}
		}
	}

	private void OnModIOLoggedIn()
	{
	}

	private void OnModIOLoggedOut()
	{
		if (localCurrentScreen == ScreenType.SubscribedMods)
		{
			if (modListScreen.isActiveAndEnabled)
			{
				modListScreen.SwapListDisplay(CustomMapsListScreen.ListScreenState.AvailableMods);
			}
			else
			{
				localCurrentScreen = ScreenType.AvailableMods;
			}
		}
		if (previousScreen == ScreenType.SubscribedMods)
		{
			previousScreen = ScreenType.AvailableMods;
		}
	}

	private void BecomeActiveTerminal()
	{
		if (!hasInstance || !(instance == this))
		{
			instance = this;
			hasInstance = true;
			if (localCurrentScreen != ScreenType.Invalid)
			{
				SyncWithSharedState();
			}
		}
	}

	private void SyncWithSharedState()
	{
		if (IsLocalOnly || localDriverID == -2)
		{
			ResetTerminalControl();
			return;
		}
		terminalControlButton.LockTerminalControl();
		if (IsDriver)
		{
			UpdateControlScreenForDriver();
		}
		else
		{
			UpdateFromDriver((int)localCurrentScreen, localModDetailsID, localDriverID);
		}
	}

	public void HandleTerminalControlButtonPressed()
	{
		if (!IsDriver && IsSwitchingRooms())
		{
			return;
		}
		BecomeActiveTerminal();
		if (!IsLocalOnly)
		{
			if (localDriverID == -2 || IsDriver)
			{
				if (VirtualStumpSerializer.Instance.HasAuthority)
				{
					HandleTerminalControlStatusChangeRequest(!terminalControlButton.IsLocked, LocalPlayerID);
				}
				else
				{
					VirtualStumpSerializer.Instance.RequestTerminalControlStatusChange(!terminalControlButton.IsLocked);
				}
			}
		}
		else
		{
			SetTerminalControlStatus(!terminalControlButton.IsLocked, LocalPlayerID);
		}
	}

	public static void ReleaseLocalTerminalControl()
	{
		if (!hasInstance || localDriverID == -2 || !IsDriver)
		{
			return;
		}
		if (!IsLocalOnly)
		{
			if (!(VirtualStumpSerializer.Instance == null))
			{
				if (VirtualStumpSerializer.Instance.HasAuthority)
				{
					HandleTerminalControlStatusChangeRequest(lockedStatus: false, LocalPlayerID);
				}
				else
				{
					VirtualStumpSerializer.Instance.RequestTerminalControlStatusChange(lockedStatus: false);
				}
			}
		}
		else
		{
			SetTerminalControlStatus(isLocked: false, LocalPlayerID);
		}
	}

	private static void ShowTerminalControlScreen()
	{
		if (hasInstance)
		{
			if (localDriverID == -2)
			{
				instance.controlAccessScreen.Reset();
				instance.detailsAccessScreen.Reset();
			}
			else
			{
				instance.controlAccessScreen.SetDriverName();
				instance.detailsAccessScreen.SetDriverName();
			}
			HideDevScreen();
			instance.modListScreen.Hide();
			instance.modFeaturedScreen?.Hide();
			instance.modDetailsScreen.Hide();
			instance.modDisplayScreen.Hide();
			instance.controlAccessScreen.Show();
			instance.detailsAccessScreen.Show();
			instance.modSearchScreen.Hide();
			previousScreen = localCurrentScreen;
			localCurrentScreen = ScreenType.TerminalControlPrompt;
		}
	}

	private static void HideTerminalControlScreens()
	{
		if (!hasInstance || localCurrentScreen != 0)
		{
			return;
		}
		long mapId;
		if (previousScreen > ScreenType.TerminalControlPrompt)
		{
			localCurrentScreen = previousScreen;
			if ((localCurrentScreen == ScreenType.SubscribedMods || localCurrentScreen == ScreenType.FavoriteMods) && !ModIOManager.IsLoggedIn())
			{
				localCurrentScreen = ScreenType.AvailableMods;
			}
		}
		else if (CustomMapLoader.IsMapLoaded() || CustomMapManager.IsLoading() || CustomMapManager.GetRoomMapId() != ModId.Null)
		{
			localCurrentScreen = ScreenType.ModDetails;
		}
		else if (instance.isTentTerminal && TryTakeAtriumMap(out mapId))
		{
			localCurrentScreen = ScreenType.ModDetails;
			localModDetailsID = mapId;
		}
		else
		{
			localCurrentScreen = DefaultBrowseScreen;
		}
		instance.UpdateControlScreenForDriver();
	}

	private static bool TryTakeAtriumMap(out long mapId)
	{
		mapId = 0L;
		Mod mod = atriumMap;
		atriumMap = null;
		if (mod == null || mod.File == null || mod.File.State != ModFileState.Installed)
		{
			return false;
		}
		mapId = mod.Id;
		return mapId > 0;
	}

	public static void RequestDriverNickNameRefresh()
	{
		if (hasInstance && IsDriver)
		{
			RefreshDriverNickName();
			VirtualStumpSerializer.Instance.RefreshDriverNickName();
		}
	}

	public static void RefreshDriverNickName()
	{
		if (!hasInstance)
		{
			return;
		}
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags);
		instance.terminalControllerLabelText.gameObject.SetActive(value: true);
		if (NetworkSystem.Instance.InRoom)
		{
			NetPlayer netPlayerByID = NetworkSystem.Instance.GetNetPlayerByID(localDriverID);
			instance.terminalControllerText.text = netPlayerByID.DefaultName;
			if (GorillaComputer.instance.NametagsEnabled && flag)
			{
				RigContainer playerRig;
				if (netPlayerByID.IsLocal)
				{
					instance.terminalControllerText.text = netPlayerByID.NickName;
				}
				else if (VRRigCache.Instance.TryGetVrrig(netPlayerByID, out playerRig))
				{
					instance.terminalControllerText.text = playerRig.Rig.playerNameVisible;
				}
			}
		}
		else
		{
			instance.terminalControllerText.text = ((GorillaComputer.instance.NametagsEnabled && flag) ? NetworkSystem.Instance.LocalPlayer.NickName : NetworkSystem.Instance.LocalPlayer.DefaultName);
		}
		instance.terminalControllerText.gameObject.SetActive(value: true);
		instance.modListScreen.RefreshDriverNickname(instance.terminalControllerText.text);
	}

	private void OnReturnedToSinglePlayer()
	{
		if (localDriverID != cachedLocalPlayerID)
		{
			ResetTerminalControl();
		}
		else
		{
			localDriverID = LocalPlayerID;
		}
		cachedLocalPlayerID = -1;
	}

	private void OnJoinedRoom()
	{
		cachedLocalPlayerID = LocalPlayerID;
		ResetTerminalControl();
	}

	public static bool IsLocked()
	{
		if (localDriverID == -2)
		{
			return false;
		}
		return true;
	}

	public static int GetDriverID()
	{
		return localDriverID;
	}

	public static string GetDriverNickname()
	{
		if (!hasInstance)
		{
			return "";
		}
		return instance.terminalControllerText.text;
	}
}

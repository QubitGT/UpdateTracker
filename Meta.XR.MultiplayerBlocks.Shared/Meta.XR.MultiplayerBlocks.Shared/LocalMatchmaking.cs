using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.MultiplayerBlocks.Shared;

public class LocalMatchmaking : MonoBehaviour
{
	[Tooltip("On Start(), players will automatically discover local sessions and start hosting if no sessions found.")]
	[SerializeField]
	private bool automaticHostOrJoin = true;

	[Tooltip("Seconds to wait for discovering local sessions, if not found then creating their own session")]
	[SerializeField]
	private int timeDiscoveringInSec = 5;

	public static readonly UnityEvent<Guid> OnSessionCreateSucceeded = new UnityEvent<Guid>();

	public static readonly UnityEvent<string> OnSessionCreateFailed = new UnityEvent<string>();

	public static readonly UnityEvent<Guid> OnSessionDiscoverSucceeded = new UnityEvent<Guid>();

	public static readonly UnityEvent<string> OnSessionDiscoverFailed = new UnityEvent<string>();

	internal static Func<Task<bool>> BeforeStartHost;

	internal static string ExtraData = null;

	private CustomMatchmaking _customMatchmaking;

	private bool _discoveredLocalSessionAsGuest;

	private void Awake()
	{
		_customMatchmaking = UnityEngine.Object.FindObjectOfType<CustomMatchmaking>();
		if (_customMatchmaking == null)
		{
			throw new InvalidOperationException("LocalMatchmaking] No CustomMatchmaking component was found in the scene");
		}
	}

	private void OnEnable()
	{
		if (!(_customMatchmaking == null))
		{
			_customMatchmaking.onRoomCreationFinished.AddListener(OnRoomCreationFinished);
		}
	}

	private void OnDisable()
	{
		if (!(_customMatchmaking == null))
		{
			_customMatchmaking.onRoomCreationFinished.RemoveListener(OnRoomCreationFinished);
		}
	}

	private void Start()
	{
		if (automaticHostOrJoin)
		{
			HostOrJoinSessionAutomatically();
		}
	}

	public async Task StartAsHost()
	{
		if (_customMatchmaking != null)
		{
			if (BeforeStartHost == null || await BeforeStartHost())
			{
				await _customMatchmaking.CreateRoom();
			}
			else
			{
				Debug.LogError("Failed to start Colocation Session as BeforeStartHost task execution failed.");
			}
		}
	}

	public async Task StartAsGuest(bool stopAfterTimeout = true)
	{
		StartDiscoveringColocationSessions(OnColocationSessionFound);
		if (stopAfterTimeout)
		{
			await Task.Delay(timeDiscoveringInSec * 1000);
			if (!_discoveredLocalSessionAsGuest)
			{
				StopDiscoveringColocationSessions(OnColocationSessionFound);
			}
		}
	}

	private async void HostOrJoinSessionAutomatically()
	{
		_discoveredLocalSessionAsGuest = false;
		await StartAsGuest();
		if (!_discoveredLocalSessionAsGuest && _customMatchmaking != null)
		{
			Debug.Log("Didn't found an existing local session, starting to create a network room");
			await StartAsHost();
		}
	}

	private void OnRoomCreationFinished(CustomMatchmaking.RoomOperationResult result)
	{
		if (result.IsSuccess)
		{
			StartAdvertisingColocationSession(Encoding.UTF8.GetBytes(CustomMatchmakingUtils.EncodeMatchInfoWithStruct(result.RoomToken, result.RoomPassword, ExtraData)));
		}
	}

	private async void OnColocationSessionFound(OVRColocationSession.Data data)
	{
		if (!(_customMatchmaking == null))
		{
			MatchInfo matchInfo = CustomMatchmakingUtils.DecodeMatchInfoWithStruct(Encoding.UTF8.GetString(data.Metadata));
			if (!string.IsNullOrEmpty(matchInfo.RoomId))
			{
				_discoveredLocalSessionAsGuest = true;
				await _customMatchmaking.JoinRoom(matchInfo.RoomId, matchInfo.RoomPassword);
				ExtraData = matchInfo.Extra;
				ReportDiscoverEvent(data);
				StopDiscoveringColocationSessions(OnColocationSessionFound);
			}
		}
	}

	public static async void StartAdvertisingColocationSession(byte[] data)
	{
		OVRResult<Guid, OVRColocationSession.Result> oVRResult = await OVRColocationSession.StartAdvertisementAsync(data);
		switch (oVRResult.Status)
		{
		case OVRColocationSession.Result.Success:
			OnSessionCreateSucceeded?.Invoke(oVRResult.Value);
			break;
		case OVRColocationSession.Result.NetworkFailed:
			OnSessionCreateFailed?.Invoke("Failed to create the local session as connected network, please make sure the headset has joined WiFi");
			break;
		case OVRColocationSession.Result.AlreadyAdvertising:
			OnSessionCreateFailed?.Invoke("Failed to create the local session as session is already being advertised, there'll be no-op for this duplicated request");
			break;
		case OVRColocationSession.Result.Unsupported:
			OnSessionCreateFailed?.Invoke("Failed to create the local session as the feature is unsupported, please make sure this feature is required in OVRManager > Colocation Session Support");
			break;
		default:
			OnSessionCreateFailed?.Invoke($"Failed to create the local session, reason: {oVRResult.Status}");
			break;
		}
	}

	public static async void StopAdvertisingColocationSession()
	{
		OVRResult<OVRColocationSession.Result> oVRResult = await OVRColocationSession.StopAdvertisementAsync();
		if (oVRResult.Status != 0)
		{
			Debug.LogError($"Failed to stop advertisement for the colocation session: {oVRResult.Status}");
		}
	}

	public static async void StartDiscoveringColocationSessions(Action<OVRColocationSession.Data> onGroupFound)
	{
		OVRColocationSession.ColocationSessionDiscovered -= onGroupFound;
		OVRColocationSession.ColocationSessionDiscovered += onGroupFound;
		OVRResult<OVRColocationSession.Result> oVRResult = await OVRColocationSession.StartDiscoveryAsync();
		switch (oVRResult.Status)
		{
		case OVRColocationSession.Result.NoDiscoveryMethodAvailable:
			OnSessionDiscoverFailed?.Invoke("Failed to discover the local session as no available method, please make sure the headset has enabled bluetooth");
			break;
		case OVRColocationSession.Result.AlreadyDiscovering:
			OnSessionDiscoverFailed?.Invoke("Failed to discover the local session as sessions are already being discovered, there'll be no-op for this duplicated request");
			break;
		default:
			OnSessionDiscoverFailed?.Invoke($"Failed to start discovering nearby session: {oVRResult.Status}");
			break;
		case OVRColocationSession.Result.Success:
			break;
		}
	}

	public static async void StopDiscoveringColocationSessions(Action<OVRColocationSession.Data> onGroupFound)
	{
		OVRResult<OVRColocationSession.Result> oVRResult = await OVRColocationSession.StopDiscoveryAsync();
		if (oVRResult.Status == OVRColocationSession.Result.Success)
		{
			OVRColocationSession.ColocationSessionDiscovered -= onGroupFound;
		}
		else
		{
			Debug.LogError($"Failed to stop discovering nearby session: {oVRResult.Status}");
		}
	}

	private static void ReportDiscoverEvent(OVRColocationSession.Data data)
	{
		OnSessionDiscoverSucceeded?.Invoke(data.AdvertisementUuid);
	}
}

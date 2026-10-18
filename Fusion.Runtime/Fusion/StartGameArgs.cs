using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Fusion.Photon.Realtime;
using Fusion.Sockets;

namespace Fusion;

public struct StartGameArgs
{
	public GameMode GameMode;

	public string SessionName;

	public Func<string> SessionNameGenerator;

	public NetAddress? Address;

	public NetAddress? CustomPublicAddress;

	public INetworkObjectProvider ObjectProvider;

	public INetworkSceneManager SceneManager;

	public INetworkRunnerUpdater Updater;

	public INetworkObjectInitializer ObjectInitializer;

	public NetworkProjectConfig Config;

	public int? PlayerCount;

	public NetworkSceneInfo? Scene;

	public Action<NetworkRunner> OnGameStarted;

	public bool DisableNATPunchthrough;

	public Type[] CustomCallbackInterfaces;

	public byte[] ConnectionToken;

	public Dictionary<string, SessionProperty> SessionProperties;

	public bool? IsOpen;

	public bool? IsVisible;

	public MatchmakingMode? MatchmakingMode;

	public bool? UseDefaultPhotonCloudPorts;

	public string CustomLobbyName;

	public string CustomSTUNServer;

	public AuthenticationValues AuthValues;

	public FusionAppSettings CustomPhotonAppSettings;

	public bool? EnableClientSessionCreation;

	public HostMigrationToken HostMigrationToken;

	public Action<NetworkRunner> HostMigrationResume;

	public CancellationToken StartGameCancellationToken;

	public bool? UseCachedRegions;

	public override string ToString()
	{
		string text = ((CustomCallbackInterfaces != null) ? string.Join(", ", CustomCallbackInterfaces.AsEnumerable()) : null);
		string text2 = ((ConnectionToken != null) ? $"Length: {ConnectionToken.Length}" : null);
		string text3 = null;
		if (SessionProperties != null)
		{
			foreach (KeyValuePair<string, SessionProperty> sessionProperty in SessionProperties)
			{
				text3 += $"{sessionProperty.Key}={sessionProperty.Value}, ";
			}
		}
		return "[StartGameArgs: " + string.Format("{0}={1}, ", "GameMode", GameMode) + "SessionName=" + SessionName + ", " + string.Format("{0}={1}, ", "IsVisible", IsVisible) + string.Format("{0}={1}, ", "IsOpen", IsOpen) + string.Format("{0}={1}, ", "Address", Address) + string.Format("{0}={1}, ", "CustomPublicAddress", CustomPublicAddress) + string.Format("{0}={1}, ", "ObjectProvider", ObjectProvider) + string.Format("{0}={1}, ", "SceneManager", SceneManager) + string.Format("{0}={1}, ", "PlayerCount", PlayerCount) + string.Format("{0}={1}, ", "Scene", Scene) + string.Format("{0}={1}, ", "DisableNATPunchthrough", DisableNATPunchthrough) + "CustomCallbackInterfaces=" + text + ", ConnectionToken=" + text2 + ", SessionProperties=" + text3 + ", CustomLobbyName=" + CustomLobbyName + ", CustomSTUNServer=" + CustomSTUNServer + ", " + string.Format("{0}={1}, ", "AuthValues", AuthValues) + string.Format("{0}={1}, ", "CustomPhotonAppSettings", CustomPhotonAppSettings) + string.Format("{0}={1}, ", "EnableClientSessionCreation", EnableClientSessionCreation) + string.Format("{0}={1}]", "HostMigrationToken", HostMigrationToken);
	}
}

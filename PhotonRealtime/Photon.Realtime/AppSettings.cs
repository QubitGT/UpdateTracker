using System;
using ExitGames.Client.Photon;

namespace Photon.Realtime;

[Serializable]
public class AppSettings
{
	public string AppIdRealtime;

	public string AppIdFusion;

	public string AppIdChat;

	public string AppIdVoice;

	public string AppVersion;

	public bool UseNameServer = true;

	public string FixedRegion;

	[NonSerialized]
	public string BestRegionSummaryFromStorage;

	public string Server;

	public int Port;

	public string ProxyServer;

	public ConnectionProtocol Protocol;

	public bool EnableProtocolFallback = true;

	public AuthModeOption AuthMode;

	public bool EnableLobbyStatistics;

	public DebugLevel NetworkLogging = DebugLevel.ERROR;

	public bool IsMasterServerAddress => !UseNameServer;

	public bool IsBestRegion
	{
		get
		{
			if (UseNameServer)
			{
				return string.IsNullOrEmpty(FixedRegion);
			}
			return false;
		}
	}

	public bool IsDefaultNameServer
	{
		get
		{
			if (UseNameServer)
			{
				return string.IsNullOrEmpty(Server);
			}
			return false;
		}
	}

	public bool IsDefaultPort => Port <= 0;

	public string ToStringFull()
	{
		return string.Format("appId {0}{1}{2}{3}use ns: {4}, reg: {5}, {9}, {6}{7}{8}auth: {10}", string.IsNullOrEmpty(AppIdRealtime) ? string.Empty : ("Realtime/PUN: " + HideAppId(AppIdRealtime) + ", "), string.IsNullOrEmpty(AppIdFusion) ? string.Empty : ("Fusion: " + HideAppId(AppIdFusion) + ", "), string.IsNullOrEmpty(AppIdChat) ? string.Empty : ("Chat: " + HideAppId(AppIdChat) + ", "), string.IsNullOrEmpty(AppIdVoice) ? string.Empty : ("Voice: " + HideAppId(AppIdVoice) + ", "), string.IsNullOrEmpty(AppVersion) ? string.Empty : ("AppVersion: " + AppVersion + ", "), "UseNameServer: " + UseNameServer + ", ", "Fixed Region: " + FixedRegion + ", ", string.IsNullOrEmpty(Server) ? string.Empty : ("Server: " + Server + ", "), IsDefaultPort ? string.Empty : ("Port: " + Port + ", "), string.IsNullOrEmpty(ProxyServer) ? string.Empty : ("Proxy: " + ProxyServer + ", "), Protocol, AuthMode);
	}

	public static bool IsAppId(string val)
	{
		try
		{
			new Guid(val);
		}
		catch
		{
			return false;
		}
		return true;
	}

	private string HideAppId(string appId)
	{
		if (!string.IsNullOrEmpty(appId) && appId.Length >= 8)
		{
			return appId.Substring(0, 8) + "***";
		}
		return appId;
	}

	public AppSettings CopyTo(AppSettings d)
	{
		d.AppIdRealtime = AppIdRealtime;
		d.AppIdFusion = AppIdFusion;
		d.AppIdChat = AppIdChat;
		d.AppIdVoice = AppIdVoice;
		d.AppVersion = AppVersion;
		d.UseNameServer = UseNameServer;
		d.FixedRegion = FixedRegion;
		d.BestRegionSummaryFromStorage = BestRegionSummaryFromStorage;
		d.Server = Server;
		d.Port = Port;
		d.ProxyServer = ProxyServer;
		d.Protocol = Protocol;
		d.AuthMode = AuthMode;
		d.EnableLobbyStatistics = EnableLobbyStatistics;
		d.NetworkLogging = NetworkLogging;
		d.EnableProtocolFallback = EnableProtocolFallback;
		return d;
	}
}

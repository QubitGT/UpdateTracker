using System;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Fusion.Photon.Realtime;

[Serializable]
public class AppSettings
{
	[NonSerialized]
	[InlineHelp]
	public string AppIdRealtime;

	[InlineHelp]
	public string AppIdFusion;

	[InlineHelp]
	public string AppIdChat;

	[InlineHelp]
	public string AppIdVoice;

	[InlineHelp]
	public string AppVersion;

	[InlineHelp]
	public bool UseNameServer = true;

	[InlineHelp]
	public string FixedRegion;

	[NonSerialized]
	[InlineHelp]
	public string BestRegionSummaryFromStorage;

	[InlineHelp]
	public string Server;

	[InlineHelp]
	public int Port;

	[InlineHelp]
	public string ProxyServer;

	[Header("Miscellaneous")]
	[InlineHelp]
	public ConnectionProtocol Protocol = ConnectionProtocol.Udp;

	[InlineHelp]
	public bool EnableProtocolFallback = true;

	[InlineHelp]
	public AuthModeOption AuthMode = AuthModeOption.Auth;

	[InlineHelp]
	public bool EnableLobbyStatistics;

	public DebugLevel NetworkLogging
	{
		get
		{
			if (InternalLogStreams.LogTraceRealtime != null)
			{
				return DebugLevel.ALL;
			}
			switch (Log.Settings.Level)
			{
			case LogLevel.Debug:
			case LogLevel.Info:
			case LogLevel.Warn:
				return DebugLevel.WARNING;
			case LogLevel.Error:
				return DebugLevel.ERROR;
			default:
				return DebugLevel.OFF;
			}
		}
		set
		{
		}
	}

	public bool IsMasterServerAddress => !UseNameServer;

	public bool IsBestRegion => UseNameServer && string.IsNullOrEmpty(FixedRegion);

	public bool IsDefaultNameServer => UseNameServer && string.IsNullOrEmpty(Server);

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
		return (string.IsNullOrEmpty(appId) || appId.Length < 8) ? appId : (appId.Substring(0, 8) + "***");
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

	public AppSettings GetCopy()
	{
		return CopyTo(new AppSettings());
	}
}

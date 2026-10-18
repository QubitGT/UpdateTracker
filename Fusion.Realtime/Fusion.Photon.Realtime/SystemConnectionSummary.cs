using System.Text;
using ExitGames.Client.Photon;

namespace Fusion.Photon.Realtime;

internal class SystemConnectionSummary
{
	private class SCSBitPos
	{
		public const int Version = 28;

		public const int UsedProtocol = 25;

		public const int EmptyBit = 24;

		public const int AppQuits = 23;

		public const int AppPause = 22;

		public const int AppPauseRecent = 21;

		public const int AppOutOfFocus = 20;

		public const int AppOutOfFocusRecent = 19;

		public const int NetworkReachable = 18;

		public const int ErrorCodeFits = 17;

		public const int ErrorCodeWinSock = 16;
	}

	public readonly byte Version = 0;

	public byte UsedProtocol;

	public bool AppQuits;

	public bool AppPause;

	public bool AppPauseRecent;

	public bool AppOutOfFocus;

	public bool AppOutOfFocusRecent;

	public bool NetworkReachable;

	public bool ErrorCodeFits;

	public bool ErrorCodeWinSock;

	public int SocketErrorCode;

	private static readonly string[] ProtocolIdToName = new string[8] { "UDP", "TCP", "2(N/A)", "3(N/A)", "WS", "WSS", "6(N/A)", "7WebRTC" };

	public SystemConnectionSummary(LoadBalancingClient client)
	{
		if (client != null)
		{
			UsedProtocol = (byte)(client.LoadBalancingPeer.UsedProtocol & (ConnectionProtocol)7);
			SocketErrorCode = client.LoadBalancingPeer.SocketErrorCode;
		}
		AppQuits = ConnectionHandler.AppQuits;
		AppPause = ConnectionHandler.AppPause;
		AppPauseRecent = ConnectionHandler.AppPauseRecent;
		AppOutOfFocus = ConnectionHandler.AppOutOfFocus;
		AppOutOfFocusRecent = ConnectionHandler.AppOutOfFocusRecent;
		NetworkReachable = ConnectionHandler.IsNetworkReachableUnity();
		ErrorCodeFits = SocketErrorCode <= 32767;
		ErrorCodeWinSock = true;
	}

	public SystemConnectionSummary(int summary)
	{
		Version = GetBits(ref summary, 28, 15);
		UsedProtocol = GetBits(ref summary, 25, 7);
		AppQuits = GetBit(ref summary, 23);
		AppPause = GetBit(ref summary, 22);
		AppPauseRecent = GetBit(ref summary, 21);
		AppOutOfFocus = GetBit(ref summary, 20);
		AppOutOfFocusRecent = GetBit(ref summary, 19);
		NetworkReachable = GetBit(ref summary, 18);
		ErrorCodeFits = GetBit(ref summary, 17);
		ErrorCodeWinSock = GetBit(ref summary, 16);
		SocketErrorCode = summary & 0xFFFF;
	}

	public int ToInt()
	{
		int value = 0;
		SetBits(ref value, Version, 28);
		SetBits(ref value, UsedProtocol, 25);
		SetBit(ref value, AppQuits, 23);
		SetBit(ref value, AppPause, 22);
		SetBit(ref value, AppPauseRecent, 21);
		SetBit(ref value, AppOutOfFocus, 20);
		SetBit(ref value, AppOutOfFocusRecent, 19);
		SetBit(ref value, NetworkReachable, 18);
		SetBit(ref value, ErrorCodeFits, 17);
		SetBit(ref value, ErrorCodeWinSock, 16);
		int num = SocketErrorCode & 0xFFFF;
		return value | num;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string arg = ProtocolIdToName[UsedProtocol];
		stringBuilder.Append($"SCS v{Version} {arg} SocketErrorCode: {SocketErrorCode} ");
		if (AppQuits)
		{
			stringBuilder.Append("AppQuits ");
		}
		if (AppPause)
		{
			stringBuilder.Append("AppPause ");
		}
		if (!AppPause && AppPauseRecent)
		{
			stringBuilder.Append("AppPauseRecent ");
		}
		if (AppOutOfFocus)
		{
			stringBuilder.Append("AppOutOfFocus ");
		}
		if (!AppOutOfFocus && AppOutOfFocusRecent)
		{
			stringBuilder.Append("AppOutOfFocusRecent ");
		}
		if (!NetworkReachable)
		{
			stringBuilder.Append("NetworkUnreachable ");
		}
		if (!ErrorCodeFits)
		{
			stringBuilder.Append("ErrorCodeRangeExceeded ");
		}
		if (ErrorCodeWinSock)
		{
			stringBuilder.Append("WinSock");
		}
		else
		{
			stringBuilder.Append("BSDSock");
		}
		return stringBuilder.ToString();
	}

	public static bool GetBit(ref int value, int bitpos)
	{
		int num = (value >> bitpos) & 1;
		return num != 0;
	}

	public static byte GetBits(ref int value, int bitpos, byte mask)
	{
		int num = (value >> bitpos) & mask;
		return (byte)num;
	}

	public static void SetBit(ref int value, bool bitval, int bitpos)
	{
		if (bitval)
		{
			value |= 1 << bitpos;
		}
		else
		{
			value &= ~(1 << bitpos);
		}
	}

	public static void SetBits(ref int value, byte bitvals, int bitpos)
	{
		value |= bitvals << bitpos;
	}
}

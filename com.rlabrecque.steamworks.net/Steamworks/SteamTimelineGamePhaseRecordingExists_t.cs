using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
[CallbackIdentity(6001)]
public struct SteamTimelineGamePhaseRecordingExists_t
{
	public const int k_iCallback = 6001;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	private byte[] m_rgchPhaseID_;

	public ulong m_ulRecordingMS;

	public ulong m_ulLongestClipMS;

	public uint m_unClipCount;

	public uint m_unScreenshotCount;

	public string m_rgchPhaseID
	{
		get
		{
			return InteropHelp.ByteArrayToStringUTF8(m_rgchPhaseID_);
		}
		set
		{
			InteropHelp.StringToByteArrayUTF8(value, m_rgchPhaseID_, 64);
		}
	}
}

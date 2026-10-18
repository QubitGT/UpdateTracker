using System;
using System.Runtime.InteropServices;

namespace Steamworks;

[Serializable]
public struct SteamNetworkingMessage_t
{
	public IntPtr m_pData;

	public int m_cbSize;

	public HSteamNetConnection m_conn;

	public SteamNetworkingIdentity m_identityPeer;

	public long m_nConnUserData;

	public SteamNetworkingMicroseconds m_usecTimeReceived;

	public long m_nMessageNumber;

	public IntPtr m_pfnFreeData;

	internal IntPtr m_pfnRelease;

	public int m_nChannel;

	public int m_nFlags;

	public long m_nUserData;

	public ushort m_idxLane;

	public ushort _pad1__;

	public void Release()
	{
		throw new NotImplementedException("Please use the static Release function instead which takes an IntPtr.");
	}

	public static void Release(IntPtr pointer)
	{
		NativeMethods.SteamAPI_SteamNetworkingMessage_t_Release(pointer);
	}

	public static SteamNetworkingMessage_t FromIntPtr(IntPtr pointer)
	{
		return Marshal.PtrToStructure<SteamNetworkingMessage_t>(pointer);
	}
}

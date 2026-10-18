using System;
using System.Runtime.InteropServices;

namespace Steamworks;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SteamNetworkingIdentity : IEquatable<SteamNetworkingIdentity>
{
	public ESteamNetworkingIdentityType m_eType;

	private int m_cbSize;

	private uint m_reserved0;

	private uint m_reserved1;

	private uint m_reserved2;

	private uint m_reserved3;

	private uint m_reserved4;

	private uint m_reserved5;

	private uint m_reserved6;

	private uint m_reserved7;

	private uint m_reserved8;

	private uint m_reserved9;

	private uint m_reserved10;

	private uint m_reserved11;

	private uint m_reserved12;

	private uint m_reserved13;

	private uint m_reserved14;

	private uint m_reserved15;

	private uint m_reserved16;

	private uint m_reserved17;

	private uint m_reserved18;

	private uint m_reserved19;

	private uint m_reserved20;

	private uint m_reserved21;

	private uint m_reserved22;

	private uint m_reserved23;

	private uint m_reserved24;

	private uint m_reserved25;

	private uint m_reserved26;

	private uint m_reserved27;

	private uint m_reserved28;

	private uint m_reserved29;

	private uint m_reserved30;

	private uint m_reserved31;

	public const int k_cchMaxString = 128;

	public const int k_cchMaxGenericString = 32;

	public const int k_cchMaxXboxPairwiseID = 33;

	public const int k_cbMaxGenericBytes = 32;

	public void Clear()
	{
		NativeMethods.SteamAPI_SteamNetworkingIdentity_Clear(ref this);
	}

	public bool IsInvalid()
	{
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_IsInvalid(ref this);
	}

	public void SetSteamID(CSteamID steamID)
	{
		NativeMethods.SteamAPI_SteamNetworkingIdentity_SetSteamID(ref this, (ulong)steamID);
	}

	public CSteamID GetSteamID()
	{
		return (CSteamID)NativeMethods.SteamAPI_SteamNetworkingIdentity_GetSteamID(ref this);
	}

	public void SetSteamID64(ulong steamID)
	{
		NativeMethods.SteamAPI_SteamNetworkingIdentity_SetSteamID64(ref this, steamID);
	}

	public ulong GetSteamID64()
	{
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetSteamID64(ref this);
	}

	public bool SetXboxPairwiseID(string pszString)
	{
		using InteropHelp.UTF8StringHandle pszString2 = new InteropHelp.UTF8StringHandle(pszString);
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_SetXboxPairwiseID(ref this, pszString2);
	}

	public string GetXboxPairwiseID()
	{
		return InteropHelp.PtrToStringUTF8(NativeMethods.SteamAPI_SteamNetworkingIdentity_GetXboxPairwiseID(ref this));
	}

	public void SetPSNID(ulong id)
	{
		NativeMethods.SteamAPI_SteamNetworkingIdentity_SetPSNID(ref this, id);
	}

	public ulong GetPSNID()
	{
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetPSNID(ref this);
	}

	public void SetIPAddr(SteamNetworkingIPAddr addr)
	{
		NativeMethods.SteamAPI_SteamNetworkingIdentity_SetIPAddr(ref this, ref addr);
	}

	public SteamNetworkingIPAddr GetIPAddr()
	{
		throw new NotImplementedException();
	}

	public void SetIPv4Addr(uint nIPv4, ushort nPort)
	{
		NativeMethods.SteamAPI_SteamNetworkingIdentity_SetIPv4Addr(ref this, nIPv4, nPort);
	}

	public uint GetIPv4()
	{
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetIPv4(ref this);
	}

	public ESteamNetworkingFakeIPType GetFakeIPType()
	{
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_GetFakeIPType(ref this);
	}

	public bool IsFakeIP()
	{
		return GetFakeIPType() > ESteamNetworkingFakeIPType.k_ESteamNetworkingFakeIPType_NotFake;
	}

	public void SetLocalHost()
	{
		NativeMethods.SteamAPI_SteamNetworkingIdentity_SetLocalHost(ref this);
	}

	public bool IsLocalHost()
	{
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_IsLocalHost(ref this);
	}

	public bool SetGenericString(string pszString)
	{
		using InteropHelp.UTF8StringHandle pszString2 = new InteropHelp.UTF8StringHandle(pszString);
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_SetGenericString(ref this, pszString2);
	}

	public string GetGenericString()
	{
		return InteropHelp.PtrToStringUTF8(NativeMethods.SteamAPI_SteamNetworkingIdentity_GetGenericString(ref this));
	}

	public bool SetGenericBytes(byte[] data, uint cbLen)
	{
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_SetGenericBytes(ref this, data, cbLen);
	}

	public byte[] GetGenericBytes(out int cbLen)
	{
		throw new NotImplementedException();
	}

	public bool Equals(SteamNetworkingIdentity x)
	{
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_IsEqualTo(ref this, ref x);
	}

	public void ToString(out string buf)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(128);
		NativeMethods.SteamAPI_SteamNetworkingIdentity_ToString(ref this, intPtr, 128u);
		buf = InteropHelp.PtrToStringUTF8(intPtr);
		Marshal.FreeHGlobal(intPtr);
	}

	public bool ParseString(string pszStr)
	{
		using InteropHelp.UTF8StringHandle pszStr2 = new InteropHelp.UTF8StringHandle(pszStr);
		return NativeMethods.SteamAPI_SteamNetworkingIdentity_ParseString(ref this, pszStr2);
	}
}

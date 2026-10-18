using System;
using System.Diagnostics.CodeAnalysis;

namespace Fusion.Sockets;

public struct NetSendEnvelope
{
	public unsafe void* UserData;

	public double SendTime;

	public ushort Sequence;

	internal NetPacketType PacketType;

	[return: NotNull]
	public unsafe T* TakeUserData<T>() where T : unmanaged
	{
		Assert.Always(UserData != null, (IntPtr)UserData);
		T* userData = (T*)UserData;
		UserData = null;
		return userData;
	}
}

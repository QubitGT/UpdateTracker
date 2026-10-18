using System;
using System.Runtime.InteropServices;

namespace Steamworks;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct RemotePlayInput_t
{
	[StructLayout(LayoutKind.Explicit, Size = 56)]
	public struct OptionValue
	{
		[FieldOffset(0)]
		public RemotePlayInputMouseMotion_t m_MouseMotion;

		[FieldOffset(0)]
		public ERemotePlayMouseButton m_eMouseButton;

		[FieldOffset(0)]
		public RemotePlayInputMouseWheel_t m_MouseWheel;

		[FieldOffset(0)]
		public RemotePlayInputKey_t m_Key;
	}

	public RemotePlaySessionID_t m_unSessionID;

	public ERemotePlayInputType m_eType;

	public OptionValue m_val;
}

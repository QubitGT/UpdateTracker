using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Steamworks;

public class ISteamMatchmakingPingResponse
{
	public delegate void ServerResponded(gameserveritem_t server);

	public delegate void ServerFailedToRespond();

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate void InternalServerResponded(IntPtr thisptr, gameserveritem_t server);

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate void InternalServerFailedToRespond(IntPtr thisptr);

	[StructLayout(LayoutKind.Sequential)]
	private class VTable
	{
		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InternalServerResponded m_VTServerResponded;

		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InternalServerFailedToRespond m_VTServerFailedToRespond;
	}

	private VTable m_VTable;

	private IntPtr m_pVTable;

	private GCHandle m_pGCHandle;

	private IntPtr m_pInstance;

	private ServerResponded m_ServerResponded;

	private ServerFailedToRespond m_ServerFailedToRespond;

	private static readonly Dictionary<IntPtr, ISteamMatchmakingPingResponse> m_Instances = new Dictionary<IntPtr, ISteamMatchmakingPingResponse>();

	public ISteamMatchmakingPingResponse(ServerResponded onServerResponded, ServerFailedToRespond onServerFailedToRespond)
	{
		if (onServerResponded == null || onServerFailedToRespond == null)
		{
			throw new ArgumentNullException();
		}
		m_ServerResponded = onServerResponded;
		m_ServerFailedToRespond = onServerFailedToRespond;
		m_VTable = new VTable
		{
			m_VTServerResponded = InternalOnServerResponded,
			m_VTServerFailedToRespond = InternalOnServerFailedToRespond
		};
		m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf<VTable>());
		Marshal.StructureToPtr(m_VTable, m_pVTable, fDeleteOld: false);
		m_pGCHandle = GCHandle.Alloc(m_pVTable, GCHandleType.Pinned);
		m_pInstance = m_pGCHandle.AddrOfPinnedObject();
		lock (m_Instances)
		{
			m_Instances[m_pInstance] = this;
		}
	}

	~ISteamMatchmakingPingResponse()
	{
		lock (m_Instances)
		{
			m_Instances.Remove(m_pVTable);
		}
		if (m_pVTable != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(m_pVTable);
		}
		if (m_pGCHandle.IsAllocated)
		{
			m_pGCHandle.Free();
		}
	}

	[MonoPInvokeCallback(typeof(InternalServerResponded))]
	private static void InternalOnServerResponded(IntPtr thisptr, gameserveritem_t server)
	{
		if (m_Instances.TryGetValue(thisptr, out var value))
		{
			value.m_ServerResponded(server);
		}
	}

	[MonoPInvokeCallback(typeof(InternalServerFailedToRespond))]
	private static void InternalOnServerFailedToRespond(IntPtr thisptr)
	{
		if (m_Instances.TryGetValue(thisptr, out var value))
		{
			value.m_ServerFailedToRespond();
		}
	}

	public static explicit operator IntPtr(ISteamMatchmakingPingResponse that)
	{
		return that.m_pGCHandle.AddrOfPinnedObject();
	}
}

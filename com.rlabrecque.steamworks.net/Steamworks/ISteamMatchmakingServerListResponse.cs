using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Steamworks;

public class ISteamMatchmakingServerListResponse
{
	public delegate void ServerResponded(HServerListRequest hRequest, int iServer);

	public delegate void ServerFailedToRespond(HServerListRequest hRequest, int iServer);

	public delegate void RefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response);

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate void InternalServerResponded(IntPtr thisptr, HServerListRequest hRequest, int iServer);

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate void InternalServerFailedToRespond(IntPtr thisptr, HServerListRequest hRequest, int iServer);

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	private delegate void InternalRefreshComplete(IntPtr thisptr, HServerListRequest hRequest, EMatchMakingServerResponse response);

	[StructLayout(LayoutKind.Sequential)]
	private class VTable
	{
		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InternalServerResponded m_VTServerResponded;

		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InternalServerFailedToRespond m_VTServerFailedToRespond;

		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InternalRefreshComplete m_VTRefreshComplete;
	}

	private VTable m_VTable;

	private IntPtr m_pVTable;

	private GCHandle m_pGCHandle;

	private IntPtr m_pInstance;

	private ServerResponded m_ServerResponded;

	private ServerFailedToRespond m_ServerFailedToRespond;

	private RefreshComplete m_RefreshComplete;

	private static readonly Dictionary<IntPtr, ISteamMatchmakingServerListResponse> m_Instances = new Dictionary<IntPtr, ISteamMatchmakingServerListResponse>();

	public ISteamMatchmakingServerListResponse(ServerResponded onServerResponded, ServerFailedToRespond onServerFailedToRespond, RefreshComplete onRefreshComplete)
	{
		if (onServerResponded == null || onServerFailedToRespond == null || onRefreshComplete == null)
		{
			throw new ArgumentNullException();
		}
		m_ServerResponded = onServerResponded;
		m_ServerFailedToRespond = onServerFailedToRespond;
		m_RefreshComplete = onRefreshComplete;
		m_VTable = new VTable
		{
			m_VTServerResponded = InternalOnServerResponded,
			m_VTServerFailedToRespond = InternalOnServerFailedToRespond,
			m_VTRefreshComplete = InternalOnRefreshComplete
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

	~ISteamMatchmakingServerListResponse()
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
	private static void InternalOnServerResponded(IntPtr thisptr, HServerListRequest hRequest, int iServer)
	{
		try
		{
			if (m_Instances.TryGetValue(thisptr, out var value))
			{
				value.m_ServerResponded(hRequest, iServer);
			}
		}
		catch (Exception e)
		{
			CallbackDispatcher.ExceptionHandler(e);
		}
	}

	[MonoPInvokeCallback(typeof(InternalServerFailedToRespond))]
	private static void InternalOnServerFailedToRespond(IntPtr thisptr, HServerListRequest hRequest, int iServer)
	{
		try
		{
			if (m_Instances.TryGetValue(thisptr, out var value))
			{
				value.m_ServerFailedToRespond(hRequest, iServer);
			}
		}
		catch (Exception e)
		{
			CallbackDispatcher.ExceptionHandler(e);
		}
	}

	[MonoPInvokeCallback(typeof(InternalRefreshComplete))]
	private static void InternalOnRefreshComplete(IntPtr thisptr, HServerListRequest hRequest, EMatchMakingServerResponse response)
	{
		try
		{
			if (m_Instances.TryGetValue(thisptr, out var value))
			{
				value.m_RefreshComplete(hRequest, response);
			}
		}
		catch (Exception e)
		{
			CallbackDispatcher.ExceptionHandler(e);
		}
	}

	public static explicit operator IntPtr(ISteamMatchmakingServerListResponse that)
	{
		return that.m_pGCHandle.AddrOfPinnedObject();
	}
}

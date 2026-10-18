using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Steamworks;

public class ISteamMatchmakingRulesResponse
{
	public delegate void RulesResponded(string pchRule, string pchValue);

	public delegate void RulesFailedToRespond();

	public delegate void RulesRefreshComplete();

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate void InternalRulesResponded(IntPtr thisptr, IntPtr pchRule, IntPtr pchValue);

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate void InternalRulesFailedToRespond(IntPtr thisptr);

	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate void InternalRulesRefreshComplete(IntPtr thisptr);

	[StructLayout(LayoutKind.Sequential)]
	private class VTable
	{
		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InternalRulesResponded m_VTRulesResponded;

		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InternalRulesFailedToRespond m_VTRulesFailedToRespond;

		[NonSerialized]
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public InternalRulesRefreshComplete m_VTRulesRefreshComplete;
	}

	private VTable m_VTable;

	private IntPtr m_pVTable;

	private GCHandle m_pGCHandle;

	private IntPtr m_pInstance;

	private RulesResponded m_RulesResponded;

	private RulesFailedToRespond m_RulesFailedToRespond;

	private RulesRefreshComplete m_RulesRefreshComplete;

	private static readonly Dictionary<IntPtr, ISteamMatchmakingRulesResponse> m_Instances = new Dictionary<IntPtr, ISteamMatchmakingRulesResponse>();

	public ISteamMatchmakingRulesResponse(RulesResponded onRulesResponded, RulesFailedToRespond onRulesFailedToRespond, RulesRefreshComplete onRulesRefreshComplete)
	{
		if (onRulesResponded == null || onRulesFailedToRespond == null || onRulesRefreshComplete == null)
		{
			throw new ArgumentNullException();
		}
		m_RulesResponded = onRulesResponded;
		m_RulesFailedToRespond = onRulesFailedToRespond;
		m_RulesRefreshComplete = onRulesRefreshComplete;
		m_VTable = new VTable
		{
			m_VTRulesResponded = InternalOnRulesResponded,
			m_VTRulesFailedToRespond = InternalOnRulesFailedToRespond,
			m_VTRulesRefreshComplete = InternalOnRulesRefreshComplete
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

	~ISteamMatchmakingRulesResponse()
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

	[MonoPInvokeCallback(typeof(InternalRulesResponded))]
	private static void InternalOnRulesResponded(IntPtr thisptr, IntPtr pchRule, IntPtr pchValue)
	{
		if (m_Instances.TryGetValue(thisptr, out var value))
		{
			value.m_RulesResponded(InteropHelp.PtrToStringUTF8(pchRule), InteropHelp.PtrToStringUTF8(pchValue));
		}
	}

	[MonoPInvokeCallback(typeof(InternalRulesFailedToRespond))]
	private static void InternalOnRulesFailedToRespond(IntPtr thisptr)
	{
		if (m_Instances.TryGetValue(thisptr, out var value))
		{
			value.m_RulesFailedToRespond();
		}
	}

	[MonoPInvokeCallback(typeof(InternalRulesRefreshComplete))]
	private static void InternalOnRulesRefreshComplete(IntPtr thisptr)
	{
		if (m_Instances.TryGetValue(thisptr, out var value))
		{
			value.m_RulesRefreshComplete();
		}
	}

	public static explicit operator IntPtr(ISteamMatchmakingRulesResponse that)
	{
		return that.m_pGCHandle.AddrOfPinnedObject();
	}
}

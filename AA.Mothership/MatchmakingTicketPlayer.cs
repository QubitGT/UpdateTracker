using System;
using System.Runtime.InteropServices;

public class MatchmakingTicketPlayer : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public static readonly string latency_in_ms_name = MothershipApiPINVOKE.MatchmakingTicketPlayer_latency_in_ms_name_get();

	public static readonly string team_name = MothershipApiPINVOKE.MatchmakingTicketPlayer_team_name_get();

	public StringIntMap latency_in_ms
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.MatchmakingTicketPlayer_latency_in_ms_get(swigCPtr);
			StringIntMap result = ((intPtr == IntPtr.Zero) ? null : new StringIntMap(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MatchmakingTicketPlayer_latency_in_ms_set(swigCPtr, StringIntMap.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string team
	{
		get
		{
			string result = MothershipApiPINVOKE.MatchmakingTicketPlayer_team_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MatchmakingTicketPlayer_team_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal MatchmakingTicketPlayer(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MatchmakingTicketPlayer obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MatchmakingTicketPlayer obj)
	{
		if (obj != null)
		{
			if (!obj.swigCMemOwn)
			{
				throw new ApplicationException("Cannot release ownership as memory is not owned");
			}
			HandleRef result = obj.swigCPtr;
			obj.swigCMemOwn = false;
			obj.Dispose();
			return result;
		}
		return new HandleRef(null, IntPtr.Zero);
	}

	~MatchmakingTicketPlayer()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					MothershipApiPINVOKE.delete_MatchmakingTicketPlayer(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public MatchmakingTicketPlayer()
		: this(MothershipApiPINVOKE.new_MatchmakingTicketPlayer(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

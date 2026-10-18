using System;
using System.Runtime.InteropServices;

public class StartMatchmakingClientResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MatchmakingTicket ticket
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.StartMatchmakingClientResponse_ticket_get(swigCPtr);
			MatchmakingTicket result = ((intPtr == IntPtr.Zero) ? null : new MatchmakingTicket(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.StartMatchmakingClientResponse_ticket_set(swigCPtr, MatchmakingTicket.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal StartMatchmakingClientResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.StartMatchmakingClientResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(StartMatchmakingClientResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(StartMatchmakingClientResponse obj)
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

	protected override void Dispose(bool disposing)
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					MothershipApiPINVOKE.delete_StartMatchmakingClientResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.StartMatchmakingClientResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static StartMatchmakingClientResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.StartMatchmakingClientResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		StartMatchmakingClientResponse result = ((intPtr == IntPtr.Zero) ? null : new StartMatchmakingClientResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public StartMatchmakingClientResponse()
		: this(MothershipApiPINVOKE.new_StartMatchmakingClientResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

using System;
using System.Runtime.InteropServices;

public class StartMatchBackfillServerResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MatchmakingTicket Ticket
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.StartMatchBackfillServerResponse_Ticket_get(swigCPtr);
			MatchmakingTicket result = ((intPtr == IntPtr.Zero) ? null : new MatchmakingTicket(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.StartMatchBackfillServerResponse_Ticket_set(swigCPtr, MatchmakingTicket.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal StartMatchBackfillServerResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.StartMatchBackfillServerResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(StartMatchBackfillServerResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(StartMatchBackfillServerResponse obj)
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
					MothershipApiPINVOKE.delete_StartMatchBackfillServerResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.StartMatchBackfillServerResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static StartMatchBackfillServerResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.StartMatchBackfillServerResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		StartMatchBackfillServerResponse result = ((intPtr == IntPtr.Zero) ? null : new StartMatchBackfillServerResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public StartMatchBackfillServerResponse()
		: this(MothershipApiPINVOKE.new_StartMatchBackfillServerResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

using System;
using System.Runtime.InteropServices;

public class GetMatchmakingStatusClientResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MatchmakingTicket ticket
	{
		get
		{
			IntPtr matchmakingStatusClientResponse_ticket_get = MothershipApiPINVOKE.GetMatchmakingStatusClientResponse_ticket_get(swigCPtr);
			MatchmakingTicket result = ((matchmakingStatusClientResponse_ticket_get == IntPtr.Zero) ? null : new MatchmakingTicket(matchmakingStatusClientResponse_ticket_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetMatchmakingStatusClientResponse_ticket_set(swigCPtr, MatchmakingTicket.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetMatchmakingStatusClientResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetMatchmakingStatusClientResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetMatchmakingStatusClientResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetMatchmakingStatusClientResponse obj)
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
					MothershipApiPINVOKE.delete_GetMatchmakingStatusClientResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool matchmakingStatusClientResponse_ParseFromResponseString = MothershipApiPINVOKE.GetMatchmakingStatusClientResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return matchmakingStatusClientResponse_ParseFromResponseString;
	}

	public static GetMatchmakingStatusClientResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr matchmakingStatusClientResponse_FromMothershipResponse = MothershipApiPINVOKE.GetMatchmakingStatusClientResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetMatchmakingStatusClientResponse result = ((matchmakingStatusClientResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetMatchmakingStatusClientResponse(matchmakingStatusClientResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetMatchmakingStatusClientResponse()
		: this(MothershipApiPINVOKE.new_GetMatchmakingStatusClientResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

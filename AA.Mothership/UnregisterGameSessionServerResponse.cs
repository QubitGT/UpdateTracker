using System;
using System.Runtime.InteropServices;

public class UnregisterGameSessionServerResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public GameSession Result
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UnregisterGameSessionServerResponse_Result_get(swigCPtr);
			GameSession result = ((intPtr == IntPtr.Zero) ? null : new GameSession(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UnregisterGameSessionServerResponse_Result_set(swigCPtr, GameSession.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal UnregisterGameSessionServerResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.UnregisterGameSessionServerResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(UnregisterGameSessionServerResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(UnregisterGameSessionServerResponse obj)
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
					MothershipApiPINVOKE.delete_UnregisterGameSessionServerResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.UnregisterGameSessionServerResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static UnregisterGameSessionServerResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.UnregisterGameSessionServerResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		UnregisterGameSessionServerResponse result = ((intPtr == IntPtr.Zero) ? null : new UnregisterGameSessionServerResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public UnregisterGameSessionServerResponse()
		: this(MothershipApiPINVOKE.new_UnregisterGameSessionServerResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

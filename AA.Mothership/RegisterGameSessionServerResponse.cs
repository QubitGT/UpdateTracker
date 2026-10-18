using System;
using System.Runtime.InteropServices;

public class RegisterGameSessionServerResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public GameSession Result
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.RegisterGameSessionServerResponse_Result_get(swigCPtr);
			GameSession result = ((intPtr == IntPtr.Zero) ? null : new GameSession(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.RegisterGameSessionServerResponse_Result_set(swigCPtr, GameSession.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal RegisterGameSessionServerResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.RegisterGameSessionServerResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(RegisterGameSessionServerResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(RegisterGameSessionServerResponse obj)
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
					MothershipApiPINVOKE.delete_RegisterGameSessionServerResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.RegisterGameSessionServerResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static RegisterGameSessionServerResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.RegisterGameSessionServerResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		RegisterGameSessionServerResponse result = ((intPtr == IntPtr.Zero) ? null : new RegisterGameSessionServerResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public RegisterGameSessionServerResponse()
		: this(MothershipApiPINVOKE.new_RegisterGameSessionServerResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

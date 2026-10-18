using System;
using System.Runtime.InteropServices;

public class RegisterGameSessionAutomationResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public GameSession Result
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.RegisterGameSessionAutomationResponse_Result_get(swigCPtr);
			GameSession result = ((intPtr == IntPtr.Zero) ? null : new GameSession(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.RegisterGameSessionAutomationResponse_Result_set(swigCPtr, GameSession.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal RegisterGameSessionAutomationResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.RegisterGameSessionAutomationResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(RegisterGameSessionAutomationResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(RegisterGameSessionAutomationResponse obj)
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
					MothershipApiPINVOKE.delete_RegisterGameSessionAutomationResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.RegisterGameSessionAutomationResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static RegisterGameSessionAutomationResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.RegisterGameSessionAutomationResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		RegisterGameSessionAutomationResponse result = ((intPtr == IntPtr.Zero) ? null : new RegisterGameSessionAutomationResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public RegisterGameSessionAutomationResponse()
		: this(MothershipApiPINVOKE.new_RegisterGameSessionAutomationResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

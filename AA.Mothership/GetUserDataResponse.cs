using System;
using System.Runtime.InteropServices;

public class GetUserDataResponse : MothershipUserData
{
	private HandleRef swigCPtr;

	internal GetUserDataResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetUserDataResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetUserDataResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetUserDataResponse obj)
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
					MothershipApiPINVOKE.delete_GetUserDataResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool userDataResponse_ParseFromResponseString = MothershipApiPINVOKE.GetUserDataResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return userDataResponse_ParseFromResponseString;
	}

	public static GetUserDataResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr userDataResponse_FromMothershipResponse = MothershipApiPINVOKE.GetUserDataResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetUserDataResponse result = ((userDataResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetUserDataResponse(userDataResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetUserDataResponse()
		: this(MothershipApiPINVOKE.new_GetUserDataResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

using System;
using System.Runtime.InteropServices;

public class GetUserDataMetadataResponse : MothershipUserDataMetadata
{
	private HandleRef swigCPtr;

	internal GetUserDataMetadataResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetUserDataMetadataResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetUserDataMetadataResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetUserDataMetadataResponse obj)
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
					MothershipApiPINVOKE.delete_GetUserDataMetadataResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool userDataMetadataResponse_ParseFromResponseString = MothershipApiPINVOKE.GetUserDataMetadataResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return userDataMetadataResponse_ParseFromResponseString;
	}

	public static GetUserDataMetadataResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr userDataMetadataResponse_FromMothershipResponse = MothershipApiPINVOKE.GetUserDataMetadataResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetUserDataMetadataResponse result = ((userDataMetadataResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetUserDataMetadataResponse(userDataMetadataResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetUserDataMetadataResponse()
		: this(MothershipApiPINVOKE.new_GetUserDataMetadataResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

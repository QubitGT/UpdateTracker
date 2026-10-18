using System;
using System.Runtime.InteropServices;

public class GetEnvironmentResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipTitleEnvironment env
	{
		get
		{
			IntPtr environmentResponse_env_get = MothershipApiPINVOKE.GetEnvironmentResponse_env_get(swigCPtr);
			MothershipTitleEnvironment result = ((environmentResponse_env_get == IntPtr.Zero) ? null : new MothershipTitleEnvironment(environmentResponse_env_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetEnvironmentResponse_env_set(swigCPtr, MothershipTitleEnvironment.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetEnvironmentResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetEnvironmentResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetEnvironmentResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetEnvironmentResponse obj)
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
					MothershipApiPINVOKE.delete_GetEnvironmentResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool environmentResponse_ParseFromResponseString = MothershipApiPINVOKE.GetEnvironmentResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return environmentResponse_ParseFromResponseString;
	}

	public static GetEnvironmentResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr environmentResponse_FromMothershipResponse = MothershipApiPINVOKE.GetEnvironmentResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetEnvironmentResponse result = ((environmentResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetEnvironmentResponse(environmentResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetEnvironmentResponse()
		: this(MothershipApiPINVOKE.new_GetEnvironmentResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

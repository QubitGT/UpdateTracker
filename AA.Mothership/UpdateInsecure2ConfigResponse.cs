using System;
using System.Runtime.InteropServices;

public class UpdateInsecure2ConfigResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipTitleEnvironment env
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UpdateInsecure2ConfigResponse_env_get(swigCPtr);
			MothershipTitleEnvironment result = ((intPtr == IntPtr.Zero) ? null : new MothershipTitleEnvironment(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UpdateInsecure2ConfigResponse_env_set(swigCPtr, MothershipTitleEnvironment.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal UpdateInsecure2ConfigResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.UpdateInsecure2ConfigResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(UpdateInsecure2ConfigResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(UpdateInsecure2ConfigResponse obj)
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
					MothershipApiPINVOKE.delete_UpdateInsecure2ConfigResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.UpdateInsecure2ConfigResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static UpdateInsecure2ConfigResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.UpdateInsecure2ConfigResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		UpdateInsecure2ConfigResponse result = ((intPtr == IntPtr.Zero) ? null : new UpdateInsecure2ConfigResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public UpdateInsecure2ConfigResponse()
		: this(MothershipApiPINVOKE.new_UpdateInsecure2ConfigResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

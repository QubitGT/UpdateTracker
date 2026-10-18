using System;
using System.Runtime.InteropServices;

public class ExplicitAccountLinkResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public PlayerIdentityVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.ExplicitAccountLinkResponse_Results_get(swigCPtr);
			PlayerIdentityVector result = ((intPtr == IntPtr.Zero) ? null : new PlayerIdentityVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.ExplicitAccountLinkResponse_Results_set(swigCPtr, PlayerIdentityVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal ExplicitAccountLinkResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.ExplicitAccountLinkResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(ExplicitAccountLinkResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(ExplicitAccountLinkResponse obj)
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
					MothershipApiPINVOKE.delete_ExplicitAccountLinkResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.ExplicitAccountLinkResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static ExplicitAccountLinkResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.ExplicitAccountLinkResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		ExplicitAccountLinkResponse result = ((intPtr == IntPtr.Zero) ? null : new ExplicitAccountLinkResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ExplicitAccountLinkResponse()
		: this(MothershipApiPINVOKE.new_ExplicitAccountLinkResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

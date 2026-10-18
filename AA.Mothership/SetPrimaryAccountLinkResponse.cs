using System;
using System.Runtime.InteropServices;

public class SetPrimaryAccountLinkResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string links_name = MothershipApiPINVOKE.SetPrimaryAccountLinkResponse_links_name_get();

	public AccountLinksVector Links
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.SetPrimaryAccountLinkResponse_Links_get(swigCPtr);
			AccountLinksVector result = ((intPtr == IntPtr.Zero) ? null : new AccountLinksVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.SetPrimaryAccountLinkResponse_Links_set(swigCPtr, AccountLinksVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal SetPrimaryAccountLinkResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.SetPrimaryAccountLinkResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(SetPrimaryAccountLinkResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(SetPrimaryAccountLinkResponse obj)
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
					MothershipApiPINVOKE.delete_SetPrimaryAccountLinkResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.SetPrimaryAccountLinkResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static SetPrimaryAccountLinkResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.SetPrimaryAccountLinkResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		SetPrimaryAccountLinkResponse result = ((intPtr == IntPtr.Zero) ? null : new SetPrimaryAccountLinkResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public SetPrimaryAccountLinkResponse()
		: this(MothershipApiPINVOKE.new_SetPrimaryAccountLinkResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

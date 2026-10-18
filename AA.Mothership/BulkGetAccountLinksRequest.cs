using System;
using System.Runtime.InteropServices;

public class BulkGetAccountLinksRequest : MothershipRequest
{
	private HandleRef swigCPtr;

	public AccountLinkLookupVector Lookups
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.BulkGetAccountLinksRequest_Lookups_get(swigCPtr);
			AccountLinkLookupVector result = ((intPtr == IntPtr.Zero) ? null : new AccountLinkLookupVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.BulkGetAccountLinksRequest_Lookups_set(swigCPtr, AccountLinkLookupVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal BulkGetAccountLinksRequest(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.BulkGetAccountLinksRequest_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(BulkGetAccountLinksRequest obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(BulkGetAccountLinksRequest obj)
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
					MothershipApiPINVOKE.delete_BulkGetAccountLinksRequest(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t ToHttpRequest()
	{
		SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t result = new SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t(MothershipApiPINVOKE.BulkGetAccountLinksRequest_ToHttpRequest(swigCPtr), futureUse: true);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public BulkGetAccountLinksRequest()
		: this(MothershipApiPINVOKE.new_BulkGetAccountLinksRequest(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

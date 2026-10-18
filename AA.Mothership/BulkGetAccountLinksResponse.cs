using System;
using System.Runtime.InteropServices;

public class BulkGetAccountLinksResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public AccountLinksVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.BulkGetAccountLinksResponse_Results_get(swigCPtr);
			AccountLinksVector result = ((intPtr == IntPtr.Zero) ? null : new AccountLinksVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.BulkGetAccountLinksResponse_Results_set(swigCPtr, AccountLinksVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal BulkGetAccountLinksResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.BulkGetAccountLinksResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(BulkGetAccountLinksResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(BulkGetAccountLinksResponse obj)
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
					MothershipApiPINVOKE.delete_BulkGetAccountLinksResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.BulkGetAccountLinksResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static BulkGetAccountLinksResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.BulkGetAccountLinksResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		BulkGetAccountLinksResponse result = ((intPtr == IntPtr.Zero) ? null : new BulkGetAccountLinksResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public BulkGetAccountLinksResponse()
		: this(MothershipApiPINVOKE.new_BulkGetAccountLinksResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

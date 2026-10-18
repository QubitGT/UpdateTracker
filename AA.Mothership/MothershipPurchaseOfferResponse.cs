using System;
using System.Runtime.InteropServices;

public class MothershipPurchaseOfferResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public PurchaseChangesMap Changes
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.MothershipPurchaseOfferResponse_Changes_get(swigCPtr);
			PurchaseChangesMap result = ((intPtr == IntPtr.Zero) ? null : new PurchaseChangesMap(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MothershipPurchaseOfferResponse_Changes_set(swigCPtr, PurchaseChangesMap.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal MothershipPurchaseOfferResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.MothershipPurchaseOfferResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipPurchaseOfferResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipPurchaseOfferResponse obj)
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
					MothershipApiPINVOKE.delete_MothershipPurchaseOfferResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.MothershipPurchaseOfferResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static MothershipPurchaseOfferResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.MothershipPurchaseOfferResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		MothershipPurchaseOfferResponse result = ((intPtr == IntPtr.Zero) ? null : new MothershipPurchaseOfferResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public MothershipPurchaseOfferResponse()
		: this(MothershipApiPINVOKE.new_MothershipPurchaseOfferResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

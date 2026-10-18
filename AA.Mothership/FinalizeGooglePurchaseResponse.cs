using System;
using System.Runtime.InteropServices;

public class FinalizeGooglePurchaseResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public PurchaseResultsVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.FinalizeGooglePurchaseResponse_Results_get(swigCPtr);
			PurchaseResultsVector result = ((intPtr == IntPtr.Zero) ? null : new PurchaseResultsVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.FinalizeGooglePurchaseResponse_Results_set(swigCPtr, PurchaseResultsVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal FinalizeGooglePurchaseResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.FinalizeGooglePurchaseResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(FinalizeGooglePurchaseResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(FinalizeGooglePurchaseResponse obj)
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
					MothershipApiPINVOKE.delete_FinalizeGooglePurchaseResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.FinalizeGooglePurchaseResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static FinalizeGooglePurchaseResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.FinalizeGooglePurchaseResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		FinalizeGooglePurchaseResponse result = ((intPtr == IntPtr.Zero) ? null : new FinalizeGooglePurchaseResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public FinalizeGooglePurchaseResponse()
		: this(MothershipApiPINVOKE.new_FinalizeGooglePurchaseResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

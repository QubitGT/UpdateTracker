using System;
using System.Runtime.InteropServices;

public class FinalizeSteamPurchaseResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public PurchaseResultsVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.FinalizeSteamPurchaseResponse_Results_get(swigCPtr);
			PurchaseResultsVector result = ((intPtr == IntPtr.Zero) ? null : new PurchaseResultsVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.FinalizeSteamPurchaseResponse_Results_set(swigCPtr, PurchaseResultsVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal FinalizeSteamPurchaseResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.FinalizeSteamPurchaseResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(FinalizeSteamPurchaseResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(FinalizeSteamPurchaseResponse obj)
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
					MothershipApiPINVOKE.delete_FinalizeSteamPurchaseResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.FinalizeSteamPurchaseResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static FinalizeSteamPurchaseResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.FinalizeSteamPurchaseResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		FinalizeSteamPurchaseResponse result = ((intPtr == IntPtr.Zero) ? null : new FinalizeSteamPurchaseResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public FinalizeSteamPurchaseResponse()
		: this(MothershipApiPINVOKE.new_FinalizeSteamPurchaseResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

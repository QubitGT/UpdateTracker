using System;
using System.Runtime.InteropServices;

public class MothershipRunTransactionResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public PurchaseResultsVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.MothershipRunTransactionResponse_Results_get(swigCPtr);
			PurchaseResultsVector result = ((intPtr == IntPtr.Zero) ? null : new PurchaseResultsVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MothershipRunTransactionResponse_Results_set(swigCPtr, PurchaseResultsVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal MothershipRunTransactionResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.MothershipRunTransactionResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipRunTransactionResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipRunTransactionResponse obj)
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
					MothershipApiPINVOKE.delete_MothershipRunTransactionResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.MothershipRunTransactionResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static MothershipRunTransactionResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.MothershipRunTransactionResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		MothershipRunTransactionResponse result = ((intPtr == IntPtr.Zero) ? null : new MothershipRunTransactionResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public MothershipRunTransactionResponse()
		: this(MothershipApiPINVOKE.new_MothershipRunTransactionResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

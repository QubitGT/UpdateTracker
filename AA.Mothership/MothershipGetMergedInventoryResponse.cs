using System;
using System.Runtime.InteropServices;

public class MothershipGetMergedInventoryResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public InventoryItemSummaryVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.MothershipGetMergedInventoryResponse_Results_get(swigCPtr);
			InventoryItemSummaryVector result = ((intPtr == IntPtr.Zero) ? null : new InventoryItemSummaryVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MothershipGetMergedInventoryResponse_Results_set(swigCPtr, InventoryItemSummaryVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal MothershipGetMergedInventoryResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.MothershipGetMergedInventoryResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipGetMergedInventoryResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipGetMergedInventoryResponse obj)
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
					MothershipApiPINVOKE.delete_MothershipGetMergedInventoryResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.MothershipGetMergedInventoryResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static MothershipGetMergedInventoryResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.MothershipGetMergedInventoryResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		MothershipGetMergedInventoryResponse result = ((intPtr == IntPtr.Zero) ? null : new MothershipGetMergedInventoryResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public MothershipGetMergedInventoryResponse()
		: this(MothershipApiPINVOKE.new_MothershipGetMergedInventoryResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

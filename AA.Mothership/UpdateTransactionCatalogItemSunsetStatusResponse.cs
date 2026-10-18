using System;
using System.Runtime.InteropServices;

public class UpdateTransactionCatalogItemSunsetStatusResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipTransactionCatalogItem item
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UpdateTransactionCatalogItemSunsetStatusResponse_item_get(swigCPtr);
			MothershipTransactionCatalogItem result = ((intPtr == IntPtr.Zero) ? null : new MothershipTransactionCatalogItem(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UpdateTransactionCatalogItemSunsetStatusResponse_item_set(swigCPtr, MothershipTransactionCatalogItem.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal UpdateTransactionCatalogItemSunsetStatusResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.UpdateTransactionCatalogItemSunsetStatusResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(UpdateTransactionCatalogItemSunsetStatusResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(UpdateTransactionCatalogItemSunsetStatusResponse obj)
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
					MothershipApiPINVOKE.delete_UpdateTransactionCatalogItemSunsetStatusResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.UpdateTransactionCatalogItemSunsetStatusResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static UpdateTransactionCatalogItemSunsetStatusResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.UpdateTransactionCatalogItemSunsetStatusResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		UpdateTransactionCatalogItemSunsetStatusResponse result = ((intPtr == IntPtr.Zero) ? null : new UpdateTransactionCatalogItemSunsetStatusResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public UpdateTransactionCatalogItemSunsetStatusResponse()
		: this(MothershipApiPINVOKE.new_UpdateTransactionCatalogItemSunsetStatusResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

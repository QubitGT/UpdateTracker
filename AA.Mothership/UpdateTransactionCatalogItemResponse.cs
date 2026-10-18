using System;
using System.Runtime.InteropServices;

public class UpdateTransactionCatalogItemResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipTransactionCatalogItem item
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UpdateTransactionCatalogItemResponse_item_get(swigCPtr);
			MothershipTransactionCatalogItem result = ((intPtr == IntPtr.Zero) ? null : new MothershipTransactionCatalogItem(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UpdateTransactionCatalogItemResponse_item_set(swigCPtr, MothershipTransactionCatalogItem.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal UpdateTransactionCatalogItemResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.UpdateTransactionCatalogItemResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(UpdateTransactionCatalogItemResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(UpdateTransactionCatalogItemResponse obj)
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
					MothershipApiPINVOKE.delete_UpdateTransactionCatalogItemResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.UpdateTransactionCatalogItemResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static UpdateTransactionCatalogItemResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.UpdateTransactionCatalogItemResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		UpdateTransactionCatalogItemResponse result = ((intPtr == IntPtr.Zero) ? null : new UpdateTransactionCatalogItemResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public UpdateTransactionCatalogItemResponse()
		: this(MothershipApiPINVOKE.new_UpdateTransactionCatalogItemResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

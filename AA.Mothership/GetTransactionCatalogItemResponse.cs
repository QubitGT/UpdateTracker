using System;
using System.Runtime.InteropServices;

public class GetTransactionCatalogItemResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipTransactionCatalogItem item
	{
		get
		{
			IntPtr transactionCatalogItemResponse_item_get = MothershipApiPINVOKE.GetTransactionCatalogItemResponse_item_get(swigCPtr);
			MothershipTransactionCatalogItem result = ((transactionCatalogItemResponse_item_get == IntPtr.Zero) ? null : new MothershipTransactionCatalogItem(transactionCatalogItemResponse_item_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetTransactionCatalogItemResponse_item_set(swigCPtr, MothershipTransactionCatalogItem.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetTransactionCatalogItemResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetTransactionCatalogItemResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetTransactionCatalogItemResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetTransactionCatalogItemResponse obj)
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
					MothershipApiPINVOKE.delete_GetTransactionCatalogItemResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool transactionCatalogItemResponse_ParseFromResponseString = MothershipApiPINVOKE.GetTransactionCatalogItemResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return transactionCatalogItemResponse_ParseFromResponseString;
	}

	public static GetTransactionCatalogItemResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr transactionCatalogItemResponse_FromMothershipResponse = MothershipApiPINVOKE.GetTransactionCatalogItemResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetTransactionCatalogItemResponse result = ((transactionCatalogItemResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetTransactionCatalogItemResponse(transactionCatalogItemResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetTransactionCatalogItemResponse()
		: this(MothershipApiPINVOKE.new_GetTransactionCatalogItemResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

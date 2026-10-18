using System;
using System.Runtime.InteropServices;

public class ListEntitlementCatalogItemsResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public ListEntitlementResultsVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.ListEntitlementCatalogItemsResponse_Results_get(swigCPtr);
			ListEntitlementResultsVector result = ((intPtr == IntPtr.Zero) ? null : new ListEntitlementResultsVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.ListEntitlementCatalogItemsResponse_Results_set(swigCPtr, ListEntitlementResultsVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal ListEntitlementCatalogItemsResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.ListEntitlementCatalogItemsResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(ListEntitlementCatalogItemsResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(ListEntitlementCatalogItemsResponse obj)
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
					MothershipApiPINVOKE.delete_ListEntitlementCatalogItemsResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.ListEntitlementCatalogItemsResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static ListEntitlementCatalogItemsResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.ListEntitlementCatalogItemsResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		ListEntitlementCatalogItemsResponse result = ((intPtr == IntPtr.Zero) ? null : new ListEntitlementCatalogItemsResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ListEntitlementCatalogItemsResponse()
		: this(MothershipApiPINVOKE.new_ListEntitlementCatalogItemsResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

using System;
using System.Runtime.InteropServices;

public class GetEntitlementCatalogItemResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipEntitlementCatalogItem result
	{
		get
		{
			IntPtr entitlementCatalogItemResponse_result_get = MothershipApiPINVOKE.GetEntitlementCatalogItemResponse_result_get(swigCPtr);
			MothershipEntitlementCatalogItem obj = ((entitlementCatalogItemResponse_result_get == IntPtr.Zero) ? null : new MothershipEntitlementCatalogItem(entitlementCatalogItemResponse_result_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return obj;
		}
		set
		{
			MothershipApiPINVOKE.GetEntitlementCatalogItemResponse_result_set(swigCPtr, MothershipEntitlementCatalogItem.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetEntitlementCatalogItemResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetEntitlementCatalogItemResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetEntitlementCatalogItemResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetEntitlementCatalogItemResponse obj)
	{
		if (obj != null)
		{
			if (!obj.swigCMemOwn)
			{
				throw new ApplicationException("Cannot release ownership as memory is not owned");
			}
			HandleRef handleRef = obj.swigCPtr;
			obj.swigCMemOwn = false;
			obj.Dispose();
			return handleRef;
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
					MothershipApiPINVOKE.delete_GetEntitlementCatalogItemResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool entitlementCatalogItemResponse_ParseFromResponseString = MothershipApiPINVOKE.GetEntitlementCatalogItemResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return entitlementCatalogItemResponse_ParseFromResponseString;
	}

	public static GetEntitlementCatalogItemResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr entitlementCatalogItemResponse_FromMothershipResponse = MothershipApiPINVOKE.GetEntitlementCatalogItemResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetEntitlementCatalogItemResponse obj = ((entitlementCatalogItemResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetEntitlementCatalogItemResponse(entitlementCatalogItemResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return obj;
	}

	public GetEntitlementCatalogItemResponse()
		: this(MothershipApiPINVOKE.new_GetEntitlementCatalogItemResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

using System;
using System.Runtime.InteropServices;

public class UpdateEntitlementCatalogItemResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipEntitlementCatalogItem result
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UpdateEntitlementCatalogItemResponse_result_get(swigCPtr);
			MothershipEntitlementCatalogItem obj = ((intPtr == IntPtr.Zero) ? null : new MothershipEntitlementCatalogItem(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return obj;
		}
		set
		{
			MothershipApiPINVOKE.UpdateEntitlementCatalogItemResponse_result_set(swigCPtr, MothershipEntitlementCatalogItem.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal UpdateEntitlementCatalogItemResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.UpdateEntitlementCatalogItemResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(UpdateEntitlementCatalogItemResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(UpdateEntitlementCatalogItemResponse obj)
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
					MothershipApiPINVOKE.delete_UpdateEntitlementCatalogItemResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool num = MothershipApiPINVOKE.UpdateEntitlementCatalogItemResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return num;
	}

	public static UpdateEntitlementCatalogItemResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.UpdateEntitlementCatalogItemResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		UpdateEntitlementCatalogItemResponse obj = ((intPtr == IntPtr.Zero) ? null : new UpdateEntitlementCatalogItemResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return obj;
	}

	public UpdateEntitlementCatalogItemResponse()
		: this(MothershipApiPINVOKE.new_UpdateEntitlementCatalogItemResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

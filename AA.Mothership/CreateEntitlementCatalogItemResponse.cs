using System;
using System.Runtime.InteropServices;

public class CreateEntitlementCatalogItemResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipEntitlementCatalogItem catalogItem
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.CreateEntitlementCatalogItemResponse_catalogItem_get(swigCPtr);
			MothershipEntitlementCatalogItem result = ((intPtr == IntPtr.Zero) ? null : new MothershipEntitlementCatalogItem(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.CreateEntitlementCatalogItemResponse_catalogItem_set(swigCPtr, MothershipEntitlementCatalogItem.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal CreateEntitlementCatalogItemResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.CreateEntitlementCatalogItemResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(CreateEntitlementCatalogItemResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(CreateEntitlementCatalogItemResponse obj)
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
					MothershipApiPINVOKE.delete_CreateEntitlementCatalogItemResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.CreateEntitlementCatalogItemResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static CreateEntitlementCatalogItemResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.CreateEntitlementCatalogItemResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		CreateEntitlementCatalogItemResponse result = ((intPtr == IntPtr.Zero) ? null : new CreateEntitlementCatalogItemResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public CreateEntitlementCatalogItemResponse()
		: this(MothershipApiPINVOKE.new_CreateEntitlementCatalogItemResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

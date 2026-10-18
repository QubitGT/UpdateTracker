using System;
using System.Runtime.InteropServices;

public class GetEntitlementCatalogItemRequest : MothershipRequest
{
	private HandleRef swigCPtr;

	public string titleId
	{
		get
		{
			string entitlementCatalogItemRequest_titleId_get = MothershipApiPINVOKE.GetEntitlementCatalogItemRequest_titleId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return entitlementCatalogItemRequest_titleId_get;
		}
		set
		{
			MothershipApiPINVOKE.GetEntitlementCatalogItemRequest_titleId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string envId
	{
		get
		{
			string entitlementCatalogItemRequest_envId_get = MothershipApiPINVOKE.GetEntitlementCatalogItemRequest_envId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return entitlementCatalogItemRequest_envId_get;
		}
		set
		{
			MothershipApiPINVOKE.GetEntitlementCatalogItemRequest_envId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string entitlementId
	{
		get
		{
			string entitlementCatalogItemRequest_entitlementId_get = MothershipApiPINVOKE.GetEntitlementCatalogItemRequest_entitlementId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return entitlementCatalogItemRequest_entitlementId_get;
		}
		set
		{
			MothershipApiPINVOKE.GetEntitlementCatalogItemRequest_entitlementId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetEntitlementCatalogItemRequest(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetEntitlementCatalogItemRequest_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetEntitlementCatalogItemRequest obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetEntitlementCatalogItemRequest obj)
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
					MothershipApiPINVOKE.delete_GetEntitlementCatalogItemRequest(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t ToHttpRequest()
	{
		SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t result = new SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t(MothershipApiPINVOKE.GetEntitlementCatalogItemRequest_ToHttpRequest(swigCPtr), futureUse: true);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetEntitlementCatalogItemRequest()
		: this(MothershipApiPINVOKE.new_GetEntitlementCatalogItemRequest(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

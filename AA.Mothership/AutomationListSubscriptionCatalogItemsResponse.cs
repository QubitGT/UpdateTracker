using System;
using System.Runtime.InteropServices;

public class AutomationListSubscriptionCatalogItemsResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public SubscriptionCatalogItemVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.AutomationListSubscriptionCatalogItemsResponse_Results_get(swigCPtr);
			SubscriptionCatalogItemVector result = ((intPtr == IntPtr.Zero) ? null : new SubscriptionCatalogItemVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.AutomationListSubscriptionCatalogItemsResponse_Results_set(swigCPtr, SubscriptionCatalogItemVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal AutomationListSubscriptionCatalogItemsResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.AutomationListSubscriptionCatalogItemsResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(AutomationListSubscriptionCatalogItemsResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(AutomationListSubscriptionCatalogItemsResponse obj)
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
					MothershipApiPINVOKE.delete_AutomationListSubscriptionCatalogItemsResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.AutomationListSubscriptionCatalogItemsResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static AutomationListSubscriptionCatalogItemsResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.AutomationListSubscriptionCatalogItemsResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		AutomationListSubscriptionCatalogItemsResponse result = ((intPtr == IntPtr.Zero) ? null : new AutomationListSubscriptionCatalogItemsResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public AutomationListSubscriptionCatalogItemsResponse()
		: this(MothershipApiPINVOKE.new_AutomationListSubscriptionCatalogItemsResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

using System;
using System.Runtime.InteropServices;

public class AutomationCreateSubscriptionCatalogItemResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	internal AutomationCreateSubscriptionCatalogItemResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.AutomationCreateSubscriptionCatalogItemResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(AutomationCreateSubscriptionCatalogItemResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(AutomationCreateSubscriptionCatalogItemResponse obj)
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
					MothershipApiPINVOKE.delete_AutomationCreateSubscriptionCatalogItemResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.AutomationCreateSubscriptionCatalogItemResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static AutomationCreateSubscriptionCatalogItemResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.AutomationCreateSubscriptionCatalogItemResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		AutomationCreateSubscriptionCatalogItemResponse result = ((intPtr == IntPtr.Zero) ? null : new AutomationCreateSubscriptionCatalogItemResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public AutomationCreateSubscriptionCatalogItemResponse()
		: this(MothershipApiPINVOKE.new_AutomationCreateSubscriptionCatalogItemResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

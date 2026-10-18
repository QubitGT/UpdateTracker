using System;
using System.Runtime.InteropServices;

public class AutomationGetSubscriptionsResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public SubscriptionsVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.AutomationGetSubscriptionsResponse_Results_get(swigCPtr);
			SubscriptionsVector result = ((intPtr == IntPtr.Zero) ? null : new SubscriptionsVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.AutomationGetSubscriptionsResponse_Results_set(swigCPtr, SubscriptionsVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal AutomationGetSubscriptionsResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.AutomationGetSubscriptionsResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(AutomationGetSubscriptionsResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(AutomationGetSubscriptionsResponse obj)
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
					MothershipApiPINVOKE.delete_AutomationGetSubscriptionsResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.AutomationGetSubscriptionsResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static AutomationGetSubscriptionsResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.AutomationGetSubscriptionsResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		AutomationGetSubscriptionsResponse result = ((intPtr == IntPtr.Zero) ? null : new AutomationGetSubscriptionsResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public AutomationGetSubscriptionsResponse()
		: this(MothershipApiPINVOKE.new_AutomationGetSubscriptionsResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

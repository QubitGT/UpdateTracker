using System;
using System.Runtime.InteropServices;

public class GetMySubscriptionsResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public SubscriptionsVector Results
	{
		get
		{
			IntPtr mySubscriptionsResponse_Results_get = MothershipApiPINVOKE.GetMySubscriptionsResponse_Results_get(swigCPtr);
			SubscriptionsVector result = ((mySubscriptionsResponse_Results_get == IntPtr.Zero) ? null : new SubscriptionsVector(mySubscriptionsResponse_Results_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetMySubscriptionsResponse_Results_set(swigCPtr, SubscriptionsVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetMySubscriptionsResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetMySubscriptionsResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetMySubscriptionsResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetMySubscriptionsResponse obj)
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
					MothershipApiPINVOKE.delete_GetMySubscriptionsResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool mySubscriptionsResponse_ParseFromResponseString = MothershipApiPINVOKE.GetMySubscriptionsResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return mySubscriptionsResponse_ParseFromResponseString;
	}

	public static GetMySubscriptionsResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr mySubscriptionsResponse_FromMothershipResponse = MothershipApiPINVOKE.GetMySubscriptionsResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetMySubscriptionsResponse result = ((mySubscriptionsResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetMySubscriptionsResponse(mySubscriptionsResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetMySubscriptionsResponse()
		: this(MothershipApiPINVOKE.new_GetMySubscriptionsResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

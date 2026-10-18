using System;
using System.Runtime.InteropServices;

public class ServerRefreshSubscriptionsForPlayerResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public SubscriptionsVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.ServerRefreshSubscriptionsForPlayerResponse_Results_get(swigCPtr);
			SubscriptionsVector result = ((intPtr == IntPtr.Zero) ? null : new SubscriptionsVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.ServerRefreshSubscriptionsForPlayerResponse_Results_set(swigCPtr, SubscriptionsVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal ServerRefreshSubscriptionsForPlayerResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.ServerRefreshSubscriptionsForPlayerResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(ServerRefreshSubscriptionsForPlayerResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(ServerRefreshSubscriptionsForPlayerResponse obj)
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
					MothershipApiPINVOKE.delete_ServerRefreshSubscriptionsForPlayerResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.ServerRefreshSubscriptionsForPlayerResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static ServerRefreshSubscriptionsForPlayerResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.ServerRefreshSubscriptionsForPlayerResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		ServerRefreshSubscriptionsForPlayerResponse result = ((intPtr == IntPtr.Zero) ? null : new ServerRefreshSubscriptionsForPlayerResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ServerRefreshSubscriptionsForPlayerResponse()
		: this(MothershipApiPINVOKE.new_ServerRefreshSubscriptionsForPlayerResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

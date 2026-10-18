using System;
using System.Runtime.InteropServices;

public class FinalizeSteamSubscriptionPurchaseResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public string SteamOrderId
	{
		get
		{
			string result = MothershipApiPINVOKE.FinalizeSteamSubscriptionPurchaseResponse_SteamOrderId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.FinalizeSteamSubscriptionPurchaseResponse_SteamOrderId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string SteamTransactionId
	{
		get
		{
			string result = MothershipApiPINVOKE.FinalizeSteamSubscriptionPurchaseResponse_SteamTransactionId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.FinalizeSteamSubscriptionPurchaseResponse_SteamTransactionId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal FinalizeSteamSubscriptionPurchaseResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.FinalizeSteamSubscriptionPurchaseResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(FinalizeSteamSubscriptionPurchaseResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(FinalizeSteamSubscriptionPurchaseResponse obj)
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
					MothershipApiPINVOKE.delete_FinalizeSteamSubscriptionPurchaseResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.FinalizeSteamSubscriptionPurchaseResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static FinalizeSteamSubscriptionPurchaseResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.FinalizeSteamSubscriptionPurchaseResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		FinalizeSteamSubscriptionPurchaseResponse result = ((intPtr == IntPtr.Zero) ? null : new FinalizeSteamSubscriptionPurchaseResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public FinalizeSteamSubscriptionPurchaseResponse()
		: this(MothershipApiPINVOKE.new_FinalizeSteamSubscriptionPurchaseResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

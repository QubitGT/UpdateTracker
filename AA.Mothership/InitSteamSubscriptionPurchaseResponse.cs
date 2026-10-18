using System;
using System.Runtime.InteropServices;

public class InitSteamSubscriptionPurchaseResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public string SteamOrderId
	{
		get
		{
			string result = MothershipApiPINVOKE.InitSteamSubscriptionPurchaseResponse_SteamOrderId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.InitSteamSubscriptionPurchaseResponse_SteamOrderId_set(swigCPtr, value);
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
			string result = MothershipApiPINVOKE.InitSteamSubscriptionPurchaseResponse_SteamTransactionId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.InitSteamSubscriptionPurchaseResponse_SteamTransactionId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal InitSteamSubscriptionPurchaseResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.InitSteamSubscriptionPurchaseResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(InitSteamSubscriptionPurchaseResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(InitSteamSubscriptionPurchaseResponse obj)
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
					MothershipApiPINVOKE.delete_InitSteamSubscriptionPurchaseResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.InitSteamSubscriptionPurchaseResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static InitSteamSubscriptionPurchaseResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.InitSteamSubscriptionPurchaseResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		InitSteamSubscriptionPurchaseResponse result = ((intPtr == IntPtr.Zero) ? null : new InitSteamSubscriptionPurchaseResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public InitSteamSubscriptionPurchaseResponse()
		: this(MothershipApiPINVOKE.new_InitSteamSubscriptionPurchaseResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

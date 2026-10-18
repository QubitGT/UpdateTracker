using System;
using System.Runtime.InteropServices;

public class PlayerQuestBeginLoginV2Response : MothershipResponse
{
	private HandleRef swigCPtr;

	public string AttestationNonce
	{
		get
		{
			string result = MothershipApiPINVOKE.PlayerQuestBeginLoginV2Response_AttestationNonce_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.PlayerQuestBeginLoginV2Response_AttestationNonce_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal PlayerQuestBeginLoginV2Response(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.PlayerQuestBeginLoginV2Response_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(PlayerQuestBeginLoginV2Response obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(PlayerQuestBeginLoginV2Response obj)
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
					MothershipApiPINVOKE.delete_PlayerQuestBeginLoginV2Response(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.PlayerQuestBeginLoginV2Response_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static PlayerQuestBeginLoginV2Response FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.PlayerQuestBeginLoginV2Response_FromMothershipResponse(MothershipResponse.getCPtr(response));
		PlayerQuestBeginLoginV2Response result = ((intPtr == IntPtr.Zero) ? null : new PlayerQuestBeginLoginV2Response(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public PlayerQuestBeginLoginV2Response()
		: this(MothershipApiPINVOKE.new_PlayerQuestBeginLoginV2Response(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

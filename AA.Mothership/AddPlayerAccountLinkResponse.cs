using System;
using System.Runtime.InteropServices;

public class AddPlayerAccountLinkResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string identities_name = MothershipApiPINVOKE.AddPlayerAccountLinkResponse_identities_name_get();

	public PlayerIdentityVector Identities
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.AddPlayerAccountLinkResponse_Identities_get(swigCPtr);
			PlayerIdentityVector result = ((intPtr == IntPtr.Zero) ? null : new PlayerIdentityVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.AddPlayerAccountLinkResponse_Identities_set(swigCPtr, PlayerIdentityVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal AddPlayerAccountLinkResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.AddPlayerAccountLinkResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(AddPlayerAccountLinkResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(AddPlayerAccountLinkResponse obj)
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
					MothershipApiPINVOKE.delete_AddPlayerAccountLinkResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.AddPlayerAccountLinkResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static AddPlayerAccountLinkResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.AddPlayerAccountLinkResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		AddPlayerAccountLinkResponse result = ((intPtr == IntPtr.Zero) ? null : new AddPlayerAccountLinkResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public AddPlayerAccountLinkResponse()
		: this(MothershipApiPINVOKE.new_AddPlayerAccountLinkResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

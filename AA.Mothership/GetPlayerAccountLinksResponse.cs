using System;
using System.Runtime.InteropServices;

public class GetPlayerAccountLinksResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string identities_name = MothershipApiPINVOKE.GetPlayerAccountLinksResponse_identities_name_get();

	public PlayerIdentityVector Identities
	{
		get
		{
			IntPtr playerAccountLinksResponse_Identities_get = MothershipApiPINVOKE.GetPlayerAccountLinksResponse_Identities_get(swigCPtr);
			PlayerIdentityVector result = ((playerAccountLinksResponse_Identities_get == IntPtr.Zero) ? null : new PlayerIdentityVector(playerAccountLinksResponse_Identities_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetPlayerAccountLinksResponse_Identities_set(swigCPtr, PlayerIdentityVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetPlayerAccountLinksResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetPlayerAccountLinksResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetPlayerAccountLinksResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetPlayerAccountLinksResponse obj)
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
					MothershipApiPINVOKE.delete_GetPlayerAccountLinksResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool playerAccountLinksResponse_ParseFromResponseString = MothershipApiPINVOKE.GetPlayerAccountLinksResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return playerAccountLinksResponse_ParseFromResponseString;
	}

	public static GetPlayerAccountLinksResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr playerAccountLinksResponse_FromMothershipResponse = MothershipApiPINVOKE.GetPlayerAccountLinksResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetPlayerAccountLinksResponse result = ((playerAccountLinksResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetPlayerAccountLinksResponse(playerAccountLinksResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetPlayerAccountLinksResponse()
		: this(MothershipApiPINVOKE.new_GetPlayerAccountLinksResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

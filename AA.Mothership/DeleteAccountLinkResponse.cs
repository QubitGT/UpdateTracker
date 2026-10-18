using System;
using System.Runtime.InteropServices;

public class DeleteAccountLinkResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string links_name = MothershipApiPINVOKE.DeleteAccountLinkResponse_links_name_get();

	public AccountLinksVector Links
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.DeleteAccountLinkResponse_Links_get(swigCPtr);
			AccountLinksVector result = ((intPtr == IntPtr.Zero) ? null : new AccountLinksVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.DeleteAccountLinkResponse_Links_set(swigCPtr, AccountLinksVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal DeleteAccountLinkResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.DeleteAccountLinkResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(DeleteAccountLinkResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(DeleteAccountLinkResponse obj)
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
					MothershipApiPINVOKE.delete_DeleteAccountLinkResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.DeleteAccountLinkResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static DeleteAccountLinkResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.DeleteAccountLinkResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		DeleteAccountLinkResponse result = ((intPtr == IntPtr.Zero) ? null : new DeleteAccountLinkResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public DeleteAccountLinkResponse()
		: this(MothershipApiPINVOKE.new_DeleteAccountLinkResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

using System;
using System.Runtime.InteropServices;

public class ChangeCommitStatusOfOfferBindingsResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string Results_name = MothershipApiPINVOKE.ChangeCommitStatusOfOfferBindingsResponse_Results_name_get();

	public OfferBindingVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.ChangeCommitStatusOfOfferBindingsResponse_Results_get(swigCPtr);
			OfferBindingVector result = ((intPtr == IntPtr.Zero) ? null : new OfferBindingVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.ChangeCommitStatusOfOfferBindingsResponse_Results_set(swigCPtr, OfferBindingVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal ChangeCommitStatusOfOfferBindingsResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.ChangeCommitStatusOfOfferBindingsResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(ChangeCommitStatusOfOfferBindingsResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(ChangeCommitStatusOfOfferBindingsResponse obj)
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
					MothershipApiPINVOKE.delete_ChangeCommitStatusOfOfferBindingsResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.ChangeCommitStatusOfOfferBindingsResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static ChangeCommitStatusOfOfferBindingsResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.ChangeCommitStatusOfOfferBindingsResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		ChangeCommitStatusOfOfferBindingsResponse result = ((intPtr == IntPtr.Zero) ? null : new ChangeCommitStatusOfOfferBindingsResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ChangeCommitStatusOfOfferBindingsResponse()
		: this(MothershipApiPINVOKE.new_ChangeCommitStatusOfOfferBindingsResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

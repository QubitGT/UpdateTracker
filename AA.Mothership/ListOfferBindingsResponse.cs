using System;
using System.Runtime.InteropServices;

public class ListOfferBindingsResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string Results_name = MothershipApiPINVOKE.ListOfferBindingsResponse_Results_name_get();

	public OfferBindingVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.ListOfferBindingsResponse_Results_get(swigCPtr);
			OfferBindingVector result = ((intPtr == IntPtr.Zero) ? null : new OfferBindingVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.ListOfferBindingsResponse_Results_set(swigCPtr, OfferBindingVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal ListOfferBindingsResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.ListOfferBindingsResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(ListOfferBindingsResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(ListOfferBindingsResponse obj)
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
					MothershipApiPINVOKE.delete_ListOfferBindingsResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.ListOfferBindingsResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static ListOfferBindingsResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.ListOfferBindingsResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		ListOfferBindingsResponse result = ((intPtr == IntPtr.Zero) ? null : new ListOfferBindingsResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ListOfferBindingsResponse()
		: this(MothershipApiPINVOKE.new_ListOfferBindingsResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

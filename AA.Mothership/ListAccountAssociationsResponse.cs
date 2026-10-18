using System;
using System.Runtime.InteropServices;

public class ListAccountAssociationsResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public AccountAssociationVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.ListAccountAssociationsResponse_Results_get(swigCPtr);
			AccountAssociationVector result = ((intPtr == IntPtr.Zero) ? null : new AccountAssociationVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.ListAccountAssociationsResponse_Results_set(swigCPtr, AccountAssociationVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal ListAccountAssociationsResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.ListAccountAssociationsResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(ListAccountAssociationsResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(ListAccountAssociationsResponse obj)
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
					MothershipApiPINVOKE.delete_ListAccountAssociationsResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.ListAccountAssociationsResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static ListAccountAssociationsResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.ListAccountAssociationsResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		ListAccountAssociationsResponse result = ((intPtr == IntPtr.Zero) ? null : new ListAccountAssociationsResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ListAccountAssociationsResponse()
		: this(MothershipApiPINVOKE.new_ListAccountAssociationsResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

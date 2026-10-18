using System;
using System.Runtime.InteropServices;

public class ListProgressionTreeResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string Results_name = MothershipApiPINVOKE.ListProgressionTreeResponse_Results_name_get();

	public SWIGTYPE_p_std__vectorT_MothershipApiShared__HydratedProgressionTreeResponse_t Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.ListProgressionTreeResponse_Results_get(swigCPtr);
			SWIGTYPE_p_std__vectorT_MothershipApiShared__HydratedProgressionTreeResponse_t result = ((intPtr == IntPtr.Zero) ? null : new SWIGTYPE_p_std__vectorT_MothershipApiShared__HydratedProgressionTreeResponse_t(intPtr, futureUse: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.ListProgressionTreeResponse_Results_set(swigCPtr, SWIGTYPE_p_std__vectorT_MothershipApiShared__HydratedProgressionTreeResponse_t.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal ListProgressionTreeResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.ListProgressionTreeResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(ListProgressionTreeResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(ListProgressionTreeResponse obj)
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
					MothershipApiPINVOKE.delete_ListProgressionTreeResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.ListProgressionTreeResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static ListProgressionTreeResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.ListProgressionTreeResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		ListProgressionTreeResponse result = ((intPtr == IntPtr.Zero) ? null : new ListProgressionTreeResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ListProgressionTreeResponse()
		: this(MothershipApiPINVOKE.new_ListProgressionTreeResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

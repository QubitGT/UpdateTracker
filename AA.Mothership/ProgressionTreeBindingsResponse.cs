using System;
using System.Runtime.InteropServices;

public class ProgressionTreeBindingsResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string Results_name = MothershipApiPINVOKE.ProgressionTreeBindingsResponse_Results_name_get();

	public ProgressionTreeBindingVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.ProgressionTreeBindingsResponse_Results_get(swigCPtr);
			ProgressionTreeBindingVector result = ((intPtr == IntPtr.Zero) ? null : new ProgressionTreeBindingVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.ProgressionTreeBindingsResponse_Results_set(swigCPtr, ProgressionTreeBindingVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal ProgressionTreeBindingsResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.ProgressionTreeBindingsResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(ProgressionTreeBindingsResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(ProgressionTreeBindingsResponse obj)
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
					MothershipApiPINVOKE.delete_ProgressionTreeBindingsResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.ProgressionTreeBindingsResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static ProgressionTreeBindingsResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.ProgressionTreeBindingsResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		ProgressionTreeBindingsResponse result = ((intPtr == IntPtr.Zero) ? null : new ProgressionTreeBindingsResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ProgressionTreeBindingsResponse()
		: this(MothershipApiPINVOKE.new_ProgressionTreeBindingsResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

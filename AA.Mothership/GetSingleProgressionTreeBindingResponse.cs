using System;
using System.Runtime.InteropServices;

public class GetSingleProgressionTreeBindingResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string Results_name = MothershipApiPINVOKE.GetSingleProgressionTreeBindingResponse_Results_name_get();

	public ProgressionTreeBindingResponse Results
	{
		get
		{
			IntPtr singleProgressionTreeBindingResponse_Results_get = MothershipApiPINVOKE.GetSingleProgressionTreeBindingResponse_Results_get(swigCPtr);
			ProgressionTreeBindingResponse result = ((singleProgressionTreeBindingResponse_Results_get == IntPtr.Zero) ? null : new ProgressionTreeBindingResponse(singleProgressionTreeBindingResponse_Results_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetSingleProgressionTreeBindingResponse_Results_set(swigCPtr, ProgressionTreeBindingResponse.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetSingleProgressionTreeBindingResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetSingleProgressionTreeBindingResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetSingleProgressionTreeBindingResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetSingleProgressionTreeBindingResponse obj)
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
					MothershipApiPINVOKE.delete_GetSingleProgressionTreeBindingResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool singleProgressionTreeBindingResponse_ParseFromResponseString = MothershipApiPINVOKE.GetSingleProgressionTreeBindingResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return singleProgressionTreeBindingResponse_ParseFromResponseString;
	}

	public static GetSingleProgressionTreeBindingResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr singleProgressionTreeBindingResponse_FromMothershipResponse = MothershipApiPINVOKE.GetSingleProgressionTreeBindingResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetSingleProgressionTreeBindingResponse result = ((singleProgressionTreeBindingResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetSingleProgressionTreeBindingResponse(singleProgressionTreeBindingResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetSingleProgressionTreeBindingResponse()
		: this(MothershipApiPINVOKE.new_GetSingleProgressionTreeBindingResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

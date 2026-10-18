using System;
using System.Runtime.InteropServices;

public class GetSingleProgressionTrackBindingResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string Results_name = MothershipApiPINVOKE.GetSingleProgressionTrackBindingResponse_Results_name_get();

	public ProgressionTrackBindingResponse Results
	{
		get
		{
			IntPtr singleProgressionTrackBindingResponse_Results_get = MothershipApiPINVOKE.GetSingleProgressionTrackBindingResponse_Results_get(swigCPtr);
			ProgressionTrackBindingResponse result = ((singleProgressionTrackBindingResponse_Results_get == IntPtr.Zero) ? null : new ProgressionTrackBindingResponse(singleProgressionTrackBindingResponse_Results_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetSingleProgressionTrackBindingResponse_Results_set(swigCPtr, ProgressionTrackBindingResponse.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetSingleProgressionTrackBindingResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetSingleProgressionTrackBindingResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetSingleProgressionTrackBindingResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetSingleProgressionTrackBindingResponse obj)
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
					MothershipApiPINVOKE.delete_GetSingleProgressionTrackBindingResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool singleProgressionTrackBindingResponse_ParseFromResponseString = MothershipApiPINVOKE.GetSingleProgressionTrackBindingResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return singleProgressionTrackBindingResponse_ParseFromResponseString;
	}

	public static GetSingleProgressionTrackBindingResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr singleProgressionTrackBindingResponse_FromMothershipResponse = MothershipApiPINVOKE.GetSingleProgressionTrackBindingResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetSingleProgressionTrackBindingResponse result = ((singleProgressionTrackBindingResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetSingleProgressionTrackBindingResponse(singleProgressionTrackBindingResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetSingleProgressionTrackBindingResponse()
		: this(MothershipApiPINVOKE.new_GetSingleProgressionTrackBindingResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

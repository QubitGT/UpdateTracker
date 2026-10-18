using System;
using System.Runtime.InteropServices;

public class GetProgressionTrackBindingsResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string Results_name = MothershipApiPINVOKE.GetProgressionTrackBindingsResponse_Results_name_get();

	public ProgressionTrackBindingVector Results
	{
		get
		{
			IntPtr progressionTrackBindingsResponse_Results_get = MothershipApiPINVOKE.GetProgressionTrackBindingsResponse_Results_get(swigCPtr);
			ProgressionTrackBindingVector result = ((progressionTrackBindingsResponse_Results_get == IntPtr.Zero) ? null : new ProgressionTrackBindingVector(progressionTrackBindingsResponse_Results_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetProgressionTrackBindingsResponse_Results_set(swigCPtr, ProgressionTrackBindingVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetProgressionTrackBindingsResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetProgressionTrackBindingsResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetProgressionTrackBindingsResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetProgressionTrackBindingsResponse obj)
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
					MothershipApiPINVOKE.delete_GetProgressionTrackBindingsResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool progressionTrackBindingsResponse_ParseFromResponseString = MothershipApiPINVOKE.GetProgressionTrackBindingsResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return progressionTrackBindingsResponse_ParseFromResponseString;
	}

	public static GetProgressionTrackBindingsResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr progressionTrackBindingsResponse_FromMothershipResponse = MothershipApiPINVOKE.GetProgressionTrackBindingsResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetProgressionTrackBindingsResponse result = ((progressionTrackBindingsResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetProgressionTrackBindingsResponse(progressionTrackBindingsResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetProgressionTrackBindingsResponse()
		: this(MothershipApiPINVOKE.new_GetProgressionTrackBindingsResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

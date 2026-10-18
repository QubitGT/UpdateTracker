using System;
using System.Runtime.InteropServices;

public class GetProgressionTrackValuesForPlayerResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string Results_name = MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerResponse_Results_name_get();

	public UserHydratedProgressionTrackVector Results
	{
		get
		{
			IntPtr progressionTrackValuesForPlayerResponse_Results_get = MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerResponse_Results_get(swigCPtr);
			UserHydratedProgressionTrackVector result = ((progressionTrackValuesForPlayerResponse_Results_get == IntPtr.Zero) ? null : new UserHydratedProgressionTrackVector(progressionTrackValuesForPlayerResponse_Results_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerResponse_Results_set(swigCPtr, UserHydratedProgressionTrackVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetProgressionTrackValuesForPlayerResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetProgressionTrackValuesForPlayerResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetProgressionTrackValuesForPlayerResponse obj)
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
					MothershipApiPINVOKE.delete_GetProgressionTrackValuesForPlayerResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool progressionTrackValuesForPlayerResponse_ParseFromResponseString = MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return progressionTrackValuesForPlayerResponse_ParseFromResponseString;
	}

	public static GetProgressionTrackValuesForPlayerResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr progressionTrackValuesForPlayerResponse_FromMothershipResponse = MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetProgressionTrackValuesForPlayerResponse result = ((progressionTrackValuesForPlayerResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetProgressionTrackValuesForPlayerResponse(progressionTrackValuesForPlayerResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetProgressionTrackValuesForPlayerResponse()
		: this(MothershipApiPINVOKE.new_GetProgressionTrackValuesForPlayerResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

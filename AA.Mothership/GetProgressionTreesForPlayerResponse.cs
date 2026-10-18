using System;
using System.Runtime.InteropServices;

public class GetProgressionTreesForPlayerResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public static readonly string Results_name = MothershipApiPINVOKE.GetProgressionTreesForPlayerResponse_Results_name_get();

	public UserHydratedProgressionTreeVector Results
	{
		get
		{
			IntPtr progressionTreesForPlayerResponse_Results_get = MothershipApiPINVOKE.GetProgressionTreesForPlayerResponse_Results_get(swigCPtr);
			UserHydratedProgressionTreeVector result = ((progressionTreesForPlayerResponse_Results_get == IntPtr.Zero) ? null : new UserHydratedProgressionTreeVector(progressionTreesForPlayerResponse_Results_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetProgressionTreesForPlayerResponse_Results_set(swigCPtr, UserHydratedProgressionTreeVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetProgressionTreesForPlayerResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetProgressionTreesForPlayerResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetProgressionTreesForPlayerResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetProgressionTreesForPlayerResponse obj)
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
					MothershipApiPINVOKE.delete_GetProgressionTreesForPlayerResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool progressionTreesForPlayerResponse_ParseFromResponseString = MothershipApiPINVOKE.GetProgressionTreesForPlayerResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return progressionTreesForPlayerResponse_ParseFromResponseString;
	}

	public static GetProgressionTreesForPlayerResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr progressionTreesForPlayerResponse_FromMothershipResponse = MothershipApiPINVOKE.GetProgressionTreesForPlayerResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetProgressionTreesForPlayerResponse result = ((progressionTreesForPlayerResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetProgressionTreesForPlayerResponse(progressionTreesForPlayerResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetProgressionTreesForPlayerResponse()
		: this(MothershipApiPINVOKE.new_GetProgressionTreesForPlayerResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

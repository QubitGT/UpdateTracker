using System;
using System.Runtime.InteropServices;

public class GetSharedGroupDataResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public SharedGroupDataRecordMap data
	{
		get
		{
			IntPtr sharedGroupDataResponse_data_get = MothershipApiPINVOKE.GetSharedGroupDataResponse_data_get(swigCPtr);
			SharedGroupDataRecordMap result = ((sharedGroupDataResponse_data_get == IntPtr.Zero) ? null : new SharedGroupDataRecordMap(sharedGroupDataResponse_data_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetSharedGroupDataResponse_data_set(swigCPtr, SharedGroupDataRecordMap.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public StringVector members
	{
		get
		{
			IntPtr sharedGroupDataResponse_members_get = MothershipApiPINVOKE.GetSharedGroupDataResponse_members_get(swigCPtr);
			StringVector result = ((sharedGroupDataResponse_members_get == IntPtr.Zero) ? null : new StringVector(sharedGroupDataResponse_members_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetSharedGroupDataResponse_members_set(swigCPtr, StringVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetSharedGroupDataResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetSharedGroupDataResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetSharedGroupDataResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetSharedGroupDataResponse obj)
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
					MothershipApiPINVOKE.delete_GetSharedGroupDataResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool sharedGroupDataResponse_ParseFromResponseString = MothershipApiPINVOKE.GetSharedGroupDataResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return sharedGroupDataResponse_ParseFromResponseString;
	}

	public static GetSharedGroupDataResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr sharedGroupDataResponse_FromMothershipResponse = MothershipApiPINVOKE.GetSharedGroupDataResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetSharedGroupDataResponse result = ((sharedGroupDataResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetSharedGroupDataResponse(sharedGroupDataResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetSharedGroupDataResponse()
		: this(MothershipApiPINVOKE.new_GetSharedGroupDataResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

using System;
using System.Runtime.InteropServices;

public class GetTitleResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public string title_id
	{
		get
		{
			string titleResponse_title_id_get = MothershipApiPINVOKE.GetTitleResponse_title_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return titleResponse_title_id_get;
		}
		set
		{
			MothershipApiPINVOKE.GetTitleResponse_title_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string title_name
	{
		get
		{
			string titleResponse_title_name_get = MothershipApiPINVOKE.GetTitleResponse_title_name_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return titleResponse_title_name_get;
		}
		set
		{
			MothershipApiPINVOKE.GetTitleResponse_title_name_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetTitleResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetTitleResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetTitleResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetTitleResponse obj)
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
					MothershipApiPINVOKE.delete_GetTitleResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool titleResponse_ParseFromResponseString = MothershipApiPINVOKE.GetTitleResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return titleResponse_ParseFromResponseString;
	}

	public static GetTitleResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr titleResponse_FromMothershipResponse = MothershipApiPINVOKE.GetTitleResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetTitleResponse result = ((titleResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetTitleResponse(titleResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetTitleResponse()
		: this(MothershipApiPINVOKE.new_GetTitleResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

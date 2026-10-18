using System;
using System.Runtime.InteropServices;

public class GetReportResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipReportData Report
	{
		get
		{
			IntPtr reportResponse_Report_get = MothershipApiPINVOKE.GetReportResponse_Report_get(swigCPtr);
			MothershipReportData result = ((reportResponse_Report_get == IntPtr.Zero) ? null : new MothershipReportData(reportResponse_Report_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetReportResponse_Report_set(swigCPtr, MothershipReportData.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetReportResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetReportResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetReportResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetReportResponse obj)
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
					MothershipApiPINVOKE.delete_GetReportResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool reportResponse_ParseFromResponseString = MothershipApiPINVOKE.GetReportResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return reportResponse_ParseFromResponseString;
	}

	public static GetReportResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr reportResponse_FromMothershipResponse = MothershipApiPINVOKE.GetReportResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetReportResponse result = ((reportResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetReportResponse(reportResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetReportResponse()
		: this(MothershipApiPINVOKE.new_GetReportResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

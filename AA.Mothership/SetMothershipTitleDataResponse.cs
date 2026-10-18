using System;
using System.Runtime.InteropServices;

public class SetMothershipTitleDataResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipTitleData TitleData
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.SetMothershipTitleDataResponse_TitleData_get(swigCPtr);
			MothershipTitleData result = ((intPtr == IntPtr.Zero) ? null : new MothershipTitleData(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.SetMothershipTitleDataResponse_TitleData_set(swigCPtr, MothershipTitleData.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal SetMothershipTitleDataResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.SetMothershipTitleDataResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(SetMothershipTitleDataResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(SetMothershipTitleDataResponse obj)
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
					MothershipApiPINVOKE.delete_SetMothershipTitleDataResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.SetMothershipTitleDataResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static SetMothershipTitleDataResponse FromMothershipResponse(MothershipResponse respnose)
	{
		IntPtr intPtr = MothershipApiPINVOKE.SetMothershipTitleDataResponse_FromMothershipResponse(MothershipResponse.getCPtr(respnose));
		SetMothershipTitleDataResponse result = ((intPtr == IntPtr.Zero) ? null : new SetMothershipTitleDataResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public SetMothershipTitleDataResponse()
		: this(MothershipApiPINVOKE.new_SetMothershipTitleDataResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

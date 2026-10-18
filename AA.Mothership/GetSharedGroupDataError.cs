using System;
using System.Runtime.InteropServices;

public class GetSharedGroupDataError : MothershipError
{
	private HandleRef swigCPtr;

	internal GetSharedGroupDataError(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetSharedGroupDataError_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetSharedGroupDataError obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetSharedGroupDataError obj)
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
					MothershipApiPINVOKE.delete_GetSharedGroupDataError(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public GetSharedGroupDataError(string message, int statusCode, string traceId, string mothershipErrorCode)
		: this(MothershipApiPINVOKE.new_GetSharedGroupDataError__SWIG_0(message, statusCode, traceId, mothershipErrorCode), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public GetSharedGroupDataError(string message, int statusCode, string traceId)
		: this(MothershipApiPINVOKE.new_GetSharedGroupDataError__SWIG_1(message, statusCode, traceId), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public GetSharedGroupDataError(string message, int statusCode)
		: this(MothershipApiPINVOKE.new_GetSharedGroupDataError__SWIG_2(message, statusCode), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

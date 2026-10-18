using System;
using System.Runtime.InteropServices;

public class BulkGetAccountLinksFailedError : MothershipError
{
	private HandleRef swigCPtr;

	internal BulkGetAccountLinksFailedError(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.BulkGetAccountLinksFailedError_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(BulkGetAccountLinksFailedError obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(BulkGetAccountLinksFailedError obj)
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
					MothershipApiPINVOKE.delete_BulkGetAccountLinksFailedError(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public BulkGetAccountLinksFailedError(string message, int statusCode, string traceId, string mothershipErrorCode)
		: this(MothershipApiPINVOKE.new_BulkGetAccountLinksFailedError__SWIG_0(message, statusCode, traceId, mothershipErrorCode), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public BulkGetAccountLinksFailedError(string message, int statusCode, string traceId)
		: this(MothershipApiPINVOKE.new_BulkGetAccountLinksFailedError__SWIG_1(message, statusCode, traceId), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public BulkGetAccountLinksFailedError(string message, int statusCode)
		: this(MothershipApiPINVOKE.new_BulkGetAccountLinksFailedError__SWIG_2(message, statusCode), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

using System;
using System.Runtime.InteropServices;

public class MothershipWebSocketMessageDelegateWrapper : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal MothershipWebSocketMessageDelegateWrapper(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipWebSocketMessageDelegateWrapper obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipWebSocketMessageDelegateWrapper obj)
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

	~MothershipWebSocketMessageDelegateWrapper()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					MothershipApiPINVOKE.delete_MothershipWebSocketMessageDelegateWrapper(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public virtual void OnOpenCallback(IntPtr userData)
	{
		MothershipApiPINVOKE.MothershipWebSocketMessageDelegateWrapper_OnOpenCallback(swigCPtr, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void OnMessageCallback(MothershipWebSocketMessage message, IntPtr userData)
	{
		MothershipApiPINVOKE.MothershipWebSocketMessageDelegateWrapper_OnMessageCallback(swigCPtr, MothershipWebSocketMessage.getCPtr(message), userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void OnCloseCallback(IntPtr userData)
	{
		MothershipApiPINVOKE.MothershipWebSocketMessageDelegateWrapper_OnCloseCallback(swigCPtr, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void OnErrorCallback(IntPtr userData)
	{
		MothershipApiPINVOKE.MothershipWebSocketMessageDelegateWrapper_OnErrorCallback(swigCPtr, userData);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

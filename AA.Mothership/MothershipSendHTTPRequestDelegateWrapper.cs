using System;
using System.Reflection;
using System.Runtime.InteropServices;
using AOT;

public class MothershipSendHTTPRequestDelegateWrapper : IDisposable
{
	public delegate bool SwigDelegateMothershipSendHTTPRequestDelegateWrapper_0(IntPtr request);

	private HandleRef swigCPtr;

	protected static MothershipSendHTTPRequestDelegateWrapper selfInstance;

	protected bool swigCMemOwn;

	private SwigDelegateMothershipSendHTTPRequestDelegateWrapper_0 swigDelegate0;

	private static Type[] swigMethodTypes0 = new Type[1] { typeof(MothershipHTTPRequest) };

	internal MothershipSendHTTPRequestDelegateWrapper(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipSendHTTPRequestDelegateWrapper obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipSendHTTPRequestDelegateWrapper obj)
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

	~MothershipSendHTTPRequestDelegateWrapper()
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
					MothershipApiPINVOKE.delete_MothershipSendHTTPRequestDelegateWrapper(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public virtual bool SendRequest(MothershipHTTPRequest request)
	{
		bool result = MothershipApiPINVOKE.MothershipSendHTTPRequestDelegateWrapper_SendRequest(swigCPtr, MothershipHTTPRequest.getCPtr(request));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public MothershipSendHTTPRequestDelegateWrapper()
		: this(MothershipApiPINVOKE.new_MothershipSendHTTPRequestDelegateWrapper(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		SwigDirectorConnect();
	}

	private void SwigDirectorConnect()
	{
		selfInstance = this;
		if (SwigDerivedClassHasMethod("SendRequest", swigMethodTypes0))
		{
			swigDelegate0 = SwigDirectorMethodSendRequest;
		}
		MothershipApiPINVOKE.MothershipSendHTTPRequestDelegateWrapper_director_connect(swigCPtr, swigDelegate0);
	}

	private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
	{
		MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.DeclaringType == null || methodInfo.Name != methodName)
			{
				continue;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length != methodTypes.Length)
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < parameters.Length; j++)
			{
				if (parameters[j].ParameterType != methodTypes[j])
				{
					flag = false;
					break;
				}
			}
			if (flag && methodInfo.IsVirtual && methodInfo.DeclaringType.IsSubclassOf(typeof(MothershipSendHTTPRequestDelegateWrapper)) && methodInfo.DeclaringType != methodInfo.GetBaseDefinition().DeclaringType)
			{
				return true;
			}
		}
		return false;
	}

	[MonoPInvokeCallback(typeof(SwigDelegateMothershipSendHTTPRequestDelegateWrapper_0))]
	private static bool SwigDirectorMethodSendRequest(IntPtr request)
	{
		return selfInstance.SendRequest(new MothershipHTTPRequest(request, cMemoryOwn: false));
	}
}

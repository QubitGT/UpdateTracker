using System;
using System.Reflection;
using System.Runtime.InteropServices;
using AOT;

public class MothershipWebSocketDelegateWrapper : IDisposable
{
	public delegate bool SwigDelegateMothershipWebSocketDelegateWrapper_0(IntPtr request);

	public delegate bool SwigDelegateMothershipWebSocketDelegateWrapper_1(IntPtr request);

	private HandleRef swigCPtr;

	protected static MothershipWebSocketDelegateWrapper selfInstance;

	protected bool swigCMemOwn;

	private SwigDelegateMothershipWebSocketDelegateWrapper_0 swigDelegate0;

	private SwigDelegateMothershipWebSocketDelegateWrapper_1 swigDelegate1;

	private static Type[] swigMethodTypes0 = new Type[1] { typeof(MothershipOpenWebSocketEventArgs) };

	private static Type[] swigMethodTypes1 = new Type[1] { typeof(MothershipCloseWebSocketEventArgs) };

	internal MothershipWebSocketDelegateWrapper(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipWebSocketDelegateWrapper obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipWebSocketDelegateWrapper obj)
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

	~MothershipWebSocketDelegateWrapper()
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
					MothershipApiPINVOKE.delete_MothershipWebSocketDelegateWrapper(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public virtual bool CreateConnection(MothershipOpenWebSocketEventArgs request)
	{
		bool result = MothershipApiPINVOKE.MothershipWebSocketDelegateWrapper_CreateConnection(swigCPtr, MothershipOpenWebSocketEventArgs.getCPtr(request));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual bool CloseConnection(MothershipCloseWebSocketEventArgs request)
	{
		bool result = MothershipApiPINVOKE.MothershipWebSocketDelegateWrapper_CloseConnection(swigCPtr, MothershipCloseWebSocketEventArgs.getCPtr(request));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public MothershipWebSocketDelegateWrapper()
		: this(MothershipApiPINVOKE.new_MothershipWebSocketDelegateWrapper(), cMemoryOwn: true)
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
		if (SwigDerivedClassHasMethod("CreateConnection", swigMethodTypes0))
		{
			swigDelegate0 = SwigDirectorMethodCreateConnection;
		}
		if (SwigDerivedClassHasMethod("CloseConnection", swigMethodTypes1))
		{
			swigDelegate1 = SwigDirectorMethodCloseConnection;
		}
		MothershipApiPINVOKE.MothershipWebSocketDelegateWrapper_director_connect(swigCPtr, swigDelegate0, swigDelegate1);
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
			if (flag && methodInfo.IsVirtual && methodInfo.DeclaringType.IsSubclassOf(typeof(MothershipWebSocketDelegateWrapper)) && methodInfo.DeclaringType != methodInfo.GetBaseDefinition().DeclaringType)
			{
				return true;
			}
		}
		return false;
	}

	[MonoPInvokeCallback(typeof(SwigDelegateMothershipWebSocketDelegateWrapper_0))]
	private static bool SwigDirectorMethodCreateConnection(IntPtr request)
	{
		return selfInstance.CreateConnection(new MothershipOpenWebSocketEventArgs(request, cMemoryOwn: false));
	}

	[MonoPInvokeCallback(typeof(SwigDelegateMothershipWebSocketDelegateWrapper_1))]
	private static bool SwigDirectorMethodCloseConnection(IntPtr request)
	{
		return selfInstance.CloseConnection(new MothershipCloseWebSocketEventArgs(request, cMemoryOwn: false));
	}
}

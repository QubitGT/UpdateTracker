using System;
using System.Reflection;
using System.Runtime.InteropServices;
using AOT;

public class MothershipLogDelegateWrapper : IDisposable
{
	public delegate void SwigDelegateMothershipLogDelegateWrapper_0(int level, string message);

	private HandleRef swigCPtr;

	protected static MothershipLogDelegateWrapper selfInstance;

	protected bool swigCMemOwn;

	private SwigDelegateMothershipLogDelegateWrapper_0 swigDelegate0;

	private static Type[] swigMethodTypes0 = new Type[2]
	{
		typeof(MothershipLogLevel),
		typeof(string)
	};

	internal MothershipLogDelegateWrapper(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipLogDelegateWrapper obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipLogDelegateWrapper obj)
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

	~MothershipLogDelegateWrapper()
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
					MothershipApiPINVOKE.delete_MothershipLogDelegateWrapper(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public virtual void OnLogCallback(MothershipLogLevel level, string message)
	{
		MothershipApiPINVOKE.MothershipLogDelegateWrapper_OnLogCallback(swigCPtr, (int)level, message);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public MothershipLogDelegateWrapper()
		: this(MothershipApiPINVOKE.new_MothershipLogDelegateWrapper(), cMemoryOwn: true)
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
		if (SwigDerivedClassHasMethod("OnLogCallback", swigMethodTypes0))
		{
			swigDelegate0 = SwigDirectorMethodOnLogCallback;
		}
		MothershipApiPINVOKE.MothershipLogDelegateWrapper_director_connect(swigCPtr, swigDelegate0);
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
			if (flag && methodInfo.IsVirtual && methodInfo.DeclaringType.IsSubclassOf(typeof(MothershipLogDelegateWrapper)) && methodInfo.DeclaringType != methodInfo.GetBaseDefinition().DeclaringType)
			{
				return true;
			}
		}
		return false;
	}

	[MonoPInvokeCallback(typeof(SwigDelegateMothershipLogDelegateWrapper_0))]
	private static void SwigDirectorMethodOnLogCallback(int level, string message)
	{
		selfInstance.OnLogCallback((MothershipLogLevel)level, message);
	}
}

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using AOT;

public class VerifyTokenCompleteDelegateWrapper : MothershipRequestCompleteDelegateWrapper
{
	public delegate void SwigDelegateVerifyTokenCompleteDelegateWrapper_0(IntPtr response, bool wasSuccess, IntPtr error, IntPtr userData);

	private HandleRef swigCPtr;

	protected static VerifyTokenCompleteDelegateWrapper selfInstance;

	private SwigDelegateVerifyTokenCompleteDelegateWrapper_0 swigDelegate0;

	private static Type[] swigMethodTypes0 = new Type[4]
	{
		typeof(MothershipResponse),
		typeof(bool),
		typeof(MothershipError),
		typeof(IntPtr)
	};

	internal VerifyTokenCompleteDelegateWrapper(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.VerifyTokenCompleteDelegateWrapper_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(VerifyTokenCompleteDelegateWrapper obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(VerifyTokenCompleteDelegateWrapper obj)
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
					MothershipApiPINVOKE.delete_VerifyTokenCompleteDelegateWrapper(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public VerifyTokenCompleteDelegateWrapper()
		: this(MothershipApiPINVOKE.new_VerifyTokenCompleteDelegateWrapper(), cMemoryOwn: true)
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
		if (SwigDerivedClassHasMethod("OnCompleteCallback", swigMethodTypes0))
		{
			swigDelegate0 = SwigDirectorMethodOnCompleteCallback;
		}
		MothershipApiPINVOKE.VerifyTokenCompleteDelegateWrapper_director_connect(swigCPtr, swigDelegate0);
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
			if (flag && methodInfo.IsVirtual && methodInfo.DeclaringType.IsSubclassOf(typeof(VerifyTokenCompleteDelegateWrapper)) && methodInfo.DeclaringType != methodInfo.GetBaseDefinition().DeclaringType)
			{
				return true;
			}
		}
		return false;
	}

	[MonoPInvokeCallback(typeof(SwigDelegateVerifyTokenCompleteDelegateWrapper_0))]
	private static void SwigDirectorMethodOnCompleteCallback(IntPtr response, bool wasSuccess, IntPtr error, IntPtr userData)
	{
		selfInstance.OnCompleteCallback(new MothershipResponse(response, cMemoryOwn: false), wasSuccess, new MothershipError(error, cMemoryOwn: false), userData);
	}
}

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using AOT;

public class NotificationsMessageDelegateWrapper : MothershipWebSocketMessageDelegateWrapper
{
	public delegate void SwigDelegateNotificationsMessageDelegateWrapper_0(IntPtr userData);

	public delegate void SwigDelegateNotificationsMessageDelegateWrapper_1(IntPtr message, IntPtr userData);

	public delegate void SwigDelegateNotificationsMessageDelegateWrapper_2(IntPtr userData);

	public delegate void SwigDelegateNotificationsMessageDelegateWrapper_3(IntPtr userData);

	private HandleRef swigCPtr;

	protected static NotificationsMessageDelegateWrapper selfInstance;

	private SwigDelegateNotificationsMessageDelegateWrapper_0 swigDelegate0;

	private SwigDelegateNotificationsMessageDelegateWrapper_1 swigDelegate1;

	private SwigDelegateNotificationsMessageDelegateWrapper_2 swigDelegate2;

	private SwigDelegateNotificationsMessageDelegateWrapper_3 swigDelegate3;

	private static Type[] swigMethodTypes0 = new Type[1] { typeof(IntPtr) };

	private static Type[] swigMethodTypes1 = new Type[2]
	{
		typeof(MothershipWebSocketMessage),
		typeof(IntPtr)
	};

	private static Type[] swigMethodTypes2 = new Type[1] { typeof(IntPtr) };

	private static Type[] swigMethodTypes3 = new Type[1] { typeof(IntPtr) };

	internal NotificationsMessageDelegateWrapper(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.NotificationsMessageDelegateWrapper_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(NotificationsMessageDelegateWrapper obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(NotificationsMessageDelegateWrapper obj)
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
					MothershipApiPINVOKE.delete_NotificationsMessageDelegateWrapper(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public NotificationsMessageDelegateWrapper()
		: this(MothershipApiPINVOKE.new_NotificationsMessageDelegateWrapper(), cMemoryOwn: true)
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
		if (SwigDerivedClassHasMethod("OnOpenCallback", swigMethodTypes0))
		{
			swigDelegate0 = SwigDirectorMethodOnOpenCallback;
		}
		if (SwigDerivedClassHasMethod("OnMessageCallback", swigMethodTypes1))
		{
			swigDelegate1 = SwigDirectorMethodOnMessageCallback;
		}
		if (SwigDerivedClassHasMethod("OnCloseCallback", swigMethodTypes2))
		{
			swigDelegate2 = SwigDirectorMethodOnCloseCallback;
		}
		if (SwigDerivedClassHasMethod("OnErrorCallback", swigMethodTypes3))
		{
			swigDelegate3 = SwigDirectorMethodOnErrorCallback;
		}
		MothershipApiPINVOKE.NotificationsMessageDelegateWrapper_director_connect(swigCPtr, swigDelegate0, swigDelegate1, swigDelegate2, swigDelegate3);
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
			if (flag && methodInfo.IsVirtual && methodInfo.DeclaringType.IsSubclassOf(typeof(NotificationsMessageDelegateWrapper)) && methodInfo.DeclaringType != methodInfo.GetBaseDefinition().DeclaringType)
			{
				return true;
			}
		}
		return false;
	}

	[MonoPInvokeCallback(typeof(SwigDelegateNotificationsMessageDelegateWrapper_0))]
	private static void SwigDirectorMethodOnOpenCallback(IntPtr userData)
	{
		selfInstance.OnOpenCallback(userData);
	}

	[MonoPInvokeCallback(typeof(SwigDelegateNotificationsMessageDelegateWrapper_1))]
	private static void SwigDirectorMethodOnMessageCallback(IntPtr message, IntPtr userData)
	{
		selfInstance.OnMessageCallback(new MothershipWebSocketMessage(message, cMemoryOwn: false), userData);
	}

	[MonoPInvokeCallback(typeof(SwigDelegateNotificationsMessageDelegateWrapper_2))]
	private static void SwigDirectorMethodOnCloseCallback(IntPtr userData)
	{
		selfInstance.OnCloseCallback(userData);
	}

	[MonoPInvokeCallback(typeof(SwigDelegateNotificationsMessageDelegateWrapper_3))]
	private static void SwigDirectorMethodOnErrorCallback(IntPtr userData)
	{
		selfInstance.OnErrorCallback(userData);
	}
}

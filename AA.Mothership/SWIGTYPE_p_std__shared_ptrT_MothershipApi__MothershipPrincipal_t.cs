using System;
using System.Runtime.InteropServices;

public class SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipPrincipal_t
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipPrincipal_t(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipPrincipal_t()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipPrincipal_t obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipPrincipal_t obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}

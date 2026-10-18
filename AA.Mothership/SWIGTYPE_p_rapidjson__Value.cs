using System;
using System.Runtime.InteropServices;

public class SWIGTYPE_p_rapidjson__Value
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_rapidjson__Value(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_rapidjson__Value()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_rapidjson__Value obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(SWIGTYPE_p_rapidjson__Value obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}

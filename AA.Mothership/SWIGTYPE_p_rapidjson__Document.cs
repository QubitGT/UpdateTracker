using System;
using System.Runtime.InteropServices;

public class SWIGTYPE_p_rapidjson__Document
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_rapidjson__Document(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_rapidjson__Document()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_rapidjson__Document obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(SWIGTYPE_p_rapidjson__Document obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}

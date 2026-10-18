using System;
using System.Runtime.InteropServices;

public class ComplexPrerequisiteNodes : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public string type
	{
		get
		{
			string result = MothershipApiPINVOKE.ComplexPrerequisiteNodes_type_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.ComplexPrerequisiteNodes_type_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public ComplexNodeVector nodes
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.ComplexPrerequisiteNodes_nodes_get(swigCPtr);
			ComplexNodeVector result = ((intPtr == IntPtr.Zero) ? null : new ComplexNodeVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.ComplexPrerequisiteNodes_nodes_set(swigCPtr, ComplexNodeVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal ComplexPrerequisiteNodes(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(ComplexPrerequisiteNodes obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(ComplexPrerequisiteNodes obj)
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

	~ComplexPrerequisiteNodes()
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
					MothershipApiPINVOKE.delete_ComplexPrerequisiteNodes(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public bool ParseFromString(string response)
	{
		bool result = MothershipApiPINVOKE.ComplexPrerequisiteNodes_ParseFromString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ToJson(SWIGTYPE_p_rapidjson__Value complexNode, SWIGTYPE_p_rapidjson__Document body)
	{
		bool result = MothershipApiPINVOKE.ComplexPrerequisiteNodes_ToJson(swigCPtr, SWIGTYPE_p_rapidjson__Value.getCPtr(complexNode), SWIGTYPE_p_rapidjson__Document.getCPtr(body));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool isEmpty()
	{
		bool result = MothershipApiPINVOKE.ComplexPrerequisiteNodes_isEmpty(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ComplexPrerequisiteNodes()
		: this(MothershipApiPINVOKE.new_ComplexPrerequisiteNodes(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

using System;
using System.Runtime.InteropServices;

public class HydratedProgressionNodeCost : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public HydratedInventoryChangeMap items
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.HydratedProgressionNodeCost_items_get(swigCPtr);
			HydratedInventoryChangeMap result = ((intPtr == IntPtr.Zero) ? null : new HydratedInventoryChangeMap(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.HydratedProgressionNodeCost_items_set(swigCPtr, HydratedInventoryChangeMap.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal HydratedProgressionNodeCost(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(HydratedProgressionNodeCost obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(HydratedProgressionNodeCost obj)
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

	~HydratedProgressionNodeCost()
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
					MothershipApiPINVOKE.delete_HydratedProgressionNodeCost(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public bool ParseFromString(string response)
	{
		bool result = MothershipApiPINVOKE.HydratedProgressionNodeCost_ParseFromString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ToJson(SWIGTYPE_p_rapidjson__Value nodeCost, SWIGTYPE_p_rapidjson__Document body)
	{
		bool result = MothershipApiPINVOKE.HydratedProgressionNodeCost_ToJson(swigCPtr, SWIGTYPE_p_rapidjson__Value.getCPtr(nodeCost), SWIGTYPE_p_rapidjson__Document.getCPtr(body));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool isEmpty()
	{
		bool result = MothershipApiPINVOKE.HydratedProgressionNodeCost_isEmpty(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public HydratedProgressionNodeCost()
		: this(MothershipApiPINVOKE.new_HydratedProgressionNodeCost(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

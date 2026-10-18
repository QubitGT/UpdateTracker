using System;
using System.Runtime.InteropServices;

public class HydratedProgressionTreeResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public ProgressionTree Tree
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.HydratedProgressionTreeResponse_Tree_get(swigCPtr);
			ProgressionTree result = ((intPtr == IntPtr.Zero) ? null : new ProgressionTree(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.HydratedProgressionTreeResponse_Tree_set(swigCPtr, ProgressionTree.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public TreeNodeVector Nodes
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.HydratedProgressionTreeResponse_Nodes_get(swigCPtr);
			TreeNodeVector result = ((intPtr == IntPtr.Zero) ? null : new TreeNodeVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.HydratedProgressionTreeResponse_Nodes_set(swigCPtr, TreeNodeVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public ProgressionTrack Track
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.HydratedProgressionTreeResponse_Track_get(swigCPtr);
			ProgressionTrack result = ((intPtr == IntPtr.Zero) ? null : new ProgressionTrack(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.HydratedProgressionTreeResponse_Track_set(swigCPtr, ProgressionTrack.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal HydratedProgressionTreeResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.HydratedProgressionTreeResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(HydratedProgressionTreeResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(HydratedProgressionTreeResponse obj)
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
					MothershipApiPINVOKE.delete_HydratedProgressionTreeResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public static HydratedProgressionTreeResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.HydratedProgressionTreeResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		HydratedProgressionTreeResponse result = ((intPtr == IntPtr.Zero) ? null : new HydratedProgressionTreeResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.HydratedProgressionTreeResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public HydratedProgressionTreeResponse()
		: this(MothershipApiPINVOKE.new_HydratedProgressionTreeResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

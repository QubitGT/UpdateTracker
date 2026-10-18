using System;
using System.Runtime.InteropServices;

public class UserHydratedProgressionTreeResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public ProgressionTree Tree
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_Tree_get(swigCPtr);
			ProgressionTree result = ((intPtr == IntPtr.Zero) ? null : new ProgressionTree(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_Tree_set(swigCPtr, ProgressionTree.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public UserHydratedNodeVector Nodes
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_Nodes_get(swigCPtr);
			UserHydratedNodeVector result = ((intPtr == IntPtr.Zero) ? null : new UserHydratedNodeVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_Nodes_set(swigCPtr, UserHydratedNodeVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public UserHydratedProgressionTrackResponse Track
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_Track_get(swigCPtr);
			UserHydratedProgressionTrackResponse result = ((intPtr == IntPtr.Zero) ? null : new UserHydratedProgressionTrackResponse(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_Track_set(swigCPtr, UserHydratedProgressionTrackResponse.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string PlayerId
	{
		get
		{
			string result = MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_PlayerId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_PlayerId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public bool InventoryRefreshRequired
	{
		get
		{
			bool result = MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_InventoryRefreshRequired_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_InventoryRefreshRequired_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal UserHydratedProgressionTreeResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(UserHydratedProgressionTreeResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(UserHydratedProgressionTreeResponse obj)
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
					MothershipApiPINVOKE.delete_UserHydratedProgressionTreeResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public static UserHydratedProgressionTreeResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		UserHydratedProgressionTreeResponse result = ((intPtr == IntPtr.Zero) ? null : new UserHydratedProgressionTreeResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.UserHydratedProgressionTreeResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public UserHydratedProgressionTreeResponse()
		: this(MothershipApiPINVOKE.new_UserHydratedProgressionTreeResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

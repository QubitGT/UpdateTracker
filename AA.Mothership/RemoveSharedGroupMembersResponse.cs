using System;
using System.Runtime.InteropServices;

public class RemoveSharedGroupMembersResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	internal RemoveSharedGroupMembersResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.RemoveSharedGroupMembersResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(RemoveSharedGroupMembersResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(RemoveSharedGroupMembersResponse obj)
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
					MothershipApiPINVOKE.delete_RemoveSharedGroupMembersResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.RemoveSharedGroupMembersResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static RemoveSharedGroupMembersResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.RemoveSharedGroupMembersResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		RemoveSharedGroupMembersResponse result = ((intPtr == IntPtr.Zero) ? null : new RemoveSharedGroupMembersResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public RemoveSharedGroupMembersResponse()
		: this(MothershipApiPINVOKE.new_RemoveSharedGroupMembersResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

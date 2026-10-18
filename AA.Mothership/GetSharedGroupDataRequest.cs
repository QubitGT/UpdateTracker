using System;
using System.Runtime.InteropServices;

public class GetSharedGroupDataRequest : MothershipRequestShared
{
	private HandleRef swigCPtr;

	public string sharedGroupId
	{
		get
		{
			string sharedGroupDataRequest_sharedGroupId_get = MothershipApiPINVOKE.GetSharedGroupDataRequest_sharedGroupId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return sharedGroupDataRequest_sharedGroupId_get;
		}
		set
		{
			MothershipApiPINVOKE.GetSharedGroupDataRequest_sharedGroupId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public bool getMembers
	{
		get
		{
			bool sharedGroupDataRequest_getMembers_get = MothershipApiPINVOKE.GetSharedGroupDataRequest_getMembers_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return sharedGroupDataRequest_getMembers_get;
		}
		set
		{
			MothershipApiPINVOKE.GetSharedGroupDataRequest_getMembers_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public StringVector keys
	{
		get
		{
			IntPtr sharedGroupDataRequest_keys_get = MothershipApiPINVOKE.GetSharedGroupDataRequest_keys_get(swigCPtr);
			StringVector result = ((sharedGroupDataRequest_keys_get == IntPtr.Zero) ? null : new StringVector(sharedGroupDataRequest_keys_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetSharedGroupDataRequest_keys_set(swigCPtr, StringVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetSharedGroupDataRequest(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetSharedGroupDataRequest_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetSharedGroupDataRequest obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetSharedGroupDataRequest obj)
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
					MothershipApiPINVOKE.delete_GetSharedGroupDataRequest(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t ToHttpRequest()
	{
		SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t result = new SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t(MothershipApiPINVOKE.GetSharedGroupDataRequest_ToHttpRequest(swigCPtr), futureUse: true);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetSharedGroupDataRequest()
		: this(MothershipApiPINVOKE.new_GetSharedGroupDataRequest(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

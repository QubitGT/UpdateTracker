using System;
using System.Runtime.InteropServices;

public class GetProgressionTrackValuesForPlayerRequest : MothershipRequestShared
{
	private HandleRef swigCPtr;

	public string playerId
	{
		get
		{
			string progressionTrackValuesForPlayerRequest_playerId_get = MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerRequest_playerId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return progressionTrackValuesForPlayerRequest_playerId_get;
		}
		set
		{
			MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerRequest_playerId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string titleId
	{
		get
		{
			string progressionTrackValuesForPlayerRequest_titleId_get = MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerRequest_titleId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return progressionTrackValuesForPlayerRequest_titleId_get;
		}
		set
		{
			MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerRequest_titleId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string envId
	{
		get
		{
			string progressionTrackValuesForPlayerRequest_envId_get = MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerRequest_envId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return progressionTrackValuesForPlayerRequest_envId_get;
		}
		set
		{
			MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerRequest_envId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string deploymentId
	{
		get
		{
			string progressionTrackValuesForPlayerRequest_deploymentId_get = MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerRequest_deploymentId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return progressionTrackValuesForPlayerRequest_deploymentId_get;
		}
		set
		{
			MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerRequest_deploymentId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetProgressionTrackValuesForPlayerRequest(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerRequest_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetProgressionTrackValuesForPlayerRequest obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetProgressionTrackValuesForPlayerRequest obj)
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
					MothershipApiPINVOKE.delete_GetProgressionTrackValuesForPlayerRequest(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t ToHttpRequest()
	{
		SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t result = new SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t(MothershipApiPINVOKE.GetProgressionTrackValuesForPlayerRequest_ToHttpRequest(swigCPtr), futureUse: true);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetProgressionTrackValuesForPlayerRequest()
		: this(MothershipApiPINVOKE.new_GetProgressionTrackValuesForPlayerRequest(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

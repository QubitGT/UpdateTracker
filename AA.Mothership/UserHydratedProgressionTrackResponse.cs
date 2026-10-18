using System;
using System.Runtime.InteropServices;

public class UserHydratedProgressionTrackResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public ProgressionTrack Track
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_Track_get(swigCPtr);
			ProgressionTrack result = ((intPtr == IntPtr.Zero) ? null : new ProgressionTrack(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_Track_set(swigCPtr, ProgressionTrack.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public TrackTriggerVector Triggers
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_Triggers_get(swigCPtr);
			TrackTriggerVector result = ((intPtr == IntPtr.Zero) ? null : new TrackTriggerVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_Triggers_set(swigCPtr, TrackTriggerVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public TrackLevelVector Levels
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_Levels_get(swigCPtr);
			TrackLevelVector result = ((intPtr == IntPtr.Zero) ? null : new TrackLevelVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_Levels_set(swigCPtr, TrackLevelVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public int Progress
	{
		get
		{
			int result = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_Progress_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_Progress_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string CurrentLevelName
	{
		get
		{
			string result = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_CurrentLevelName_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_CurrentLevelName_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string CurrentLevelId
	{
		get
		{
			string result = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_CurrentLevelId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_CurrentLevelId_set(swigCPtr, value);
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
			string result = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_PlayerId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_PlayerId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string LastUpdated
	{
		get
		{
			string result = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_LastUpdated_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_LastUpdated_set(swigCPtr, value);
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
			bool result = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_InventoryRefreshRequired_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_InventoryRefreshRequired_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal UserHydratedProgressionTrackResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(UserHydratedProgressionTrackResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(UserHydratedProgressionTrackResponse obj)
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
					MothershipApiPINVOKE.delete_UserHydratedProgressionTrackResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public static UserHydratedProgressionTrackResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		UserHydratedProgressionTrackResponse result = ((intPtr == IntPtr.Zero) ? null : new UserHydratedProgressionTrackResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override bool ParseFromResponseString(string string_)
	{
		bool result = MothershipApiPINVOKE.UserHydratedProgressionTrackResponse_ParseFromResponseString(swigCPtr, string_);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public UserHydratedProgressionTrackResponse()
		: this(MothershipApiPINVOKE.new_UserHydratedProgressionTrackResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

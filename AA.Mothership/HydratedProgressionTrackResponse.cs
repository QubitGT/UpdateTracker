using System;
using System.Runtime.InteropServices;

public class HydratedProgressionTrackResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public ProgressionTrack Track
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.HydratedProgressionTrackResponse_Track_get(swigCPtr);
			ProgressionTrack result = ((intPtr == IntPtr.Zero) ? null : new ProgressionTrack(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.HydratedProgressionTrackResponse_Track_set(swigCPtr, ProgressionTrack.getCPtr(value));
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
			IntPtr intPtr = MothershipApiPINVOKE.HydratedProgressionTrackResponse_Triggers_get(swigCPtr);
			TrackTriggerVector result = ((intPtr == IntPtr.Zero) ? null : new TrackTriggerVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.HydratedProgressionTrackResponse_Triggers_set(swigCPtr, TrackTriggerVector.getCPtr(value));
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
			IntPtr intPtr = MothershipApiPINVOKE.HydratedProgressionTrackResponse_Levels_get(swigCPtr);
			TrackLevelVector result = ((intPtr == IntPtr.Zero) ? null : new TrackLevelVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.HydratedProgressionTrackResponse_Levels_set(swigCPtr, TrackLevelVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal HydratedProgressionTrackResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.HydratedProgressionTrackResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(HydratedProgressionTrackResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(HydratedProgressionTrackResponse obj)
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
					MothershipApiPINVOKE.delete_HydratedProgressionTrackResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.HydratedProgressionTrackResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static HydratedProgressionTrackResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.HydratedProgressionTrackResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		HydratedProgressionTrackResponse result = ((intPtr == IntPtr.Zero) ? null : new HydratedProgressionTrackResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public HydratedProgressionTrackResponse()
		: this(MothershipApiPINVOKE.new_HydratedProgressionTrackResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

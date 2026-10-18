using System;
using System.Runtime.InteropServices;

public class MatchmakingConnectionInformation : GameSessionConnectionInformation
{
	private HandleRef swigCPtr;

	public static readonly string matched_player_sessions_name = MothershipApiPINVOKE.MatchmakingConnectionInformation_matched_player_sessions_name_get();

	public MatchedPlayerSessionMap matched_player_sessions
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.MatchmakingConnectionInformation_matched_player_sessions_get(swigCPtr);
			MatchedPlayerSessionMap result = ((intPtr == IntPtr.Zero) ? null : new MatchedPlayerSessionMap(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MatchmakingConnectionInformation_matched_player_sessions_set(swigCPtr, MatchedPlayerSessionMap.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal MatchmakingConnectionInformation(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.MatchmakingConnectionInformation_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MatchmakingConnectionInformation obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MatchmakingConnectionInformation obj)
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
					MothershipApiPINVOKE.delete_MatchmakingConnectionInformation(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public bool ParseFromJson(SWIGTYPE_p_rapidjson__GenericObjectT_false_rapidjson__Value_t object_)
	{
		bool result = MothershipApiPINVOKE.MatchmakingConnectionInformation_ParseFromJson(swigCPtr, SWIGTYPE_p_rapidjson__GenericObjectT_false_rapidjson__Value_t.getCPtr(object_));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public MatchmakingConnectionInformation()
		: this(MothershipApiPINVOKE.new_MatchmakingConnectionInformation(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

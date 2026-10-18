using System;
using System.Runtime.InteropServices;

public class MatchmakingTicket : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public static readonly string ticket_id_name = MothershipApiPINVOKE.MatchmakingTicket_ticket_id_name_get();

	public static readonly string status_name = MothershipApiPINVOKE.MatchmakingTicket_status_name_get();

	public static readonly string start_time_name = MothershipApiPINVOKE.MatchmakingTicket_start_time_name_get();

	public static readonly string estimated_wait_time_name = MothershipApiPINVOKE.MatchmakingTicket_estimated_wait_time_name_get();

	public static readonly string connection_information_name = MothershipApiPINVOKE.MatchmakingTicket_connection_information_name_get();

	public static readonly string players_name = MothershipApiPINVOKE.MatchmakingTicket_players_name_get();

	public string ticket_id
	{
		get
		{
			string result = MothershipApiPINVOKE.MatchmakingTicket_ticket_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MatchmakingTicket_ticket_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string status
	{
		get
		{
			string result = MothershipApiPINVOKE.MatchmakingTicket_status_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MatchmakingTicket_status_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string start_time
	{
		get
		{
			string result = MothershipApiPINVOKE.MatchmakingTicket_start_time_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MatchmakingTicket_start_time_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public int estimated_wait_time
	{
		get
		{
			int result = MothershipApiPINVOKE.MatchmakingTicket_estimated_wait_time_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MatchmakingTicket_estimated_wait_time_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public MatchmakingConnectionInformation connection_information
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.MatchmakingTicket_connection_information_get(swigCPtr);
			MatchmakingConnectionInformation result = ((intPtr == IntPtr.Zero) ? null : new MatchmakingConnectionInformation(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MatchmakingTicket_connection_information_set(swigCPtr, MatchmakingConnectionInformation.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public MatchmakingTicketPlayerMap players
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.MatchmakingTicket_players_get(swigCPtr);
			MatchmakingTicketPlayerMap result = ((intPtr == IntPtr.Zero) ? null : new MatchmakingTicketPlayerMap(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.MatchmakingTicket_players_set(swigCPtr, MatchmakingTicketPlayerMap.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal MatchmakingTicket(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MatchmakingTicket obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MatchmakingTicket obj)
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

	~MatchmakingTicket()
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
					MothershipApiPINVOKE.delete_MatchmakingTicket(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public bool ParseFromJson(SWIGTYPE_p_rapidjson__GenericObjectT_false_rapidjson__Value_t object_)
	{
		bool result = MothershipApiPINVOKE.MatchmakingTicket_ParseFromJson(swigCPtr, SWIGTYPE_p_rapidjson__GenericObjectT_false_rapidjson__Value_t.getCPtr(object_));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public MatchmakingTicket()
		: this(MothershipApiPINVOKE.new_MatchmakingTicket(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

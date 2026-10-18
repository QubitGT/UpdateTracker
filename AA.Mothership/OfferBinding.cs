using System;
using System.Runtime.InteropServices;

public class OfferBinding : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public static readonly string offer_binding_id_name = MothershipApiPINVOKE.OfferBinding_offer_binding_id_name_get();

	public static readonly string title_id_name = MothershipApiPINVOKE.OfferBinding_title_id_name_get();

	public static readonly string env_id_name = MothershipApiPINVOKE.OfferBinding_env_id_name_get();

	public static readonly string deployment_id_name = MothershipApiPINVOKE.OfferBinding_deployment_id_name_get();

	public static readonly string offer_display_id_name = MothershipApiPINVOKE.OfferBinding_offer_display_id_name_get();

	public static readonly string offer_id_name = MothershipApiPINVOKE.OfferBinding_offer_id_name_get();

	public static readonly string committed_name = MothershipApiPINVOKE.OfferBinding_committed_name_get();

	public static readonly string display_index_name = MothershipApiPINVOKE.OfferBinding_display_index_name_get();

	public string offer_binding_id
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferBinding_offer_binding_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferBinding_offer_binding_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string title_id
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferBinding_title_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferBinding_title_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string env_id
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferBinding_env_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferBinding_env_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string deployment_id
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferBinding_deployment_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferBinding_deployment_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string offer_display_id
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferBinding_offer_display_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferBinding_offer_display_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string offer_id
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferBinding_offer_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferBinding_offer_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public bool committed
	{
		get
		{
			bool result = MothershipApiPINVOKE.OfferBinding_committed_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferBinding_committed_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public int display_index
	{
		get
		{
			int result = MothershipApiPINVOKE.OfferBinding_display_index_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferBinding_display_index_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal OfferBinding(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(OfferBinding obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(OfferBinding obj)
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

	~OfferBinding()
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
					MothershipApiPINVOKE.delete_OfferBinding(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public bool ParseFromJson(SWIGTYPE_p_rapidjson__GenericObjectT_false_rapidjson__Value_t object_)
	{
		bool result = MothershipApiPINVOKE.OfferBinding_ParseFromJson(swigCPtr, SWIGTYPE_p_rapidjson__GenericObjectT_false_rapidjson__Value_t.getCPtr(object_));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public OfferBinding()
		: this(MothershipApiPINVOKE.new_OfferBinding(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

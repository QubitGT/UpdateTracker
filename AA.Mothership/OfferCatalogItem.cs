using System;
using System.Runtime.InteropServices;

public class OfferCatalogItem : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public static readonly string offer_id_name = MothershipApiPINVOKE.OfferCatalogItem_offer_id_name_get();

	public static readonly string title_id_name = MothershipApiPINVOKE.OfferCatalogItem_title_id_name_get();

	public static readonly string env_id_name = MothershipApiPINVOKE.OfferCatalogItem_env_id_name_get();

	public static readonly string name_name = MothershipApiPINVOKE.OfferCatalogItem_name_name_get();

	public static readonly string transaction_id_name = MothershipApiPINVOKE.OfferCatalogItem_transaction_id_name_get();

	public static readonly string bundle_pricing_name = MothershipApiPINVOKE.OfferCatalogItem_bundle_pricing_name_get();

	public static readonly string discount_percent_name = MothershipApiPINVOKE.OfferCatalogItem_discount_percent_name_get();

	public static readonly string created_time_name = MothershipApiPINVOKE.OfferCatalogItem_created_time_name_get();

	public static readonly string last_updated_time_name = MothershipApiPINVOKE.OfferCatalogItem_last_updated_time_name_get();

	public static readonly string subscription_catalog_item_id_name = MothershipApiPINVOKE.OfferCatalogItem_subscription_catalog_item_id_name_get();

	public static readonly string sunset_name = MothershipApiPINVOKE.OfferCatalogItem_sunset_name_get();

	public string offer_id
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferCatalogItem_offer_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_offer_id_set(swigCPtr, value);
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
			string result = MothershipApiPINVOKE.OfferCatalogItem_title_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_title_id_set(swigCPtr, value);
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
			string result = MothershipApiPINVOKE.OfferCatalogItem_env_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_env_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string name
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferCatalogItem_name_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_name_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string transaction_id
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferCatalogItem_transaction_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_transaction_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public OfferEntitlementMap bundle_pricing
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.OfferCatalogItem_bundle_pricing_get(swigCPtr);
			OfferEntitlementMap result = ((intPtr == IntPtr.Zero) ? null : new OfferEntitlementMap(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_bundle_pricing_set(swigCPtr, OfferEntitlementMap.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public int discount_percent
	{
		get
		{
			int result = MothershipApiPINVOKE.OfferCatalogItem_discount_percent_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_discount_percent_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string created_time
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferCatalogItem_created_time_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_created_time_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string last_updated_time
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferCatalogItem_last_updated_time_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_last_updated_time_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string subscription_catalog_item_id
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferCatalogItem_subscription_catalog_item_id_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_subscription_catalog_item_id_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public bool sunset
	{
		get
		{
			bool result = MothershipApiPINVOKE.OfferCatalogItem_sunset_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_sunset_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string display_name
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferCatalogItem_display_name_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_display_name_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string display_description
	{
		get
		{
			string result = MothershipApiPINVOKE.OfferCatalogItem_display_description_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.OfferCatalogItem_display_description_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal OfferCatalogItem(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(OfferCatalogItem obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(OfferCatalogItem obj)
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

	~OfferCatalogItem()
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
					MothershipApiPINVOKE.delete_OfferCatalogItem(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public bool ParseFromJson(SWIGTYPE_p_rapidjson__GenericObjectT_false_rapidjson__Value_t object_)
	{
		bool result = MothershipApiPINVOKE.OfferCatalogItem_ParseFromJson(swigCPtr, SWIGTYPE_p_rapidjson__GenericObjectT_false_rapidjson__Value_t.getCPtr(object_));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public OfferCatalogItem()
		: this(MothershipApiPINVOKE.new_OfferCatalogItem(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

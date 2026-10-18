using System;
using System.Runtime.InteropServices;

public class SubscriptionPricingAndTerms : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public int PriceInUSDCents
	{
		get
		{
			int result = MothershipApiPINVOKE.SubscriptionPricingAndTerms_PriceInUSDCents_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.SubscriptionPricingAndTerms_PriceInUSDCents_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public int SubscriptionBillingFrequency
	{
		get
		{
			int result = MothershipApiPINVOKE.SubscriptionPricingAndTerms_SubscriptionBillingFrequency_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.SubscriptionPricingAndTerms_SubscriptionBillingFrequency_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string SubscriptionBillingFrequencyUnit
	{
		get
		{
			string result = MothershipApiPINVOKE.SubscriptionPricingAndTerms_SubscriptionBillingFrequencyUnit_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.SubscriptionPricingAndTerms_SubscriptionBillingFrequencyUnit_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal SubscriptionPricingAndTerms(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(SubscriptionPricingAndTerms obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(SubscriptionPricingAndTerms obj)
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

	~SubscriptionPricingAndTerms()
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
					MothershipApiPINVOKE.delete_SubscriptionPricingAndTerms(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public bool ParseFromString(string string_)
	{
		bool result = MothershipApiPINVOKE.SubscriptionPricingAndTerms_ParseFromString(swigCPtr, string_);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ParseFromJson(SWIGTYPE_p_rapidjson__GenericObjectT_false_rapidjson__Value_t object_)
	{
		bool result = MothershipApiPINVOKE.SubscriptionPricingAndTerms_ParseFromJson(swigCPtr, SWIGTYPE_p_rapidjson__GenericObjectT_false_rapidjson__Value_t.getCPtr(object_));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public SubscriptionPricingAndTerms()
		: this(MothershipApiPINVOKE.new_SubscriptionPricingAndTerms(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

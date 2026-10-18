using System;
using System.Runtime.InteropServices;

public class CreateAccountAssociationResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public AccountAssociationVector Results
	{
		get
		{
			IntPtr intPtr = MothershipApiPINVOKE.CreateAccountAssociationResponse_Results_get(swigCPtr);
			AccountAssociationVector result = ((intPtr == IntPtr.Zero) ? null : new AccountAssociationVector(intPtr, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.CreateAccountAssociationResponse_Results_set(swigCPtr, AccountAssociationVector.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal CreateAccountAssociationResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.CreateAccountAssociationResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(CreateAccountAssociationResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(CreateAccountAssociationResponse obj)
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
					MothershipApiPINVOKE.delete_CreateAccountAssociationResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool result = MothershipApiPINVOKE.CreateAccountAssociationResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static CreateAccountAssociationResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.CreateAccountAssociationResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		CreateAccountAssociationResponse result = ((intPtr == IntPtr.Zero) ? null : new CreateAccountAssociationResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public CreateAccountAssociationResponse()
		: this(MothershipApiPINVOKE.new_CreateAccountAssociationResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

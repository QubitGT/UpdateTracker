using System;
using System.Runtime.InteropServices;

public class GetDeploymentResponse : MothershipResponse
{
	private HandleRef swigCPtr;

	public MothershipTitleEnvDeployment deployment
	{
		get
		{
			IntPtr deploymentResponse_deployment_get = MothershipApiPINVOKE.GetDeploymentResponse_deployment_get(swigCPtr);
			MothershipTitleEnvDeployment result = ((deploymentResponse_deployment_get == IntPtr.Zero) ? null : new MothershipTitleEnvDeployment(deploymentResponse_deployment_get, cMemoryOwn: false));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.GetDeploymentResponse_deployment_set(swigCPtr, MothershipTitleEnvDeployment.getCPtr(value));
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal GetDeploymentResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.GetDeploymentResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(GetDeploymentResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(GetDeploymentResponse obj)
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
					MothershipApiPINVOKE.delete_GetDeploymentResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromResponseString(string response)
	{
		bool deploymentResponse_ParseFromResponseString = MothershipApiPINVOKE.GetDeploymentResponse_ParseFromResponseString(swigCPtr, response);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return deploymentResponse_ParseFromResponseString;
	}

	public static GetDeploymentResponse FromMothershipResponse(MothershipResponse response)
	{
		IntPtr deploymentResponse_FromMothershipResponse = MothershipApiPINVOKE.GetDeploymentResponse_FromMothershipResponse(MothershipResponse.getCPtr(response));
		GetDeploymentResponse result = ((deploymentResponse_FromMothershipResponse == IntPtr.Zero) ? null : new GetDeploymentResponse(deploymentResponse_FromMothershipResponse, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public GetDeploymentResponse()
		: this(MothershipApiPINVOKE.new_GetDeploymentResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

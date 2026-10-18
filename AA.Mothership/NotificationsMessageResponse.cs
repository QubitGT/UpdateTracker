using System;
using System.Runtime.InteropServices;

public class NotificationsMessageResponse : MothershipWebSocketMessage
{
	private HandleRef swigCPtr;

	public string Title
	{
		get
		{
			string result = MothershipApiPINVOKE.NotificationsMessageResponse_Title_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.NotificationsMessageResponse_Title_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string Body
	{
		get
		{
			string result = MothershipApiPINVOKE.NotificationsMessageResponse_Body_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.NotificationsMessageResponse_Body_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public string RecipientId
	{
		get
		{
			string result = MothershipApiPINVOKE.NotificationsMessageResponse_RecipientId_get(swigCPtr);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
			return result;
		}
		set
		{
			MothershipApiPINVOKE.NotificationsMessageResponse_RecipientId_set(swigCPtr, value);
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	internal NotificationsMessageResponse(IntPtr cPtr, bool cMemoryOwn)
		: base(MothershipApiPINVOKE.NotificationsMessageResponse_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(NotificationsMessageResponse obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(NotificationsMessageResponse obj)
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
					MothershipApiPINVOKE.delete_NotificationsMessageResponse(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			base.Dispose(disposing);
		}
	}

	public override bool ParseFromMessageString(string message)
	{
		bool result = MothershipApiPINVOKE.NotificationsMessageResponse_ParseFromMessageString(swigCPtr, message);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static NotificationsMessageResponse FromWebSocketMessage(MothershipWebSocketMessage response)
	{
		IntPtr intPtr = MothershipApiPINVOKE.NotificationsMessageResponse_FromWebSocketMessage(MothershipWebSocketMessage.getCPtr(response));
		NotificationsMessageResponse result = ((intPtr == IntPtr.Zero) ? null : new NotificationsMessageResponse(intPtr, cMemoryOwn: false));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public NotificationsMessageResponse()
		: this(MothershipApiPINVOKE.new_NotificationsMessageResponse(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}

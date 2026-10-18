using System;
using System.Runtime.InteropServices;

public class MothershipApiClient : IDisposable
{
	public class MothershipInflightRequest : IDisposable
	{
		private HandleRef swigCPtr;

		protected bool swigCMemOwn;

		public SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipRequest_t InternalRequest
		{
			get
			{
				IntPtr intPtr = MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_InternalRequest_get(swigCPtr);
				SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipRequest_t result = ((intPtr == IntPtr.Zero) ? null : new SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipRequest_t(intPtr, futureUse: false));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
				return result;
			}
			set
			{
				MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_InternalRequest_set(swigCPtr, SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipRequest_t.getCPtr(value));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
			}
		}

		public SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t HttpRequest
		{
			get
			{
				IntPtr intPtr = MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_HttpRequest_get(swigCPtr);
				SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t result = ((intPtr == IntPtr.Zero) ? null : new SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t(intPtr, futureUse: false));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
				return result;
			}
			set
			{
				MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_HttpRequest_set(swigCPtr, SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipHTTPRequest_t.getCPtr(value));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
			}
		}

		public SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipRequestCompleteDelegateWrapper_t CallbackWrapper
		{
			get
			{
				IntPtr intPtr = MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_CallbackWrapper_get(swigCPtr);
				SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipRequestCompleteDelegateWrapper_t result = ((intPtr == IntPtr.Zero) ? null : new SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipRequestCompleteDelegateWrapper_t(intPtr, futureUse: false));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
				return result;
			}
			set
			{
				MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_CallbackWrapper_set(swigCPtr, SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipRequestCompleteDelegateWrapper_t.getCPtr(value));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
			}
		}

		public SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipResponse_t ResponseInstance
		{
			get
			{
				IntPtr intPtr = MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_ResponseInstance_get(swigCPtr);
				SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipResponse_t result = ((intPtr == IntPtr.Zero) ? null : new SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipResponse_t(intPtr, futureUse: false));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
				return result;
			}
			set
			{
				MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_ResponseInstance_set(swigCPtr, SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipResponse_t.getCPtr(value));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
			}
		}

		public int retryCount
		{
			get
			{
				int result = MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_retryCount_get(swigCPtr);
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
				return result;
			}
			set
			{
				MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_retryCount_set(swigCPtr, value);
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
			}
		}

		public float retryTime
		{
			get
			{
				float result = MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_retryTime_get(swigCPtr);
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
				return result;
			}
			set
			{
				MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_retryTime_set(swigCPtr, value);
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
			}
		}

		public string playerId
		{
			get
			{
				string result = MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_playerId_get(swigCPtr);
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
				return result;
			}
			set
			{
				MothershipApiPINVOKE.MothershipApiClient_MothershipInflightRequest_playerId_set(swigCPtr, value);
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
			}
		}

		internal MothershipInflightRequest(IntPtr cPtr, bool cMemoryOwn)
		{
			swigCMemOwn = cMemoryOwn;
			swigCPtr = new HandleRef(this, cPtr);
		}

		internal static HandleRef getCPtr(MothershipInflightRequest obj)
		{
			return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
		}

		internal static HandleRef swigRelease(MothershipInflightRequest obj)
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

		~MothershipInflightRequest()
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
						MothershipApiPINVOKE.delete_MothershipApiClient_MothershipInflightRequest(swigCPtr);
					}
					swigCPtr = new HandleRef(null, IntPtr.Zero);
				}
			}
		}

		public MothershipInflightRequest()
			: this(MothershipApiPINVOKE.new_MothershipApiClient_MothershipInflightRequest(), cMemoryOwn: true)
		{
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public class MothershipActiveWebSocketInfo : IDisposable
	{
		private HandleRef swigCPtr;

		protected bool swigCMemOwn;

		public WebSocketStatus Status
		{
			get
			{
				int result = MothershipApiPINVOKE.MothershipApiClient_MothershipActiveWebSocketInfo_Status_get(swigCPtr);
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
				return (WebSocketStatus)result;
			}
			set
			{
				MothershipApiPINVOKE.MothershipApiClient_MothershipActiveWebSocketInfo_Status_set(swigCPtr, (int)value);
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
			}
		}

		public SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipOpenWebSocketEventArgs_t InitialRequest
		{
			get
			{
				IntPtr intPtr = MothershipApiPINVOKE.MothershipApiClient_MothershipActiveWebSocketInfo_InitialRequest_get(swigCPtr);
				SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipOpenWebSocketEventArgs_t result = ((intPtr == IntPtr.Zero) ? null : new SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipOpenWebSocketEventArgs_t(intPtr, futureUse: false));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
				return result;
			}
			set
			{
				MothershipApiPINVOKE.MothershipApiClient_MothershipActiveWebSocketInfo_InitialRequest_set(swigCPtr, SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipOpenWebSocketEventArgs_t.getCPtr(value));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
			}
		}

		public SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipWebSocketMessageDelegateWrapper_t CallbackWrapper
		{
			get
			{
				IntPtr intPtr = MothershipApiPINVOKE.MothershipApiClient_MothershipActiveWebSocketInfo_CallbackWrapper_get(swigCPtr);
				SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipWebSocketMessageDelegateWrapper_t result = ((intPtr == IntPtr.Zero) ? null : new SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipWebSocketMessageDelegateWrapper_t(intPtr, futureUse: false));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
				return result;
			}
			set
			{
				MothershipApiPINVOKE.MothershipApiClient_MothershipActiveWebSocketInfo_CallbackWrapper_set(swigCPtr, SWIGTYPE_p_std__shared_ptrT_MothershipApi__MothershipWebSocketMessageDelegateWrapper_t.getCPtr(value));
				if (MothershipApiPINVOKE.SWIGPendingException.Pending)
				{
					throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
				}
			}
		}

		internal MothershipActiveWebSocketInfo(IntPtr cPtr, bool cMemoryOwn)
		{
			swigCMemOwn = cMemoryOwn;
			swigCPtr = new HandleRef(this, cPtr);
		}

		internal static HandleRef getCPtr(MothershipActiveWebSocketInfo obj)
		{
			return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
		}

		internal static HandleRef swigRelease(MothershipActiveWebSocketInfo obj)
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

		~MothershipActiveWebSocketInfo()
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
						MothershipApiPINVOKE.delete_MothershipApiClient_MothershipActiveWebSocketInfo(swigCPtr);
					}
					swigCPtr = new HandleRef(null, IntPtr.Zero);
				}
			}
		}

		public MothershipActiveWebSocketInfo()
			: this(MothershipApiPINVOKE.new_MothershipApiClient_MothershipActiveWebSocketInfo(), cMemoryOwn: true)
		{
			if (MothershipApiPINVOKE.SWIGPendingException.Pending)
			{
				throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
			}
		}
	}

	public enum WebSocketStatus
	{
		INACTIVE,
		ACTIVE,
		CLOSING
	}

	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal MothershipApiClient(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipApiClient obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipApiClient obj)
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

	~MothershipApiClient()
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
					MothershipApiPINVOKE.delete_MothershipApiClient(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public virtual void SetHttpRequestDelegate(MothershipSendHTTPRequestDelegateWrapper inSendRequestDelegate)
	{
		MothershipApiPINVOKE.MothershipApiClient_SetHttpRequestDelegate(swigCPtr, MothershipSendHTTPRequestDelegateWrapper.getCPtr(inSendRequestDelegate));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void SetWebSocketDelegate(MothershipWebSocketDelegateWrapper inWebsocketDelegate)
	{
		MothershipApiPINVOKE.MothershipApiClient_SetWebSocketDelegate(swigCPtr, MothershipWebSocketDelegateWrapper.getCPtr(inWebsocketDelegate));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void ReceiveHttpResponse(MothershipHTTPResponse response)
	{
		MothershipApiPINVOKE.MothershipApiClient_ReceiveHttpResponse(swigCPtr, MothershipHTTPResponse.getCPtr(response));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void ReceiveWebsocketMessage(MothershipWebSocketResponse response)
	{
		MothershipApiPINVOKE.MothershipApiClient_ReceiveWebsocketMessage(swigCPtr, MothershipWebSocketResponse.getCPtr(response));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void Tick(float deltaTimeInSeconds)
	{
		MothershipApiPINVOKE.MothershipApiClient_Tick(swigCPtr, deltaTimeInSeconds);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void SetLogDelegate(MothershipLogDelegateWrapper logDelegate)
	{
		MothershipApiPINVOKE.MothershipApiClient_SetLogDelegate(swigCPtr, MothershipLogDelegateWrapper.getCPtr(logDelegate));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void Log(MothershipLogLevel level, string message)
	{
		MothershipApiPINVOKE.MothershipApiClient_Log(swigCPtr, (int)level, message);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
